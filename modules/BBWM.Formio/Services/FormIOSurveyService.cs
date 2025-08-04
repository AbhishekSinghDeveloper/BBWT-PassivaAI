using AutoMapper;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Filters;
using BBWM.Core.Filters.TypedFilters;
using BBWM.Core.Membership.Model;
using BBWM.Core.Services;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Interfaces;
using BBWM.FormIO.Models;
using Microsoft.EntityFrameworkCore;

namespace BBWM.FormIO.Services
{
    public class FormIOSurveyService : IFormIOSurveyService
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDataService _dataService;

        public FormIOSurveyService(
            IDbContext context,
            IMapper mapper,
            IDataService dataService)
        {
            _mapper = mapper;
            _context = context;
            _dataService = dataService;
        }

        public async Task<FormSurveyDTO> Create(FormSurveyDTO surveyDTO, CancellationToken ct)
        {
            var surveyCreatedDTO = await _dataService.Create<FormSurvey, FormSurveyDTO>(surveyDTO,
                (entity, _) => { entity.Created = new DateTimeOffset(DateTime.UtcNow); }, ct);

            var definitionId = await _dataService.Context.Set<FormRevision>()
                .Where(revision => revision.Id == surveyCreatedDTO.FormRevisionId)
                .Select(revision => revision.FormDefinitionId)
                .FirstOrDefaultAsync(ct);

            foreach (var userId in surveyDTO.SurveyedUsers)
            {
                await _context.Set<FormData>().AddAsync(new FormData
                {
                    CreatedOn = DateTime.UtcNow,
                    UserId = userId,
                    Json = "{}",
                    FormDefinitionId = definitionId,
                    SurveyId = surveyCreatedDTO.Id
                });
            }

            await _context.SaveChangesAsync();

            var survey = await _context.Set<FormSurvey>().Where(x => x.Id == surveyCreatedDTO.Id)
                .Include(x => x.SurveyFormDataInstances)
                .ThenInclude(y => y.CreatedBy)
                .Include(x => x.SurveyFormDataInstances)
                .Include(y => y.FormRevision)
                .FirstOrDefaultAsync(ct);

            surveyDTO = _mapper.Map(survey, surveyCreatedDTO);

            return surveyDTO;
        }

        public async Task<FormSurveyDTO> Update(FormSurveyDTO surveyDTO, CancellationToken ct = default)
        {
            var survey = await _context.Set<FormSurvey>().Where(s => s.Id == surveyDTO.Id)
                .Include(x => x.SurveyFormDataInstances)
                .ThenInclude(y => y.CreatedBy)
                .Include(x => x.SurveyFormDataInstances)
                .Include(y => y.FormRevision)
                .FirstOrDefaultAsync(ct);

            var definitionId = await _dataService.Context.Set<FormRevision>()
                .Where(revision => revision.Id == survey!.FormRevisionId)
                .Select(revision => revision.FormDefinitionId)
                .FirstOrDefaultAsync(ct);

            var surveyedUsers = surveyDTO.SurveyedUsers
                .Where(su => !survey.SurveyFormDataInstances.Select(x => x.UserId).Contains(su)).ToList();

            var deleteUsers = survey.SurveyFormDataInstances.Where(fd => !surveyDTO.SurveyedUsers.Contains(fd.UserId)).ToList();

            foreach (var userId in surveyedUsers)
            {
                await _context.Set<FormData>().AddAsync(new FormData
                {
                    CreatedOn = DateTime.UtcNow,
                    UserId = userId,
                    Json = "{}",
                    FormDefinitionId = definitionId,
                    SurveyId = survey.Id
                });
            }

            await _context.SaveChangesAsync(ct);

            // TODO: do we delete the existing FormDatas of previously selected users?
            if (deleteUsers.Any())
            {
                foreach (var fd in deleteUsers)
                {
                    if (fd.Json.Length <= 3)
                        await _context.Set<FormData>().Where(x => x.Id == fd.Id).DeleteFromQueryAsync();
                }

                await _context.SaveChangesAsync(ct);
            }

            await _context.Set<FormData>().Where(x => x.SurveyId == survey.Id)
                .UpdateFromQueryAsync(x => new FormData { FormDefinitionId = definitionId });


            return _mapper.Map<FormSurveyDTO>(await _context.Set<FormSurvey>().Where(s => s.Id == surveyDTO.Id)
                .Include(x => x.SurveyFormDataInstances)
                .ThenInclude(y => y.CreatedBy)
                .Include(x => x.SurveyFormDataInstances)
                .Include(y => y.FormRevision)
                .FirstOrDefaultAsync(ct));
        }

