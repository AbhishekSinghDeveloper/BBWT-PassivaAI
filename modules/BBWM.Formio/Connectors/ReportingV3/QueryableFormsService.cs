using BBWM.Core.Services;
using BBWM.FormIO.Classes;
using BBWM.FormIO.DTO.FormViewDTOs;
using BBWM.FormIO.Models;
using BBWM.FormIO.Interfaces.FormViewInterfaces;
using BBWM.FormIO.Models.FormViewModels;

namespace BBWM.FormIO.Connectors.ReportingV3;

public class QueryableFormsService : IQueryableFormsService
{
    private readonly IDataService _dataService;
    private readonly IFormFieldService _formFieldService;
    private readonly IFormViewDeclarationService _formViewDeclarationService;
    private readonly IFormGridViewDeclarationService _formGridViewDeclarationService;

    public QueryableFormsService(
        IDataService dataService,
        IFormFieldService formFieldService,
        IFormViewDeclarationService formViewDeclarationService,
        IFormGridViewDeclarationService formGridViewDeclarationService)
    {
        _dataService = dataService;
        _formFieldService = formFieldService;
        _formViewDeclarationService = formViewDeclarationService;
        _formGridViewDeclarationService = formGridViewDeclarationService;
    }

    public async Task<IEnumerable<QueryableForm>> GetQueryableForms(bool includeColumns, bool includeChildren, CancellationToken ct = default)
    {
        // Get all form definitions.
        var formDefinitions = (await _dataService.GetAll<FormDefinition, FormDefinitionViewDTO>(ct)).ToList();

        // If it is necessary to include columns, get all active form revisions.
        if (includeColumns)
        {
            // Get all active form revisions.
            var activeRevisionIds = formDefinitions.Select(definition => definition.ActiveRevisionId).ToList();
            var formRevisions = (await _dataService.GetAll<FormRevision, FormRevisionViewDTO>(
                query => query.Where(revision => activeRevisionIds.Contains(revision.Id)), ct)).ToList();

            // Assign active revision to corresponding form definition.
            foreach (var definition in formDefinitions)
                definition.ActiveRevision = formRevisions.FirstOrDefault(revision => revision.Id == definition.ActiveRevisionId);
        }

        // If it is necessary to include children, get all form revision grids.
        if (includeChildren)
        {
            // Get all form revision grids.
            var formRevisionGrids = (await _dataService.GetAll<FormRevisionGrid, FormRevisionGridDTO>(ct)).ToList();

            // Assign revision grids to corresponding form definition.
            foreach (var definition in formDefinitions)
                definition.FormRevisionGrids = formRevisionGrids.Where(revisionGrid => revisionGrid.FormDefinitionId == definition.Id).ToList();
        }

        // Get the queryable version of the form definitions.
        var queryableForms = formDefinitions
            .Select(definition => ToQueryableForm(definition, includeColumns, includeChildren))
            .Where(form => form != null);

        return queryableForms!;
    }

    public async Task<QueryableForm?> GetQueryableForm(string formId, string? parentFormId, CancellationToken ct = default)
    {
        if (parentFormId != null)
        {
            // Get form revision grid corresponding to this id if exists.
            var formRevisionGrid = await _dataService.Get<FormRevisionGrid, FormRevisionGridDTO>(int.Parse(formId), ct);
            if (formRevisionGrid == null) return null;

            // Get the queryable version of this form revision grid.
            var queryableForm = ToQueryableForm(formRevisionGrid, true);
            if (queryableForm == null) return null;

            queryableForm.ParentFormId = parentFormId;
            return queryableForm;
        }

        // Get form definition corresponding to this id if exists.
        var formDefinition = await _dataService.Get<FormDefinition, FormDefinitionViewDTO>(int.Parse(formId), ct);
        if (formDefinition == null) return null;

        // Get active revision corresponding to this form definition.
        formDefinition.ActiveRevision = await _dataService.Get<FormRevision, FormRevisionViewDTO>(formDefinition.ActiveRevisionId, ct);

        // Get form revision grids corresponding to this form definition.
        formDefinition.FormRevisionGrids = (await _dataService.GetAll<FormRevisionGrid, FormRevisionGridDTO>(
            query => query.Where(grid => grid.FormDefinitionId == formDefinition.Id), ct)).ToList();

        // Get the queryable version of this form definition.
        return ToQueryableForm(formDefinition, true, true);
    }

    private QueryableForm? ToQueryableForm(FormDefinitionViewDTO definition, bool includeColumns, bool includeChildren)
    {
        var viewName = definition.ViewName;
        if (string.IsNullOrEmpty(viewName)) return null;

        var json = definition.ActiveRevision?.Json ?? "";
        var name = string.IsNullOrEmpty(definition.Name) ? definition.ViewName! : definition.Name;

        var queryableForm = new QueryableForm
        {
            Id = definition.Id.ToString(),
            FormName = name,
            TableAlias = viewName,
            Children = new List<QueryableForm>(),
            Columns = new List<QueryableFormColumn>(),
        };

        // Include children forms only if it is necessary.
        // Don't include children descendents nor children columns.
        if (includeChildren)
        {
            queryableForm.Children = definition.FormRevisionGrids
                .Select(grid => ToQueryableForm(grid, false))
                .Where(form => form != null).ToList()!;
        }

        // Include columns only if it is necessary.
        if (!includeColumns) return queryableForm;

        // Get valid components from JSON.
        var field = _formFieldService.GetFormField(json, ".data");
        if (field == null) return queryableForm;

        queryableForm.Columns = _formViewDeclarationService
            .GetFormViewColumns(viewName, field)
            .Select(ToQueryableFormColumn).ToList();

        return queryableForm;
    }

    private QueryableForm? ToQueryableForm(FormRevisionGridDTO revisionGrid, bool includeColumns)
    {
        var viewName = revisionGrid.ViewName;
        if (string.IsNullOrEmpty(viewName)) return null;

        var json = revisionGrid.Json;
        var name = string.IsNullOrEmpty(revisionGrid.Name) ? revisionGrid.ViewName : revisionGrid.Name;

        var queryableForm = new QueryableForm
        {
            Id = revisionGrid.Id.ToString(),
            FormName = name,
            TableAlias = viewName,
            Columns = new List<QueryableFormColumn>()
        };

        // Include columns only if it is necessary.
        if (!includeColumns) return queryableForm;

        // Get valid components from JSON.
        var field = _formFieldService.GetFormField(json, revisionGrid.Path);
        if (field == null) return queryableForm;

        queryableForm.Columns = _formGridViewDeclarationService
            .GetFormGridViewColumns(viewName, field)
            .Select(ToQueryableFormColumn).ToList();

        return queryableForm;
    }

    private static QueryableFormColumn ToQueryableFormColumn(FormViewColumnItem column)
    {
        return new QueryableFormColumn
        {
            Id = column.Alias,
            Type = column.Type,
            ColumnAlias = column.Alias,
            IsPrimaryKey = column.IsPrimaryKey,
            IsForeignKey = column.IsForeignKey,
            FormName = column.FormLabel ?? column.Alias,
        };
    }
}