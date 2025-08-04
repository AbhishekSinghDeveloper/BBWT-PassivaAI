using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Membership.Model;
using BBWM.FormIO.Classes;
using BBWM.FormIO.Interfaces.FormViewInterfaces;
using BBWM.FormIO.Models;
using Microsoft.EntityFrameworkCore;

namespace BBWM.FormIO.Services.FormViewServices;

public class FormViewDeclarationService : IFormViewDeclarationService
{
    private readonly IDbContext _context;
    private readonly IFormFieldService _formFieldService;
    private readonly IFormViewHelperService _formViewHelperService;

    public FormViewDeclarationService(
        IDbContext context,
        IFormFieldService formFieldService,
        IFormViewHelperService formViewHelperService)
    {
        _context = context;
        _formFieldService = formFieldService;
        _formViewHelperService = formViewHelperService;
    }

    public IList<FormViewColumnItem> GetFormViewColumns(string viewName, FormField root)
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

        return columnItems.Concat(foreignKeys).ToList();
    }

    private IList<FormViewFiltrationRule> GetFormRevisionViewFiltrationRules(int definitionId)
    {
        // Get form data and organization table names from entity types.
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

    private FormViewDeclaration GetFormViewDeclaration(int definitionId, string tableName, string columnName, string viewName, FormField root)
    {
        var columnItems = GetFormViewColumns(viewName, root).ToList();
        var filtrationRules = GetFormRevisionViewFiltrationRules(definitionId).ToList();
        var jsonTableColumns = _formViewHelperService.GetJsonTableColumns(root).ToList();

        var table = new FormViewTableReference { Name = tableName };

        var jsonTable = new FormViewJsonTableDeclaration
        {
            Path = root.Path,
            Alias = viewName,
            Columns = jsonTableColumns,
            ColumnName = $"{tableName}.{columnName}"
        };

        var tableItems = new List<FormViewTableItem> { table, jsonTable };

        return new FormViewDeclaration
        {
            ViewName = viewName,
            TableItems = tableItems,
            SelectionItems = columnItems,
            FiltrationRules = filtrationRules
        };
    }

    public async Task CreateFormRevisionView(int revisionId, CancellationToken ct = default)
    {
        var revision = await _context.Set<FormRevision>()
            .Where(revision => revision.Id == revisionId)
            .Select(revision => new { revision.Json, revision.FormDefinition!.ViewName, revision.FormDefinitionId })
            .FirstOrDefaultAsync(ct);

        // If there is no revision, or no valid view name is related to it, throw exception.
        if (revision == null) throw new BusinessException("There is no form revision with given id.");
        if (revision.FormDefinitionId == null) throw new BusinessException("This form revision doesn't belong to any form definition.");
        if (string.IsNullOrEmpty(revision.ViewName)) throw new BusinessException("Invalid form definition view name.");

        // Get for view declaration.
        var root = _formFieldService.GetFormField(revision.Json, ".data");
        if (root == null) throw new BusinessException("No valid json value found in given form revision.");

        // Create form view.
        await CreateFormRevisionView(revision.FormDefinitionId.Value, revision.ViewName, root, ct);
    }

    public async Task CreateFormRevisionView(int definitionId, string viewName, FormField root, CancellationToken ct = default)
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
        var declaration = GetFormViewDeclaration(definitionId, dataTableName, dataJsonColumnName, viewName, root);

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
}