        public async Task Delete(int id, CancellationToken ct = default)
        {
            if (await _context.Set<FormData>().AnyAsync(x => x.SurveyId == id && x.Json.Length > 3))
            {
                await _dataService.Context.Set<FormData>().Where(x => x.SurveyId == id).DeleteFromQueryAsync();
                await _dataService.Delete<FormSurvey>(id, ct);
                return;
            }

            throw new BusinessException("The selected survey has data associated and can't be deleted.");
        }

        public async Task<List<UserSuggestionDTO>> GetAllUserSuggestions(QueryCommand command)
        {
            var isAdmin = false;
            if (command.Filters.Any(x => x.PropertyName == "isAdmin"))
            {
                var adminRole = (command.Filters.FirstOrDefault(x => x.PropertyName == "isAdmin") as BooleanFilter);
                isAdmin = adminRole?.Value ?? false;
                command.Filters.RemoveAll(x => x.PropertyName == "isAdmin");
            }

            var orgIds = new List<int>();
            if (command.Filters.Any(x => x.PropertyName == "orgIds"))
            {
                var ids = (command.Filters.FirstOrDefault(x => x.PropertyName == "orgIds") as NumberArrayFilter)?.Value;
                if (ids?.Any() ?? false) orgIds.AddRange(ids);
                command.Filters.RemoveAll(x => x.PropertyName == "orgIds");
            }

            return await this._context.Set<User>()
                .Where(u => isAdmin || u.UserOrganizations.Any(org => orgIds.Any(orgId => orgId == org.OrganizationId)))
                .Select(x => new UserSuggestionDTO() { Id = x.Id, Name = x.FirstName + " " + x.LastName, UserName = x.UserName ?? "" })
                .ToListAsync();
        }

        public async Task<List<FormRevisionSuggestionDTO>> GetAllFormRevisionsSuggestions(QueryCommand command)
        {
            var isAdmin = false;
            if (command.Filters.Any(x => x.PropertyName == "isAdmin"))
            {
                var adminRole = (command.Filters.FirstOrDefault(x => x.PropertyName == "isAdmin") as BooleanFilter);
                isAdmin = adminRole?.Value ?? false;
                command.Filters.RemoveAll(x => x.PropertyName == "isAdmin");
            }

            var orgIds = new List<int>();
            if (command.Filters.Any(x => x.PropertyName == "orgIds"))
            {
                var ids = (command.Filters.FirstOrDefault(x => x.PropertyName == "orgIds") as NumberArrayFilter)?.Value;
                if (ids?.Any() ?? false) orgIds.AddRange(ids);
                command.Filters.RemoveAll(x => x.PropertyName == "orgIds");
            }

            return await this._context.Set<FormDefinition>()
                .Where(fd => (isAdmin || (fd.FormDefinitionOrganizations != null &&
                                          fd.FormDefinitionOrganizations.Any(org => orgIds.Any(orgId => orgId == org.OrganizationId)))))
                .Select(x => new FormRevisionSuggestionDTO { Id = x.ActiveRevisionId, Name = x.Name }).ToListAsync();
        }

