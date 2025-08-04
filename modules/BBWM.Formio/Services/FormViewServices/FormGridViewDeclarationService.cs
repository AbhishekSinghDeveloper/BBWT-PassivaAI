using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Membership.Model;
using BBWM.FormIO.Classes;
using BBWM.FormIO.Interfaces.FormViewInterfaces;
using BBWM.FormIO.Models;
using BBWM.FormIO.Models.FormViewModels;
using Microsoft.EntityFrameworkCore;

namespace BBWM.FormIO.Services.FormViewServices;

public class FormGridViewDeclarationService : IFormGridViewDeclarationService
{
    private readonly IDbContext _context;
    private readonly IFormFieldService _formFieldService;
    private readonly IFormViewHelperService _formViewHelperService;

    private record FormViewLevel(int LevelIndex, string RelativePath, string Ordinal);

    public FormGridViewDeclarationService(
        IDbContext context,
        IFormFieldService formFieldService,
        IFormViewHelperService formViewHelperService)
    {
        _context = context;
        _formFieldService = formFieldService;
        _formViewHelperService = formViewHelperService;
    }

    public IList<FormViewColumnItem> GetFormGridViewColumns(string viewName, FormField root)
    {
        // Get form view selection items, corresponding to form fields.
        var columnItems = _formViewHelperService.GetViewColumns(viewName, root);

        // Get form data and organization table names from entity types.
        var dataTableName = _context.Model.FindEntityType(typeof(FormData))?.GetTableName();
        var organizationTableName = _context.Model.FindEntityType(typeof(Organization))?.GetTableName();

        // Get necessary data to declare view foreign keys.
        var dataPrimaryKeyName = _context.Model.FindEntityType(typeof(FormData))
            ?.FindProperty(nameof(FormData.Id))?.GetColumnName();
        var dataOrganizationForeignKeyName = _context.Model.FindEntityType(typeof(FormData))
            ?.FindProperty(nameof(FormData.OrganizationId))?.GetColumnName();

        // If some parameter is missing, throw exception.
        if (dataTableName == null || organizationTableName == null ||
            dataPrimaryKeyName == null || dataOrganizationForeignKeyName == null)
            throw new InvalidOperationException("Cannot create view for this form: some metadata is missing.");

        // Declare view foreign keys.
        var viewFormDataForeignKey = new FormViewColumnReference
        {
            Name = $"{dataTableName}.{dataPrimaryKeyName}",
            ColumnAlias = $"FK_{dataTableName}",
            IsForeignKey = true,
            Type = "INT"
        };

        var viewOrganizationForeignKey = new FormViewColumnReference
        {
            Name = $"{dataTableName}.{dataOrganizationForeignKeyName}",
            ColumnAlias = $"FK_{organizationTableName}",
            IsForeignKey = true,
            Type = "INT"
        };

        var foreignKeys = new List<FormViewColumnReference>
        {
            viewFormDataForeignKey,
            viewOrganizationForeignKey
        };

        // Generate an index for each nesting level.
        var indexes = GetFormViewLevelsInfo(root)
            .Select(level => new FormViewColumnReference
            {
                ColumnAlias = level.Ordinal,
                Name = level.Ordinal,
                Type = "INT"
            }).ToList();

        return columnItems.Concat(foreignKeys).Concat(indexes).ToList();
    }

    private IList<FormViewFiltrationRule> GetFormGridViewFiltrationRules(int definitionId)
    {
        // Get form data and form data table names from entity types.
        var dataTableName = _context.Model.FindEntityType(typeof(FormData))?.GetTableName();

        // Get necessary data to declare view foreign keys.
        var dataDefinitionForeignKeyName = _context.Model.FindEntityType(typeof(FormData))
            ?.FindProperty(nameof(FormData.FormDefinitionId))?.GetColumnName();

        // If some parameter is missing, throw exception.
        if (dataTableName == null || dataDefinitionForeignKeyName == null)
            throw new InvalidOperationException("Cannot create view for this form: some metadata is missing.");

        // Declare view filtration rules.
        var definitionFilterRule = new FormViewFiltrationRule
        {
            ColumnValue = definitionId.ToString(),
            ColumnName = $"{dataTableName}.{dataDefinitionForeignKeyName}"
        };

        return new List<FormViewFiltrationRule> { definitionFilterRule };
    }

