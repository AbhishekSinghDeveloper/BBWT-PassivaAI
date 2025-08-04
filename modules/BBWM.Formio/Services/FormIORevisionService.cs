using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Filters;
using BBWM.Core.Services;
using BBWM.Core.Web.Extensions;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Extensions;
using BBWM.FormIO.Interfaces;
using BBWM.FormIO.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using BBWM.FormIO.Interfaces.FormViewInterfaces;

namespace BBWM.FormIO.Services;

public class FormIORevisionService : IFormioIORevisionService
{
    private readonly IDbContext _dbContext;
    private readonly IDataService _dataService;
    private readonly string _currentUserId;
    private readonly IFormViewService _viewService;
    private readonly IFormIODataService _formIODataService;

    public FormIORevisionService(IDbContext context,
        IDataService dataService,
        IHttpContextAccessor httpContextAccessor,
        IFormViewService viewService,
        IFormIODataService formIoDataService)
    {
        _viewService = viewService;
        _dbContext = context;
        _dataService = dataService;
        _formIODataService = formIoDataService;
        _currentUserId = httpContextAccessor.HttpContext.GetUserId();
    }

    public async Task<PageResult<FormRevisionDTO>> GetPage(QueryCommand command, CancellationToken ct = default)
    {
        return await _dataService.GetPage<FormRevision, FormRevisionDTO>(command, query => query
            .Include(x => x.Creator), ct);
    }

    public async Task<FormRevisionDTO> Create(NewFormRevisionRequestDTO dto, CancellationToken ct)
    {
        var createdRevision = new FormRevisionDTO();

        try
        {
            JObject json = JObject.Parse(dto.Json);
            var list = json?.First?.First != null
                ? json?.First?.First?.GetInnerFormDefinitionComponents(x => x.Value<string>("type") == "state-tabs")
                : new List<JToken>();
            if (list?.Count > 1)
            {
                throw new BusinessException("There can be only one 'state-tabs' component");
            }

            // If the form points to an existing form, create a new Version
            // FormDefinition to update
            var formDefinitionEntity = await _dataService.Context.Set<FormDefinition>()
                .Where(x => x.Id == dto.FormDefinitionId).FirstOrDefaultAsync(ct);

            if (formDefinitionEntity is null)
            {
                throw new BusinessException("FormDefinition not found. Unable to create a new Version.");
            }

            // create a new FormRevision always
            var newFormRevision = new FormRevisionDTO
            {
                Id = 0,
                DateCreated = DateTime.Now,
                FormData = new List<FormData>(),
                FormDefinitionId = dto.FormDefinitionId,
                MobileFriendly = dto.MobileFriendly,
                Json = dto.Json,
                MUFCapable = list?.Any() ?? false,
                Note = dto.Note,
                CreatorId = _currentUserId,
            };

            // CHECK USERID is changing for some reason
            createdRevision = await _dataService.Create<FormRevision, FormRevisionDTO>(newFormRevision, ct);

            formDefinitionEntity.ActiveRevisionId = createdRevision.Id;
            var success = await _dataService.Context.SaveChangesAsync(ct) > 0;

            // Update form views related data (form revision view name if it's necessary,
            // form definition view, form revision grids, and form revision grid views).
            if (success) await _viewService.UpdateViewRelatedData(formDefinitionEntity.Id, ct);
        }
        catch (Exception ex)
        {
            throw new BusinessException(ex.Message);
        }

        return createdRevision;
    }

    public async Task<FormRevisionDTO> CreateInitialFormRevision(int formDefinitionId, InitialFormRevisionRequestDTO dto, CancellationToken ct)
    {
        var newEntity = new FormRevisionDTO
        {
            Id = 0,
            Note = null,
            DateCreated = DateTime.Now,
            FormData = new List<FormData>(),
            FormDefinitionId = formDefinitionId,
            Json = dto.Json,
            MUFCapable = dto.MUFCapable,
            MobileFriendly = dto.MobileFriendly,
            CreatorId = _currentUserId,
            MajorVersion = 1,
            MinorVersion = 0
        };

        return await _dataService.Create<FormRevision, FormRevisionDTO>(newEntity, ct);
    }

    public async Task Delete(int id, CancellationToken ct = default)
    {
        // delete all revisions
        var formId = (await _dataService.Context.Set<FormRevision>().FindAsync(id))?.FormDefinitionId;
        // Delete the form revision
        await _dataService.DeleteAll<FormRevision>(query => query.Where(x => x.Id == id), ct);

        // Assign the latest revision as active
        await _dataService.Context.SaveChangesAsync();
        var lastRevision = (await _dataService.Context.Set<FormRevision>().Where(x => x.FormDefinitionId == formId)
            .OrderBy(x => x.Id).LastAsync())?.Id;

        if (lastRevision.HasValue)
        {
            await _dataService.Context.Set<FormDefinition>().Where(x => x.Id == formId)
                .UpdateFromQueryAsync(def => new()
            {
                ActiveRevisionId = lastRevision.Value
            });
        }
    }

