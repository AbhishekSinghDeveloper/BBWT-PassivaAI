using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using N = Newtonsoft.Json;
using NL = Newtonsoft.Json.Linq;
using System.Text.Json;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Filters;
using BBWM.Core.Filters.TypedFilters;
using BBWM.Core.Membership.Model;
using BBWM.Core.Services;
using BBWM.Core.Web.Extensions;
using BBWM.FileStorage;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Interfaces;
using BBWM.FormIO.Models;
using BBWM.FormIO.Utils;
using Newtonsoft.Json.Linq;
using BBWM.Messages;
using System.Collections.Specialized;
using BBWM.Messages.Templates;
using BBWM.Core.ModelHashing;
using Microsoft.AspNetCore.Identity;
using BBWM.Core.Membership.Interfaces;
using BBWM.FormIO.Enums;
using BBWM.FormIO.Interfaces.FormViewInterfaces;

namespace BBWM.FormIO.Services
{
    public class FormIODataService : IFormIODataService
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDataService _dataService;
        private readonly UserManager<User> _userManager;
        private readonly ISecurityService _securityService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IFormIOFileService _fileService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IModelHashingService _modelHashingService;

        private string DomainUrl => _httpContextAccessor.HttpContext.GetDomainUrl();

        public FormIODataService(
            IDbContext context,
            IMapper mapper,
            IDataService dataService,
            IHttpContextAccessor httpContextAccessor,
            IFileStorageService fileStorageService,
            IEmailTemplateService emailTemplateService,
            UserManager<User> userManager,
            ISecurityService securityService,
            IModelHashingService modelHashingService,
            IEmailSender emailSender,
            IFormIOFileService fileService)
        {
            _mapper = mapper;
            _context = context;
            _dataService = dataService;
            _fileStorageService = fileStorageService;
            _fileService = fileService;
            _modelHashingService = modelHashingService;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
            _emailTemplateService = emailTemplateService;
            _userManager = userManager;
            _securityService = securityService;
        }

        public Task<bool> FormHasData(int definitionId, CancellationToken ct = default)
        {
            return _context.Set<FormData>().AnyAsync(data => data.FormDefinitionId == definitionId, ct);
        }

        public async Task Delete(int id, CancellationToken ct = default)
        {
            await DeleteFilesWhenFormDataIsDeleted(id, ct);

            await _dataService.Delete<FormData, int>(id, ct);
        }

        public async Task DeleteMultiple(List<int> idsToDelete, CancellationToken ct = default)
        {
            foreach (var id in idsToDelete)
            {
                await Delete(id, ct);
            }
        }