    private FormViewDeclaration GetFormGridViewDeclaration(int definitionId, string tableName, string columnName, string viewName, FormField root)
    {
        var columnItems = GetFormGridViewColumns(viewName, root).ToList();
        var filtrationRules = GetFormGridViewFiltrationRules(definitionId).ToList();
        var jsonTableColumns = _formViewHelperService.GetJsonTableColumns(root).ToList();

        var sourceColumn = $"{tableName}.{columnName}";
        var levels = GetFormViewLevelsInfo(root).Reverse().ToList();

        // Declare a JSON_TABLE nesting level for each level of the view.
        var jsonTable = levels.Aggregate<FormViewLevel, FormViewJsonTableDeclaration?>(null,
            (nestedJsonTable, level) => new FormViewJsonTableDeclaration
            {
                Path = $"{level.RelativePath}[*]",
                Alias = viewName,
                ColumnName = sourceColumn,
                NestedDeclaration = nestedJsonTable,
                Ordinal = new FormViewJsonTableOrdinal { Name = level.Ordinal },
                Columns = level.LevelIndex == levels.Count ? jsonTableColumns : new List<FormViewJsonTableColumn>()
            });
        if (jsonTable == null) throw new BusinessException($"Invalid nested view path: {root.Path}.");

        var dataTable = new FormViewTableReference { Name = tableName };
        var tableItems = new List<FormViewTableItem> { dataTable, jsonTable };

        return new FormViewDeclaration
        {
            ViewName = viewName,
            TableItems = tableItems,
            SelectionItems = columnItems,
            FiltrationRules = filtrationRules
        };
    }

    public async Task CreateFormRevisionGridView(int revisionGridId, CancellationToken ct = default)
    {
        var revisionGrid = await _context.Set<FormRevisionGrid>()
            .Where(revision => revision.Id == revisionGridId)
            .Select(revision => new { revision.Json, revision.ViewName, revision.FormDefinitionId, revision.Path })
            .FirstOrDefaultAsync(ct);

        // If there is no revision grid, or no valid view name is related to it, throw exception.
        if (revisionGrid == null) throw new BusinessException("There is no form revision grid with given id.");
        if (string.IsNullOrEmpty(revisionGrid.ViewName)) throw new BusinessException("Invalid form revision grid view name.");

        // Get for view declaration.
        var root = _formFieldService.GetFormField(revisionGrid.Json, revisionGrid.Path)?.Children.First();
        if (root == null) throw new BusinessException("No valid json value found in given form revision grid.");

        // Create form view.
        await CreateFormRevisionGridView(revisionGrid.FormDefinitionId, revisionGrid.ViewName, root, ct);
    }

    public async Task CreateFormRevisionGridView(int definitionId, string viewName, FormField root, CancellationToken ct = default)
    {
        // Get form data and organization table names from entity types.
        var dataTableName = _context.Model.FindEntityType(typeof(FormData))?.GetTableName();

        // Get necessary data to declare view foreign keys.
        var dataJsonColumnName = _context.Model.FindEntityType(typeof(FormData))
            ?.FindProperty(nameof(FormData.Json))?.GetColumnName();

        // If some parameter is missing, throw exception.
        if (dataTableName == null || dataJsonColumnName == null)
            throw new InvalidOperationException("Cannot create view for this form: some metadata is missing.");

        // Get for view declaration.
        var declaration = GetFormGridViewDeclaration(definitionId, dataTableName, dataJsonColumnName, viewName, root);

        if (declaration.Sql is not { Length: > 0 } query)
            throw new BusinessException("Invalid form revision json: cannot declare corresponding view.");

        // Create form view.
        await _formViewHelperService.CreateView(query, ct);
    }

    public async Task DeleteFormRevisionView(int revisionId, CancellationToken ct = default)
    {
        var viewName = await _context.Set<FormRevision>()
            .Where(revision => revision.Id == revisionId)
            .Select(revision => revision.FormDefinition!.ViewName)
            .FirstOrDefaultAsync(ct);
        if (viewName == null) return;

        await _formViewHelperService.DeleteView(viewName, ct);
    }

    private static IEnumerable<FormViewLevel> GetFormViewLevelsInfo(FormField root)
    {
        // Get path segments.
        var paths = (root.Path + root.ChildrenPath).Split("[*]", StringSplitOptions.RemoveEmptyEntries);

        // Generate an index for each nesting level.
        return paths.Select((path, i) => new FormViewLevel(i + 1, path, $"RowIndex_Level_{i + 1}"));
    }
}