    public async Task<bool> SetActive(int formId, int revisionId, CancellationToken ct)
    {
        //Get corresponding form definition.
        var formDefinitionEntity = await _dataService.Context.Set<FormDefinition>()
            .FirstOrDefaultAsync(definition => definition.Id == formId, ct);
        if (formDefinitionEntity == null) return false;

        // Set this revision as active for the form definition.
        formDefinitionEntity.ActiveRevisionId = revisionId;
        var success = await _dataService.Context.SaveChangesAsync(ct) > 0;

        // Update form views related data (form revision view name if it's necessary,
        // form definition view, form revision grids, and form revision grid views).
        if (success) await _viewService.UpdateViewRelatedData(formDefinitionEntity.Id, ct);
        return success;
    }

    public async Task<bool> Update(int formRevisionId, UpdateFormRevisionRequestDTO dto, CancellationToken ct = default)
    {
        var entity = await _dbContext.Set<FormRevision>().Where(x => x.Id == formRevisionId).FirstOrDefaultAsync(ct);

        if (entity == null)
        {
            throw new BusinessException("Unable to update FormRevision, entity doesn't exist.");
        }

        JObject json = JObject.Parse(dto.Json);
        var list = json?.First?.First != null
            ? json?.First?.First?.GetInnerFormDefinitionComponents(x => x.Value<string>("type") == "state-tabs")
            : new List<JToken>();
        if (list?.Count > 1)
        {
            throw new BusinessException("There can be only one 'state-tabs' component");
        }

        // check if the form revision has any form data
        var hasFormData = await _formIODataService.FormHasData(entity.FormDefinitionId!.Value, ct);

        // if version has form data and the user says that the new version has no breaking changes
        // create another version
        if (hasFormData)
        {
            return await UpdateMinorOrMayorVersions(entity, dto, ct);
        }

        entity.Note = dto.Note;
        // TODO: should we update the creator of the revision?
        entity.CreatorId = dto.CreatorId;
        entity.Json = dto.Json;
        entity.MUFCapable = list?.Any() ?? false;
        entity.MobileFriendly = dto.MobileFriendly;

        //Get corresponding form definition.
        var formDefinitionEntity = await _dataService.Context.Set<FormDefinition>()
            .FirstAsync(definition => definition.Id == entity.FormDefinitionId, ct);

        // If this one is not the active form revision, just save the changes and return.
        if (formDefinitionEntity.ActiveRevisionId != formRevisionId)
            return await _dataService.Context.SaveChangesAsync(ct) > 0;

        // Update the form definition name.
        formDefinitionEntity.Name = dto.FormDefinitionName;

        // Save the changes and return false if the saving fails.
        if (await _dataService.Context.SaveChangesAsync(ct) <= 0) return false;

        // Update form views related data (form revision view name if it's necessary,
        // form definition view, form revision grids, and form revision grid views).
        await _viewService.UpdateViewRelatedData(formDefinitionEntity.Id, ct);

        return true;
    }

    private async Task<bool> UpdateMinorOrMayorVersions(FormRevision previousVersion, UpdateFormRevisionRequestDTO dto, CancellationToken ct = default)
    {
        // we need to set the Active Revision of the FormDefinition to the new Revision that will be created
        var formDefinition = await _dbContext.Set<FormDefinition>()
                                 .Where(x => x.Id == previousVersion.FormDefinitionId)
                                 .FirstOrDefaultAsync(ct)
                             ?? throw new BusinessException("Unable to save the new Version, FormDefinition doesn't exist.");
        JObject json = JObject.Parse(dto.Json);
        var list = json?.First?.First != null
            ? json?.First?.First?.GetInnerFormDefinitionComponents(x => x.Value<string>("type") == "state-tabs")
            : new List<JToken>();
        if (list?.Count > 1)
        {
            throw new BusinessException("There can be only one 'state-tabs' component");
        }

        var newVersion = new FormRevisionDTO()
        {
            Id = 0,
            CreatorId = dto.CreatorId,
            DateCreated = DateTime.Now,
            FormData = new List<FormData>(),
            FormDefinitionId = previousVersion.FormDefinitionId!.Value,
            Json = dto.Json,
            MajorVersion = dto.SaveAsMajorVersion ? previousVersion.MajorVersion + 1 : previousVersion.MajorVersion,
            MinorVersion = dto.IncreaseMinorVersion
                ? previousVersion.MinorVersion + 1
                : dto.SaveAsMajorVersion
                    ? 0
                    : previousVersion.MinorVersion,
            MobileFriendly = dto.MobileFriendly,
            Note = dto.Note,
            MUFCapable = list?.Any() ?? false,
        };

        try
        {
            var createdVersion = await _dataService.Create<FormRevision, FormRevisionDTO>(newVersion, ct);

            formDefinition.ActiveRevisionId = createdVersion.Id;
            // update form name
            formDefinition.Name = dto.FormDefinitionName;
            var success = await _dataService.Context.SaveChangesAsync(ct) > 0;

            // Update form views related data (form revision view name if it's necessary,
            // form definition view, form revision grids, and form revision grid views).
            if (success) await _viewService.UpdateViewRelatedData(formDefinition.Id, ct);
            return success;
        }
        catch (Exception ex)
        {
            throw new BusinessException(ex.Message);
        }
    }
}