        public async Task<bool> DiscardDraft(int id, bool clearUploadedFiles, CancellationToken ct)
        {
            try
            {
                await _dataService.Delete<FormDataDraft>(id, ct);
                return await _dataService.Context.SaveChangesAsync(ct) > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<FormDataDraftDTO> GetFormDataDraft(int formDefinitionId, string userId, CancellationToken ct)
        {
            return _mapper.Map<FormDataDraftDTO>(await _dataService.Context.Set<FormDataDraft>()
                .FirstOrDefaultAsync(data => data.FormDefinitionId == formDefinitionId && data.UserId == userId, ct));
        }

        public async Task<string> GetFormDataJson(int id, CancellationToken ct)
        {
            try
            {
                var shouldUpdateEntity = false;
                var formDataEntity = await _context.Set<FormData>()
                                         .Where(x => x.Id == id)
                                         .FirstOrDefaultAsync(ct)
                                     ?? throw new BusinessException("Form Data doesn't exist.");

                // to check if the JSON has "file_attachments" or "imageuploader" components
                var jsonObject = FormDataJsonParser.FromJson(formDataEntity.Json);

                var formDataJsonResult = formDataEntity.Json;

                if (string.IsNullOrEmpty(formDataEntity.Json))
                {
                    return "";
                }

                // check if formData has "FileAttachments" component
                if (jsonObject?.Data?.FileAttachments is not null)
                {
                    foreach (var file in jsonObject.Data.FileAttachments)
                    {
                        if (Uri.TryCreate(file.Url, UriKind.Absolute, out var uri))
                        {
                            if (HasThePreSignedUrlExpired(uri, file.UploadTime))
                            {
                                //generate new pre-signed URL
                                var fileWithNewPreSignedUrl = await _fileService.GetFile(file.Key, ct);
                                file.Url = fileWithNewPreSignedUrl.Url;
                                file.UploadTime = DateTime.Now;
                                shouldUpdateEntity = true;
                                // save on DB
                            }
                        }
                    }

                    // save the updated json into formDataEntity.JSON
                    var serializedJson = JsonSerializer.Serialize(jsonObject.Data, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    });
                    formDataJsonResult = UpdateJson(formDataJsonResult, serializedJson, "file_attachments", ct);
                }

                if (jsonObject?.Data?.ImageUploader is not null)
                {
                    foreach (var imageUploader in jsonObject.Data.ImageUploader)
                    {
                        // check if formData has "ImageUploader" component
                        if (imageUploader.Value is not null)
                        {
                            if (Uri.TryCreate(imageUploader.Value.Url, UriKind.Absolute, out var uri))
                            {
                                if (HasThePreSignedUrlExpired(uri, imageUploader.Value.UploadTime))
                                {
                                    try
                                    {
                                        //generate new pre-signed URL
                                        var fileWithNewPreSignedUrl = await _fileService.GetFile(imageUploader.Key, ct);
                                        imageUploader.Value.Url = fileWithNewPreSignedUrl?.Url ?? "";
                                        imageUploader.Value.UploadTime = DateTime.Now;
                                        shouldUpdateEntity = true;
                                        // save on DB
                                    }
                                    catch (Exception)
                                    {
                                        // ignored
                                    }
                                }
                            }

                            var serializedJson = JsonSerializer.Serialize(imageUploader.Value, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            });
                            formDataJsonResult = UpdateJson(formDataJsonResult, serializedJson, imageUploader.Key, ct);
                        }
                    }
                }

                if (shouldUpdateEntity)
                {
                    formDataEntity.Json = formDataJsonResult;
                    await _context.SaveChangesAsync(ct);
                }

                return formDataJsonResult;
            }
            catch (Exception ex)
            {
                throw new BusinessException("Failed attempt to get the Form Data: " + ex.Message, ex);
            }
        }

        public async Task<PageResult<FormDataPageDTO>> GetPage(QueryCommand command, CancellationToken ct = default)
        {
            var isAdmin = false;
            if (command.Filters.Any(x => x.PropertyName == "isAdmin"))
            {
                var adminRole = (command.Filters.FirstOrDefault(x => x.PropertyName == "isAdmin") as BooleanFilter);
                isAdmin = adminRole?.Value ?? false;
                command.Filters.RemoveAll(x => x.PropertyName == "isAdmin");
            }

            var orgIds = new List<int>();
            if (command.Filters.Any(x => x.PropertyName == "organizationId"))
            {
                var ids = (command.Filters.FirstOrDefault(x => x.PropertyName == "organizationId") as NumberArrayFilter)?.Value?.ToList();
                if (ids?.Any() ?? false) orgIds.AddRange(ids);
                command.Filters.RemoveAll(x => x.PropertyName == "organizationId");
            }

            // check if the query has a filter for formRevisionId = -1, if so remove it
            if (command.Filters.Any(x => x.PropertyName == "formDefinitionId" && (x as NumberFilter)?.Value == -1))
            {
                command.Filters.RemoveAll(x => x.PropertyName == "formDefinitionId");
            }

            var surveyPending = command.Filters.Any(x => x.PropertyName == "SurveyId") ? true : false;

            var result = await _dataService.GetPage<FormData, FormDataPageDTO>(
                command,
                query => QueryHandler(query, orgIds, isAdmin, surveyPending),
                sorter: Sorter,
                filter: Filter,
                ct: ct);

            return result;
        }