        public Task<List<FormSurveyDataDTO>> GetSurveyData(int id)
        {
            return _context.Set<FormData>().Where(x => x.SurveyId == id)
                .Select(data => new
                {
                    data.Id,
                    data.Json,
                    UserID = data.UserId,
                    data.SurveyId,
                    Createdby = data.CreatedBy,
                    ActiveRevision = data.FormDefinition!.FormRevisions.FirstOrDefault(revision => revision.Id == data.FormDefinition.ActiveRevisionId)
                })
                .Select(data => new FormSurveyDataDTO
                {
                    Id = data.Id,
                    Json = data.Json,
                    RespondentId = data.UserID,
                    RespondentFullName = data.Createdby.FirstName + " " + data.Createdby.LastName,
                    RespondentUserName = data.Createdby.UserName ?? " - ",
                    FormRevisionJson = data.ActiveRevision != null && data.ActiveRevision.Json != null ? data.ActiveRevision.Json : " - ",
                    Survey = new FormSurveyDTO { Id = (int)data.SurveyId! }
                }).ToListAsync();
        }

        public async Task<PageResult<FormSurveyPageDTO>> GetPage(QueryCommand command, CancellationToken ct = default)
        {
            var isAdmin = false;
            if (command.Filters.Any(x => x.PropertyName == "isAdmin"))
            {
                var adminRole = command.Filters.FirstOrDefault(x => x.PropertyName == "isAdmin") as BooleanFilter;
                isAdmin = adminRole?.Value ?? false;
                command.Filters.RemoveAll(x => x.PropertyName == "isAdmin");
            }

            var orgIds = new List<int>();
            if (command.Filters.Any(x => x.PropertyName == "orgIds"))
            {
                var ids = (command.Filters.FirstOrDefault(x => x.PropertyName == "orgIds") as NumberArrayFilter)?.Value;
                if (ids?.Any() ?? false) orgIds.AddRange(ids);
                command.Filters.RemoveAll(x => x.PropertyName == "orgIds");
            }

            var result = await _dataService.GetPage<FormSurvey, FormSurveyPageDTO>(
                command,
                query => QueryHandler(query, isAdmin, orgIds),
                filter: Filter,
                sorter: Sorter,
                ct);

            return result;
        }

        private static IQueryable<FormSurvey> QueryHandler(IQueryable<FormSurvey> query, bool isAdmin, List<int> orgIds)
        {
            return query
                .Include(surveys => surveys.FormRevision)
                .ThenInclude(formRevision => formRevision.FormDefinition)
                .ThenInclude(def => def.FormDefinitionOrganizations)
                .ThenInclude(def => def.Organization)
                .Where(s => isAdmin || s.FormRevision.FormDefinition.FormDefinitionOrganizations
                    .Any(org => orgIds.Any(orgId => orgId == org.OrganizationId)));
        }

        private IQueryable<FormSurvey> Sorter(IQueryable<FormSurvey> query, ISorter sorter)
        {
            switch (sorter.SortingField)
            {
                case "formDefinitionName":
                    query = sorter.SortingDirection == OrderDirection.Asc
                        ? query.OrderBy(survey => survey.FormRevision.FormDefinition!.Name)
                        : query.OrderByDescending(survey => survey.FormRevision.FormDefinition!.Name);
                    sorter.SortingField = null;
                    break;
                case "version":
                    query = sorter.SortingDirection == OrderDirection.Asc
                        ? query.OrderBy(survey => survey.FormRevision.MajorVersion)
                            .ThenBy(survey => survey.FormRevision.MinorVersion)
                        : query.OrderByDescending(survey => survey.FormRevision.MajorVersion)
                            .ThenByDescending(survey => survey.FormRevision.MinorVersion);
                    sorter.SortingField = null;
                    break;
                case "orgs":
                    query = sorter.SortingDirection == OrderDirection.Asc
                        ? query.OrderBy(survey => survey.FormRevision.FormDefinition.FormDefinitionOrganizations.Count)
                            .ThenBy(survey => survey.FormRevision.FormDefinition.FormDefinitionOrganizations.First().FormDefinition.Name ?? "")
                        : query.OrderByDescending(survey => survey.FormRevision.FormDefinition.FormDefinitionOrganizations.Count)
                            .ThenByDescending(survey => survey.FormRevision.FormDefinition.FormDefinitionOrganizations.First().FormDefinition.Name ?? "");
                    sorter.SortingField = null;
                    break;
            }

            return query;
        }

        private QueryFilter<FormSurvey> Filter(QueryFilter<FormSurvey> queryFilter)
        {
            return queryFilter;
        }
    }
}