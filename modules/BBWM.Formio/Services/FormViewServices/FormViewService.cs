using System.Globalization;
using BBWM.Core.Exceptions;
using BBWM.Core.Services;
using BBWM.FormIO.Interfaces.FormViewInterfaces;
using BBWM.FormIO.Models;
using BBWM.FormIO.Models.FormViewModels;
using Microsoft.EntityFrameworkCore;

namespace BBWM.FormIO.Services.FormViewServices;

public class FormViewService : IFormViewService
{
    private readonly IDataService _dataService;
    private readonly IFormFieldService _formFieldService;
    private readonly IFormViewHelperService _formViewHelperService;
    private readonly IFormRevisionGridService _formRevisionGridService;
    private readonly IFormViewDeclarationService _formViewDeclarationService;
    private readonly IFormGridViewDeclarationService _formGridViewDeclarationService;

    public FormViewService(
        IDataService dataService,
        IFormFieldService formFieldService,
        IFormViewHelperService formViewHelperService,
        IFormRevisionGridService formRevisionGridService,
        IFormViewDeclarationService formViewDeclarationService,
        IFormGridViewDeclarationService formGridViewDeclarationService)
    {
        _dataService = dataService;
        _formFieldService = formFieldService;
        _formViewHelperService = formViewHelperService;
        _formRevisionGridService = formRevisionGridService;
        _formViewDeclarationService = formViewDeclarationService;
        _formGridViewDeclarationService = formGridViewDeclarationService;
    }

    public async Task UpdateViewRelatedData(int definitionId, CancellationToken ct = default)
    {
        // Get form revision info related to given form definition id.
        var definition = _dataService.Context.Set<FormDefinition>()
            .Where(definition => definition.Id == definitionId)
            .Select(definition => new
            {
                definition.Name,
                definition.ViewName,
                ActiveRevision = definition.FormRevisions
                    .Where(revision => revision.Id == definition.ActiveRevisionId)
                    .Select(revision => new { revision.Id, revision.Json })
                    .FirstOrDefault()
            }).FirstOrDefault();

        if (definition == null) throw new BusinessException("Cannot find form definition with given id.");
        if (definition.ActiveRevision == null) throw new BusinessException("Definition with given id doesn't have active revision.");

        // If this form definition has no valid view name, assign it one.
        var viewName = definition.ViewName;
        if (string.IsNullOrEmpty(definition.ViewName))
        {
            viewName = await _formViewHelperService.GetFormUniqueViewName(definition.Name, ct);
            await _dataService.Context.Set<FormDefinition>()
                .Where(formDefinition => formDefinition.Id == definitionId)
                .UpdateFromQueryAsync(_ => new { ViewName = viewName }, ct);
        }

        var root = _formFieldService.GetFormField(definition.ActiveRevision.Json, ".data");
        if (root == null) throw new BusinessException("Invalid form revision json.");

        // Update form revision grids.
        var revisionUpdate = await _formRevisionGridService.UpdateRevisionGrids(definitionId, viewName!, root, ct);

        // Create the view related to this form revision.
        await _formViewDeclarationService.CreateFormRevisionView(definitionId, viewName!, root, ct);

        // Flatten the form fields tree, and convert it in a dictionary of paths to facilitate searches.
        var fields = root.Fields.ToDictionary(field => field.Path, field => field,
            StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.IgnoreCase));

        // Create the views related to this form revision grids.
        foreach (var grid in revisionUpdate.Created.Concat(revisionUpdate.Updated))
        {
            if (!fields.TryGetValue(grid.Path, out var field))
                throw new BusinessException("Invalid form revision json.");

            // Create corresponding form revision grid view.
            await _formGridViewDeclarationService.CreateFormRevisionGridView(definitionId, grid.ViewName, field, ct);
        }

        // Delete views of unused form revision grids.
        foreach (var grid in revisionUpdate.Deleted)
            await _formViewHelperService.DeleteView(grid.ViewName, ct);
    }

    public async Task DeleteViewRelatedData(int definitionId, CancellationToken ct = default)
    {
        // Get view name related to given form definition id.
        var viewName = _dataService.Context.Set<FormDefinition>()
            .Where(definition => definition.Id == definitionId)
            .Select(definition => definition.ViewName).FirstOrDefault();

        // Delete view corresponding to this form definition.
        if (!string.IsNullOrEmpty(viewName)) await _formViewHelperService.DeleteView(viewName, ct);

        // Get revision grids of this form definition.
        var grids = await _dataService.Context.Set<FormRevisionGrid>()
            .Where(revisionGrid => revisionGrid.FormDefinitionId == definitionId)
            .ToListAsync(ct);

        // Delete views related to those form revision grids.
        foreach (var grid in grids) await _formViewHelperService.DeleteView(grid.ViewName, ct);

        // Remove all form revision grids related to this form definition.
        _dataService.Context.Set<FormRevisionGrid>().RemoveRange(grids);
        await _dataService.Context.SaveChangesAsync(ct);
    }
}