        private IQueryable<FormData> Sorter(IQueryable<FormData> query, ISorter sorter)
        {
            if (sorter.SortingField == "username")
            {
                query = sorter.SortingDirection == OrderDirection.Asc
                    ? query.OrderBy(fd => fd.CreatedBy.UserName)
                    : query.OrderByDescending(fd => fd.CreatedBy.UserName);
                sorter.SortingField = null;
            }

            if (sorter.SortingField == "formName")
            {
                query = sorter.SortingDirection == OrderDirection.Asc
                    ? query.OrderBy(fd => fd.FormDefinition!.Name)
                    : query.OrderByDescending(fd => fd.FormDefinition!.Name);
                sorter.SortingField = null;
            }

            if (sorter.SortingField == "version")
            {
                query = sorter.SortingDirection == OrderDirection.Asc
                    ? query.OrderBy(formData => formData.FormDefinition!.FormRevisions
                            .Where(revision => revision.Id == formData.FormDefinition.ActiveRevisionId)
                            .Select(revision => revision.MajorVersion)
                            .FirstOrDefault())
                        .ThenBy(formData => formData.FormDefinition!.FormRevisions
                            .Where(revision => revision.Id == formData.FormDefinition.ActiveRevisionId)
                            .Select(revision => revision.MinorVersion)
                            .FirstOrDefault())
                    : query.OrderByDescending(formData => formData.FormDefinition!.FormRevisions
                            .Where(revision => revision.Id == formData.FormDefinition.ActiveRevisionId)
                            .Select(revision => revision.MajorVersion)
                            .FirstOrDefault())
                        .ThenByDescending(formData => formData.FormDefinition!.FormRevisions
                            .Where(revision => revision.Id == formData.FormDefinition.ActiveRevisionId)
                            .Select(revision => revision.MinorVersion)
                            .FirstOrDefault());
                sorter.SortingField = null;
            }

            return query;
        }

        private QueryFilter<FormData> Filter(QueryFilter<FormData> queryFilter)
        {
            return queryFilter.Handle("version", HandleFilterByVersion());
        }

        private static Func<IQueryable<FormData>, StringArrayFilter, IQueryable<FormData>> HandleFilterByVersion()
        {
            return (query, filter) =>
            {
                if (filter.Value is null) return query;

                var firstPass = true;

                var resultQuery = query;

                foreach (var filterValue in filter.Value)
                {
                    var majorAndMinorVersionsArray = filterValue.Split(".");
                    var majorVersion = int.Parse(majorAndMinorVersionsArray[0]);
                    var minorVersion = int.Parse(majorAndMinorVersionsArray[1]);

                    var filteredQuery = query.Where(formData =>
                        formData.FormDefinition!.FormRevisions
                            .Where(revision => revision.Id == formData.FormDefinition.ActiveRevisionId)
                            .Select(revision => revision.MajorVersion)
                            .FirstOrDefault() == majorVersion &&
                        formData.FormDefinition!.FormRevisions
                            .Where(revision => revision.Id == formData.FormDefinition.ActiveRevisionId)
                            .Select(revision => revision.MinorVersion)
                            .FirstOrDefault() == minorVersion);

                    if (firstPass)
                    {
                        resultQuery = filteredQuery;
                        firstPass = false;
                    }
                    else
                    {
                        resultQuery = resultQuery.Union(filteredQuery);
                    }
                }

                return resultQuery;
            };
        }

        public async Task<FormDataDraftDTO> SaveFormDataDraft(FormDataDraftDTO formDataDraft, CancellationToken ct)
        {
            var newDraft = _mapper.Map<FormDataDraft>(formDataDraft);
            var draft = await _context.Set<FormDataDraft>()
                .FirstOrDefaultAsync(dataDraft => dataDraft.FormDefinitionId == newDraft.FormDefinitionId && dataDraft.UserId == newDraft.UserId);

            try
            {
                if (draft != null)
                {
                    draft.Json = newDraft.Json;
                    draft.CreatedOn = newDraft.CreatedOn;
                    _dataService.Context.Set<FormDataDraft>().Update(draft);
                    await _dataService.Context.SaveChangesAsync();
                    return _mapper.Map<FormDataDraftDTO>(draft);
                }

                _dataService.Context.Set<FormDataDraft>().Add(newDraft);
                await _dataService.Context.SaveChangesAsync();
                return _mapper.Map<FormDataDraftDTO>(newDraft);
            }
            catch (Exception)
            {
                return formDataDraft;
            }
        }

        public async Task<FormDataDTO> Update(FormDataDTO dto, CancellationToken ct = default)
        {
            var entity = await _dataService.Context.Set<FormData>().FirstOrDefaultAsync(x => x.Id.Equals(dto.Id), ct);
            if (entity != null)
            {
                entity.CreatedOn = DateTime.UtcNow;
            }

            return await _dataService.Update<FormData, FormDataDTO>(dto, ct);
        }

