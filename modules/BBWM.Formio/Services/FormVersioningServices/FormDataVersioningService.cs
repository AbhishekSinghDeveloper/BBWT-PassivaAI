using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Services;
using BBWM.Core.Tasks;
using BBWM.FormIO.Classes;
using BBWM.FormIO.DTO.FormVersioningDTOs;
using BBWM.FormIO.Enums;
using BBWM.FormIO.Interfaces.FormVersioningInterfaces;
using BBWM.FormIO.Interfaces.FormViewInterfaces;
using BBWM.FormIO.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace BBWM.FormIO.Services.FormVersioningServices;

public class FormDataVersioningService : IFormDataVersioningService
{
    private readonly IDataService _dataService;
    private readonly IFormFieldService _formFieldService;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<FormDataVersioningService> _logger;


    public FormDataVersioningService(
        IDataService dataService,
        IFormFieldService formFieldService,
        IBackgroundTaskQueue backgroundTaskQueue,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<FormDataVersioningService> logger)
    {
        _logger = logger;
        _dataService = dataService;
        _formFieldService = formFieldService;
        _backgroundTaskQueue = backgroundTaskQueue;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public void UpdateFormDataInBackground(int definitionId, IEnumerable<FormFieldDataUpdate> updates)
    {
        _backgroundTaskQueue.QueueBackgroundWorkItem(async ct =>
        {
            _logger.LogInformation("Updating data for new form version.");

            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<IDbContext>();
                await UpdateFormData(definitionId, updates, context, ct);
            }
            catch (Exception exception)
            {
                _logger.LogInformation($"Data update failed due to error: {exception.Message}");
            }

            _logger.LogInformation("Data update finished");
            return null;
        });
    }

    public Task UpdateFormData(int definitionId, IEnumerable<FormFieldDataUpdate> updates, CancellationToken ct = default)
        => UpdateFormData(definitionId, updates, _dataService.Context, ct);

    public async Task UpdateFormData(int definitionId, IEnumerable<FormFieldDataUpdate> updates,
        IDbContext context, CancellationToken ct = default)
    {
        // Get form revision info related to given form definition id.
        var revision = context.Set<FormDefinition>()
            .Where(definition => definition.Id == definitionId)
            .Select(definition => definition.FormRevisions
                .Where(revision => revision.Id == definition.ActiveRevisionId)
                .Select(revision => new { revision.Id, revision.Json })
                .FirstOrDefault()).FirstOrDefault();

        if (revision == null) throw new BusinessException("Definition with given id doesn't have active revision.");

        // Get the whole form field tree corresponding to this active form revision.
        var root = _formFieldService.GetFormField(revision.Json, ".data");
        if (root == null) throw new BusinessException("Invalid form revision json.");

        // Update form data related to this form definition.
        var formFieldDataUpdates = updates.ToList();
        await UpdateFormData(definitionId, formFieldDataUpdates, root, context, ct);
    }

    private static async Task UpdateFormData(int definitionId, ICollection<FormFieldDataUpdate> updates,
        FormField root, IDbContext context, CancellationToken ct = default)
    {
        // Get all form data related to this form definition.
        var definitionData = await context.Set<FormData>()
            .AsNoTracking()
            .Where(data => data.FormDefinitionId == definitionId)
            .Select(data => new { data.Id, data.Json })
            .ToListAsync(ct);

        // Map the objects manually.
        var versioningDataCollection = definitionData
            .Select(data => new FormDataVersioningDTO
            {
                Id = data.Id,
                JsonObject = JObject.Parse(data.Json)
            }).Where(data => data.JsonObject != null).ToList();

        UpdateFormData(versioningDataCollection, updates, root);

        // Update all form data with the new json definitions.
        foreach (var versioningData in versioningDataCollection)
            await context.Set<FormData>().Where(data => data.Id == versioningData.Id)
                .UpdateFromQueryAsync(_ => new { Json = versioningData.JsonObject!.ToString() }, ct);
    }

    private static void UpdateFormData(ICollection<FormDataVersioningDTO> dataCollection,
        ICollection<FormFieldDataUpdate> updates, FormField root)
    {
        foreach (var update in updates.Where(update => update.Action != FormFieldChangeAction.Remove))
        {
            foreach (var data in dataCollection)
            {
                // Search for the data json objects at this field children direction.
                var path = root.Path + root.ChildrenPath;
                var tokens = data.JsonObject!.SelectTokens(path).ToList();

                // Update the value in the corresponding json token.
                foreach (var token in tokens)
                {
                    switch (update)
                    {
                        // If default value for addition action is null, and this update has no nested updates,
                        // just remove the field from the json data (absence of data is treated as null).
                        case { Action: FormFieldChangeAction.Add, Value: null, Updates.Count: 0 }:
                            token[update.Key]?.Remove(); break;

                        // If the same occur, but there are nested updates, then create this component field
                        // in the json data to be able to place its nested update values (within an addition update
                        // there can only be other addition updates, so this component field creation is needed).
                        // Do the same if action is edit and there are nested updates.
                        case { Action: FormFieldChangeAction.Add, Value: null, Updates.Count: > 0 } or
                            { Action: FormFieldChangeAction.Edit, Updates.Count: > 0 }:
                            token[update.Key] ??= GetFieldDefaultValue(update.Type); break;

                        // Otherwise, id there is value for this addition action,
                        // update this field in the form json data with the given value.
                        case { Action: FormFieldChangeAction.Add, Value: not null }:
                            token[update.Key] = update.Value; break;
                    }
                }
            }

            // If there are no nested updates, continue.
            if (update.Updates.Count == 0) continue;

            // Find the form field corresponding to this form field update,
            // and call recursively with these nested updates and the found form field.
            var field = root.Children.FirstOrDefault(child => string.Equals(child.Key, update.Key));
            if (field != null) UpdateFormData(dataCollection, update.Updates, field);
        }
    }

    private static JToken GetFieldDefaultValue(string fieldType)
        => JToken.Parse(string.Equals(fieldType, "datagrid") ? "[{}]" : "{}");
}