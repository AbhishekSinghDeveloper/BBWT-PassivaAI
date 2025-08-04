using System.Globalization;
using BBWM.Core.Exceptions;
using BBWM.Core.Services;
using BBWM.FormIO.Classes;
using BBWM.FormIO.DTO.FormViewDTOs;
using BBWM.FormIO.Interfaces.FormViewInterfaces;
using BBWM.FormIO.Models;
using BBWM.FormIO.Models.FormViewModels;

namespace BBWM.FormIO.Services.FormViewServices;

public class FormRevisionGridService : IFormRevisionGridService
{
    private const string FieldType = "dataGrid";

    private readonly IDataService _dataService;
    private readonly IFormFieldService _formFieldService;
    private readonly IFormViewHelperService _formViewHelperService;

    public FormRevisionGridService(
        IDataService dataService,
        IFormFieldService formFieldService,
        IFormViewHelperService formViewHelperService)
    {
        _dataService = dataService;
        _formFieldService = formFieldService;
        _formViewHelperService = formViewHelperService;
    }

    public async Task<FormRevisionGridUpdate> UpdateRevisionGrids(int definitionId, CancellationToken ct = default)
    {
        var definition = _dataService.Context.Set<FormDefinition>()
            .Where(definition => definition.Id == definitionId)
            .Select(definition => new
            {
                definition.ViewName,
                ActiveRevision = definition.FormRevisions
                    .Where(revision => revision.Id == definition.ActiveRevisionId)
                    .Select(revision => new { revision.Id, revision.Json })
                    .FirstOrDefault()
            }).FirstOrDefault();

        if (definition == null) throw new BusinessException("Cannot find form definition with given id.");
        if (definition.ActiveRevision == null) throw new BusinessException("Form definition with given id doesn't have active revision.");
        if (string.IsNullOrEmpty(definition.ViewName)) throw new BusinessException("Form definition with given id doesn't have a valid view name.");

        // If form field root is not passed, get it from form definition json.
        var root = _formFieldService.GetFormField(definition.ActiveRevision.Json);
        if (root == null) throw new BusinessException("No valid json value found in active revision of given form definition.");

        return await UpdateRevisionGrids(definitionId, definition.ViewName, root, ct);
    }

    public async Task<FormRevisionGridUpdate> UpdateRevisionGrids(int definitionId, string viewName, FormField root, CancellationToken ct = default)
    {
        // Revision update report to register changes.
        var revisionUpdate = new FormRevisionGridUpdate();

        // Get all grids related to this form definition.
        var grids = (await _dataService.GetAll<FormRevisionGrid, FormRevisionGridDTO>(
                query => query.Where(grid => grid.FormDefinitionId == definitionId), ct))
            .ToDictionary(grid => grid.Path, grid => grid, StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.IgnoreCase));

        // Create or update form revision grids using given form field tree.
        await UpdateRevisionGrids(definitionId, viewName, root, revisionUpdate, grids, ct: ct);

        // Delete unused form revisions grid corresponding to given form field tree.
        await DeleteRevisionGrids(root, revisionUpdate, grids, ct);

        return revisionUpdate;
    }

    private async Task UpdateRevisionGrids(int definitionId, string? viewName, FormField field, FormRevisionGridUpdate revisionUpdate,
        IDictionary<string, FormRevisionGridDTO> grids, FormRevisionGridDTO? parentGrid = null, CancellationToken ct = default)
    {
        // If this form field is, in fact, a data grid.
        if (string.Equals(field.Type, FieldType, StringComparison.InvariantCultureIgnoreCase))
        {
            // Search for it in existent grids. If exists, update the grid with the new values.
            if (grids.TryGetValue(field.Path, out var grid))
            {
                grid.Name = field.Label;
                grid.Json = GetGridFieldDefinition(field);
                grid.ParentFormRevisionGridId = parentGrid?.Id;
                grid = await _dataService.Update<FormRevisionGrid, FormRevisionGridDTO>(grid, ct);
                revisionUpdate.Updated.Add(grid);
            }
            // Otherwise, create a new grid from scratch.
            else
            {
                grid = new FormRevisionGridDTO
                {
                    Path = field.Path,
                    Name = field.Label,
                    Json = GetGridFieldDefinition(field),
                    FormDefinitionId = definitionId,
                    ParentFormRevisionGridId = parentGrid?.Id,
                    ViewName = await _formViewHelperService.GetFormUniqueViewName($"{viewName}.{field.Key}", ct)
                };
                grid = await _dataService.Create<FormRevisionGrid, FormRevisionGridDTO>(grid, ct);
                revisionUpdate.Created.Add(grid);
            }

            // Update parent grid with this grid to call recursively.
            parentGrid = grid;
        }

        // Continue with child form fields.
        foreach (var child in field.Children)
            await UpdateRevisionGrids(definitionId, viewName, child, revisionUpdate, grids, parentGrid, ct);
    }

    private async Task DeleteRevisionGrids(FormField root, FormRevisionGridUpdate revisionUpdate,
        IDictionary<string, FormRevisionGridDTO> grids, CancellationToken ct = default)
    {
        // Get only data grid fields.
        var fields = root.Fields
            .Where(field => string.Equals(field.Type, FieldType, StringComparison.InvariantCultureIgnoreCase))
            .ToDictionary(field => field.Path, field => field, StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.IgnoreCase));

        // Get unused grids from grid dtos (existent grid whose path doesn't match with any grid field).
        var gridsDeleted = grids.Values.Where(grid => !fields.ContainsKey(grid.Path)).Reverse().ToList();

        while (gridsDeleted.Count > 0)
        {
            // Get the first grid which is not parent of any other grid in the list.
            var grid = gridsDeleted.FirstOrDefault(grid => gridsDeleted.All(childGrid => childGrid.ParentFormRevisionGridId != grid.Id));
            // If there is no one, break the loop to avoid infinite cycling.
            if (grid == null) return;

            // Delete the grid.
            await _dataService.Delete<FormRevisionGrid>(grid.Id, ct);
            revisionUpdate.Deleted.Add(grid);

            // Update the list to be able to delete the parents of this grid.
            gridsDeleted.Remove(grid);
        }
    }

    // Get json definition corresponding to current form revision grid.
    private static string GetGridFieldDefinition(FormField gridField)
        => $"{{\"components\":{gridField.Token.SelectToken("components") ?? "[]"}}}";
}