        private async Task DeleteFilesWhenFormDataIsDeleted(int formDataId, CancellationToken ct)
        {
            if (formDataId == default)
            {
                return;
            }

            var formDataEntity = await _context.Set<FormData>().Where(x => x.Id == formDataId).AsNoTracking().FirstOrDefaultAsync(ct);
            if (formDataEntity == null)
            {
                return;
            }

            try
            {
                var jsonData = FormDataJsonParser.FromJson(formDataEntity.Json);

                var imageUploaderKeysToDelete = jsonData?.Data?.ImageUploader;
                var fileAttachmentsKeysToDelete = jsonData?.Data?.FileAttachments?.Select(x => x.Key).ToArray();

                List<Task> arrayOfTasks = new List<Task>();

                if (fileAttachmentsKeysToDelete is not null)
                {
                    Task deleteFileAttachmentsFiles = _fileService.DeleteMultipleFilesAsync(fileAttachmentsKeysToDelete ?? Array.Empty<string>(), ct);
                    arrayOfTasks.Add(deleteFileAttachmentsFiles);
                }

                if (imageUploaderKeysToDelete is not null)
                {
                    imageUploaderKeysToDelete.ToList().ForEach(x =>
                    {
                        Task deleteImageUploaderFiles = _fileStorageService.DeleteFile(x.Key ?? "", ct);
                        arrayOfTasks.Add(deleteImageUploaderFiles);
                    });
                }

                await Task.WhenAll(arrayOfTasks);
            }
            catch (Exception)
            {
                // failed to parse or process shouldn't prevent from deletion
            }
        }

        public async Task<bool> SaveFormData(FormDataDTO dto, CancellationToken ct)
        {
            var changesApplied = false;
            if (dto.RequestId != null) // Saving a Requested FormData
            {
                // current form data id must be associated to its related form request
                var ok = await _dataService.Context.Set<FormRequest>().AnyAsync(x => x.Id == dto.RequestId.Value && x.FormDataId == dto.Id);
                if (!ok)
                {
                    throw new BusinessException("Invalid Form request identifiers");
                }

                await _dataService.Context.Set<FormRequest>().Where(x => x.Id == dto.RequestId.Value).UpdateFromQueryAsync(req => new()
                {
                    CompletionDate = dto.CreatedOn
                });
                changesApplied = await _dataService.Context.Set<FormData>().Where(x => x.Id == dto.Id).UpdateFromQueryAsync(def => new()
                {
                    Json = dto.Json
                }) > 0;
            }
            else if (dto.IsMUF && dto.Id > 0) // Saving a MultiUser FormData
            {
                var mergedJson = dto.Json;
                var links = await _dataService.Context.Set<MultiUserFormAssociationLinks>()
                    .Where(x => x.Id == dto.MultiUserFormAssocLinkId)
                    .Join(_dataService.Context.Set<MultiUserFormStage>().Include(x => x.MultiUserFormDefinition),
                        link => link.MultiUserFormStageId,
                        stage => stage.Id,
                        (link, stage) => stage)
                    .ToListAsync();
                var link = links.FirstOrDefault(x => x.ReviewerStage);
                var currentStage = links.FirstOrDefault();
                var data = (await _dataService.Context.Set<FormData>().FirstOrDefaultAsync(x => x.Id == dto.Id));
                // this means that the current stage is a reviewer stage
                var tabsToBeResetted = new List<KeyValuePair<string, string>>();
                // Sanitize the JSON removing the linkedTab object and replacing with the real value, also checking if the current state is reviewer and getting which tabs needs review.
                if (dto.Json.Contains("\"linkedTab\":"))
                {
                    var values = N.JsonConvert.DeserializeObject(dto.Json) as NL.JObject;
                    foreach (var item in values.First.Children())
                    {
                        foreach (var review in item.Children())
                        {
                            // Only process objects not properties, as Reviewer Input sends an object
                            if (review.Values().Count() > 1 && !string.IsNullOrEmpty(review.First["linkedTab"].ToString()))
                            {
                                // reset the tab only if the current stage is a reviewer stage
                                if (link != null && !string.IsNullOrEmpty(review.First["value"].ToString()))
                                    tabsToBeResetted.Add(new KeyValuePair<string, string>(review.First["linkedTab"].ToString(),
                                        review.First["value"].ToString()));
                                // Sanitize the reviewer input or related components value from object to primitive value
                                item[((JProperty)review).Name] = review.First["value"];
                            }
                        }
                    }

                    // update the Json string back after modifying the reviewers notes to only have the value.
                    dto.Json = N.JsonConvert.SerializeObject(values);
                }

                // Merge data to the current Form Data Json value
                if (data != null && dto.SurveyId is null)
                {
                    mergedJson = JsonMerger.Merge(data.Json, dto.Json);
                }

                changesApplied = await _dataService.Context.Set<FormData>().Where(x => x.Id == dto.Id).UpdateFromQueryAsync(def => new()
                {
                    Json = mergedJson
                }) > 0;
                // This is to know if a reviewer take a note to not mark the reviewer state as done
                var allOk = true;
                if (tabsToBeResetted.Any())
                {
                    changesApplied = true;
                    var assocLink = await _dataService.Context.Set<MultiUserFormAssociationLinks>().Include(x => x.MultiUserFormAssociations)
                        .FirstOrDefaultAsync(x => x.Id == dto.MultiUserFormAssocLinkId);
                    var dataId = _modelHashingService.HashProperty(typeof(FormDataDTO), nameof(FormDataDTO.Id), dto.Id);
                    var mufAssocId = _modelHashingService.HashProperty(typeof(MultiUserFormAssociationsDTO), nameof(MultiUserFormAssociationsDTO.Id),
                        assocLink.MultiUserFormAssociationsId);
                    foreach (var tab in tabsToBeResetted)
                    {
                        // Get the reviewer MUFAssocLink
                        if (assocLink != null)
                        {
                            var relatedLinks = await _dataService.Context.Set<MultiUserFormAssociationLinks>()
                                .Include(x => x.MultiUserFormStage)
                                .Where(x => x.MultiUserFormAssociationsId == assocLink.MultiUserFormAssociationsId && x.Id != dto.MultiUserFormAssocLinkId &&
                                            tab.Key == x.MultiUserFormStage.InnerTabKey)
                                .ToListAsync();
                            if (relatedLinks != null && relatedLinks.Any())
                            {
                                foreach (var relatedLink in relatedLinks)
                                {
                                    if (relatedLink != null)
                                    {
                                        var email = (await _context.Set<User>().FindAsync(relatedLink.UserId))?.Email ?? "";
                                        var url = $"app/formio/multiuser/display?mufId={mufAssocId}&target={relatedLink.UserId}&formDataId={dataId}";
                                        relatedLink.IsFilled = false;
                                        _dataService.Context.Set<MultiUserFormAssociationLinks>().Update(relatedLink);
                                        if (!string.IsNullOrEmpty(email))
                                        {
                                            try
                                            {
                                                await SendEmail(email, url,
                                                    $"{link?.MultiUserFormDefinition.Name ?? ""}({assocLink.MultiUserFormAssociations.Description})", tab.Value,
                                                    ct);
                                            }
                                            catch (Exception)
                                            {
                                                // ignored
                                            }
                                        }
                                    }
                                }
                            }

                            allOk = false;
                        }
                    }

                    await _dataService.Context.SaveChangesAsync();
                }

                if (changesApplied)
                {
                    await _dataService.Context.Set<MultiUserFormAssociationLinks>().Where(x => x.Id == dto.MultiUserFormAssocLinkId).UpdateFromQueryAsync(def =>
                        new MultiUserFormAssociationLinks()
                        {
                            IsFilled = true & allOk,
                            Completed = DateTime.UtcNow,
                        });
                }

                // setting the active sequente to the earlier stage step index
                var activeIndexes =
                    (await _dataService.Context.Set<MultiUserFormAssociationLinks>().Include(x => x.MultiUserFormStage)
                        .Where(x => x.MultiUserFormAssociationsId == dto.MufAssocId && !x.IsFilled).Select(x => x.MultiUserFormStage)
                        .GroupBy(x => x.SequenceStepIndex).ToListAsync()).OrderBy(x => x.Key).FirstOrDefault();
                if (activeIndexes != null)
                {
                    foreach (var item in activeIndexes)
                    {
                        // Update the current Active sequense only if needed
                        if ((currentStage?.SequenceStepIndex ?? -1) != item.SequenceStepIndex)
                        {
                            await _dataService.Context.Set<MultiUserFormAssociations>().Where(x => x.Id == dto.MufAssocId).UpdateFromQueryAsync(x =>
                                new MultiUserFormAssociations
                                    { ActiveStepSequenceIndex = item.SequenceStepIndex <= 0 ? x.TotalSequenceSteps : item.SequenceStepIndex });
                            // if the new actived stage is for an external user, then send an email notification
                            if (item?.StageTargetType == StageTargetType.ExternalUsers)
                            {
                                var assocLink = await _dataService.Context.Set<MultiUserFormAssociationLinks>().Include(x => x.MultiUserFormAssociations)
                                    .ThenInclude(x => x.MultiUserFormDefinition).FirstOrDefaultAsync(x =>
                                        x.MultiUserFormAssociations.FormDataId == dto.Id && x.MultiUserFormStageId == item.Id);
                                var otp = SecurityCodeGenerator();
                                var newPassword = _securityService.GetHashedPassword(otp);
                                var dataId = _modelHashingService.HashProperty(typeof(FormDataDTO), nameof(FormDataDTO.Id), dto.Id);
                                var mufId = _modelHashingService.HashProperty(typeof(MultiUserFormAssociationsDTO), nameof(MultiUserFormAssociationsDTO.Id),
                                    assocLink.MultiUserFormAssociationsId);
                                var url = $"app/formio/multiuser/external?mufId={mufId}&formDataId={dataId}&target={assocLink.ExternalUserEmail}";
                                var user = await _userManager.FindByEmailAsync(assocLink.ExternalUserEmail);
                                if (user != null)
                                {
                                    try
                                    {
                                        await _userManager.RemovePasswordAsync(user).ConfigureAwait(false);
                                        var result = await _userManager.AddPasswordAsync(user, newPassword).ConfigureAwait(false);
                                        if (result.Succeeded)
                                        {
                                            await SendExternalNotificationEmail(assocLink.ExternalUserEmail, otp, url,
                                                $"{assocLink.MultiUserFormAssociations.MultiUserFormDefinition.Name}({assocLink.MultiUserFormAssociations.Description})",
                                                ct);
                                        }
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (dto.Id > 0 && dto.SurveyId != null) // Surveys
            {
                changesApplied = await _dataService.Context.Set<FormData>().Where(x => x.Id == dto.Id).UpdateFromQueryAsync(def => new()
                {
                    Json = dto.Json
                }) > 0;
                return changesApplied;
            }
            else
            {
                // add the FormData entity to the FormRevision.FormDatas[]
                var formDefinition = await _dataService.Context.Set<FormDefinition>().FirstAsync(x => x.Id == dto.FormDefinitionId);

                if (formDefinition is null)
                {
                    throw new BusinessException("FormDefinition doesn't exist. Unable to save data.");
                }

                var form = _mapper.Map<FormData>(dto);
                //form.UserID = _currentUserId;

                formDefinition.FormData.Add(form);

                changesApplied = await _dataService.Context.SaveChangesAsync() > 0;
            }

            try
            {
                if (await _dataService.Context.SaveChangesAsync() > 0 || changesApplied)
                {
                    // TODO: test
                    if (dto.DraftId.HasValue) await DiscardDraft(dto.DraftId.Value, false, ct);
                    // Ensure deletion of all Draft (in case more than one were saved) related to the current user and formDef
                    _dataService.Context.Set<FormDataDraft>()
                        .Where(dataDraft => dataDraft.UserId == dto.UserId && dataDraft.FormDefinitionId == dto.FormDefinitionId)
                        .DeleteFromQuery();
                    await _dataService.Context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public List<string> GetVersionsForFiltering(List<int> orgIds, bool isAdmin, CancellationToken ct)
        {
            var formDatas = _dataService.Context.Set<FormData>()
                .Include(formData => formData.FormDefinition)
                .Where(formData => isAdmin || orgIds.Any(org => org == formData.OrganizationId.GetValueOrDefault()))
                .ToList();

            var versions = formDatas
                .Select(formData => formData.FormDefinition!.FormRevisions
                    .FirstOrDefault(revision => revision.Id == formData.FormDefinition.ActiveRevisionId))
                .Select(formRevision => new { MajorVersion = formRevision?.MajorVersion ?? 0, MinorVersion = formRevision?.MinorVersion ?? 0 })
                .ToHashSet();

            var availableVersions = versions
                .OrderBy(version => version.MajorVersion)
                .ThenBy(version => version.MinorVersion)
                .Select(valueType => $"{valueType.MajorVersion}.{valueType.MinorVersion}")
                .ToList();

            return availableVersions;
        }

        private static bool HasThePreSignedUrlExpired(Uri uri, DateTimeOffset uploadedAt)
        {
            try
            {
                var expireIn = uri.Query
                    .Split("&")
                    .First(x => x.Contains("X-Amz-Expires"))
                    .Split("=")
                    .Skip(1)
                    .First();

                var passedTime = DateTime.Now - uploadedAt;

                return passedTime.TotalSeconds > int.Parse(expireIn);
            }
            catch
            {
                return false;
            }
        }

        private string UpdateJson(string jsonOriginal, string newJsonValue, string componentToSearch, CancellationToken ct)
        {
            // we will update this JSON if properties "file_attachments" or "imageuploaders" are present
            var jsonOriginalAsjObject = N.JsonConvert.DeserializeObject(jsonOriginal!) as JObject;
            var jsonOriginalAsjObjectData = jsonOriginalAsjObject!.SelectToken($"data.{componentToSearch}");

            var newJsonValueAsJObject = N.JsonConvert.DeserializeObject(newJsonValue) as JObject;

            var componentToSearchCamelCase = componentToSearch switch
            {
                "file_attachments" => "fileAttachments",
                "imageuploader" => "imageUploader",
                _ => ""
            };

            var newJsonValueAsJObjectData = newJsonValueAsJObject!.SelectToken($"{componentToSearchCamelCase}");

            if (jsonOriginalAsjObjectData is null || newJsonValueAsJObjectData is null)
            {
                return jsonOriginal;
            }

            jsonOriginalAsjObjectData!.Replace(newJsonValueAsJObjectData);

            return jsonOriginalAsjObject.ToString(N.Formatting.None);
        }

        private static IQueryable<FormData> QueryHandler(IQueryable<FormData> query, List<int> orgIds, bool isAdmin, bool surveyPending)
        {
            query = query
                .Include(formData => formData.CreatedBy)
                .Include(formData => formData.FormDefinition);

            query = surveyPending
                ? query.Include(f => f.Survey)
                : query;

            query = isAdmin || surveyPending
                ? query
                : query.Where(formData => formData.OrganizationId.HasValue && orgIds.Any(org => org == formData.OrganizationId.Value));
            return query;
        }

        private async Task SendExternalNotificationEmail(string email, string otp, string url, string name, CancellationToken ct)
        {
            // Send email
            var emailTemplate = await _emailTemplateService.GetByCode("FormIOExternalUserNotification", ct);
            if (emailTemplate is null)
                throw new ConflictException("Email template for the user invitation not found.");

            // Generate callbackUrl
            var callbackUrl = $"{DomainUrl}/{url}";

            var tagValues = new NameValueCollection
            {
                { "$UserName", "dear friend" },
                { "$CallbackUrl", callbackUrl },
                { "$OTP", otp },
                { "$FormIOName", name }
            };
            _emailTemplateService.BuildEmail(emailTemplate, tagValues);

            await _emailSender.SendEmail(emailTemplate.Subject, emailTemplate.Body, from: emailTemplate.From, to: email);
        }

        private async Task SendEmail(string email, string url, string name, string note, CancellationToken ct)
        {
            // Send email
            var emailTemplate = await _emailTemplateService.GetByCode("FormioReviewNotification", ct);
            if (emailTemplate is null)
                throw new ConflictException("Email template for the reviewer notification not found.");

            // Generate callbackUrl
            var callbackUrl = $"{DomainUrl}/{url}";

            var tagValues = new NameValueCollection
            {
                { "$UserName", "dear friend" },
                { "$CallbackUrl", callbackUrl },
                { "$FormIOName", name },
                { "$FormIONote", note }
            };
            _emailTemplateService.BuildEmail(emailTemplate, tagValues);

            await _emailSender.SendEmail(emailTemplate.Subject, emailTemplate.Body, from: emailTemplate.From, to: email);
        }

        private static string SecurityCodeGenerator()
        {
            var rnd = new Random(Environment.TickCount);
            var code = "";
            for (var i = 0; i < 6; i++)
            {
                code += (rnd.Next(0, 9)).ToString();
            }

            return code;
        }
    }
}