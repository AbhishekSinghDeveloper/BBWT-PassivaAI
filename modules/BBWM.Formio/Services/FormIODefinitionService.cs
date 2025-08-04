using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Filters;
using BBWM.Core.Filters.TypedFilters;
using BBWM.Core.Membership.Model;
using BBWM.Core.Services;
using BBWM.Core.Web.Extensions;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Extensions;
using BBWM.FormIO.Interfaces;
using BBWM.FormIO.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using BBWM.FormIO.Interfaces.FormViewInterfaces;

namespace BBWM.FormIO.Services
{
    public class FormIODefinitionService : IFormIODefinitionService
    {
        private readonly IDbContext _context;
        private readonly string _currentUserId;
        private readonly IDataService _dataService;
        private readonly IFormViewService _viewService;
        private readonly IFormioIORevisionService _formioIORevisionService;
        private readonly DatabaseType _dbType;
        private readonly BBF.Reporting.Core.Interfaces.IQueryProviderFactory _queryProviderFactory;

        public FormIODefinitionService(
            IDbContext context,
            IDataService dataService,
            IFormViewService viewService,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IFormioIORevisionService formioIORevisionService,
            BBF.Reporting.Core.Interfaces.IQueryProviderFactory queryProviderFactory)
        {
            _context = context;
            _dataService = dataService;
            _viewService = viewService;
            _dbType = configuration.GetDatabaseConnectionSettings().DatabaseType;
            _currentUserId = httpContextAccessor.HttpContext.GetUserId();
            _formioIORevisionService = formioIORevisionService;
            _queryProviderFactory = queryProviderFactory;
        }

        public async Task Delete(int id, CancellationToken ct = default)
        {
            var revisionIds = await _dataService.Context.Set<FormRevision>()
                .Where(revision => revision.FormDefinitionId == id)
                .Select(revision => revision.Id).ToListAsync();

            var hasMUFs = _dataService.Context.Set<MultiUserFormDefinition>()
                .Where(multiUserFormDefinition => multiUserFormDefinition.FormRevisionId != null)
                .Any(multiUserFormDefinition => revisionIds.Contains(multiUserFormDefinition.FormRevisionId!.Value));

            var hasFormData = _dataService.Context.Set<FormData>().Any(formData => id == formData.FormDefinitionId);

            if (hasMUFs || hasFormData)
            {
                throw new BusinessException("This Form Design has Instances or MultiUserForms with data, so it can't be Deleted.");
            }

            // Delete form views related data (form definition view,
            // form grid data, form revision grids, and form revision grid views).
            await _viewService.DeleteViewRelatedData(id, ct);

            // delete all revisions
            await _dataService.DeleteAll<FormRevision>(query => query.Where(revision => revision.FormDefinitionId == id), ct);

            await _dataService.Delete<FormDefinition, int>(id, ct);
        }

        /// <summary>
        /// Remove all trailing numbers of the string parameter
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string RemoveTrailingNumbers(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            const string pattern = @"\d+$";
            const string replacement = "";
            Regex rgx = new Regex(pattern);
            return rgx.Replace(value, replacement);
        }

        private bool IsMultipleValueComponent(JToken value)
        {
            return value?.Value<string>("type") == "select";
        }

        public async Task<FormDefinitionComposedDTO> GetFormDefinitionJson(int? id, int? revisionId, bool readOnly, List<string> paramList,
            CancellationToken cancellationToken)
        {
            var result = id is null
                ? null
                : await _dataService.Get<FormDefinition, FormDefinitionComposedDTO>((int)id, query => query.Include(x => x.FormRevisions), cancellationToken);
            // If revisionId is set, then get that particular revision data, if not, then get the Active Revision's data
            if (result == null && revisionId.HasValue)
            {
                var form = await _context.Set<FormRevision>().Where(x => x.Id == revisionId).FirstOrDefaultAsync();
                result = await _dataService.Get<FormDefinition, FormDefinitionComposedDTO>((int)form.FormDefinitionId,
                    query => query.Include(x => x.FormRevisions), cancellationToken);
            }

            var revision = revisionId.HasValue
                ? await _dataService.Get<FormRevision, FormRevisionMinDTO>(revisionId.Value, cancellationToken)
                : result.ActiveRevision;

            if (revision != null)
            {
                result.Json = revision?.Json;
                result.MobileFriendly = revision.MobileFriendly;
            }

            // Only process to get values from DB if form is not in read-only mode
            if (!readOnly)
            {
                var parameterListEntity = await _dataService.GetAll<FormParameterList, FormParameterListDTO>(cancellationToken);
                JObject json = JObject.Parse(revision!.Json!);
                var list = json?.First?.First != null
                    ? json?.First?.First?.GetInnerFormDefinitionComponents(x =>
                        x.Value<string>("type") == "panel" || x.Value<string>("type") == "select" || x.Value<string>("type") == "textfield" ||
                        x.Value<string>("type") == "textarea" || x.Value<string>("type") == "day" || x.Value<string>("type") == "signature" ||
                        x.Value<string>("type") == "datagrid")
                    : new List<JToken>();
                var identifiers = new string[] { "dbo.", "nq." };
                // Group components by api key and if they are multi or single value component, this way we can have both a single and multi value api key at the same time
                var components = list!.Where(token => identifiers.Any(str => RemoveTrailingNumbers(token.Value<string>("key") ?? "")?.StartsWith(str) ?? false))
                    .GroupBy(token => new KeyValuePair<string, bool>(RemoveTrailingNumbers(token.Value<string>("key") ?? ""), IsMultipleValueComponent(token)))
                    .Select(x => x.First()).ToList();
                if (components != null && components.Any())
                {
                    components.ForEach(item =>
                    {
                        var key = item.Value<string>("key") ?? "";
                        var tag = item.Value<JArray>("tags");
                        var multiValues = IsMultipleValueComponent(item);
                        var columns = new List<string>();
                        // Extract columns to be used
                        if (item.Value<string>("type") == "datagrid")
                        {
                            var childrens = item.Value<JArray>("components");
                            if (childrens != null && childrens.Any())
                            {
                                foreach (var columnHolder in childrens)
                                {
                                    if (columnHolder.Value<string>("type") == "textfield")
                                        columns.Add($"'{columnHolder.Value<string>("key")}',{columnHolder.Value<string>("key")}" ?? string.Empty);
                                }
                            }

                            // remove all empty columns
                            columns = columns.Where(x => !string.IsNullOrEmpty(x)).ToList();
                        }

                        // The source is a Named Query
                        if (key.StartsWith("nq"))
                        {
                            var columnKeys = columns.Select(x => x.Split(',').Last()).ToList();
                            ;
                            var queryId = key.Split('.').Skip(1).Aggregate((x, y) => x = $"{x}.{y}");
                            try
                            {
                                var queryGUID = new Guid(queryId);
                                var qp = _queryProviderFactory.GetQueryProvider(queryGUID);
                                var dataRows = qp.GetQueryDataRows(queryGUID).Result;
                                var rows = dataRows
                                    .Select(x => (IDictionary<string, object>)x)
                                    .Select(x => new FormJsonSelectDBFieldValue
                                        {
                                            Label = "",
                                            Value = "{" + x
                                                .Where(obj => columnKeys.Any(z => z == obj.Key))
                                                .Select(obj => $"\"{obj.Key}\":\"{obj.Value.ToString()}\"")
                                                .Aggregate((curr, next) => $"{curr},{next}") + "}"
                                        }
                                    )
                                    .ToList();
                                var field = new FormJsonSelectDBField()
                                {
                                    FieldKey = key,
                                    MultiValue = multiValues,
                                    Values = new List<FormJsonSelectDBFieldValue>()
                                };
                                field.Values.AddRange(rows);
                                result.Fields.Add(field);
                            }
                            catch (Exception)
                            {
                            }
                        }
                        else
                        {
                            var parameterEntity = parameterListEntity.Where(pl => tag?.Any(x => x.ToString() == pl.Name) ?? false).ToList();
                            if (parameterEntity.Any())
                            {
                                if (!string.IsNullOrEmpty(key))
                                {
                                    var field = new FormJsonSelectDBField()
                                    {
                                        FieldKey = key,
                                        MultiValue = multiValues,
                                        Values = new List<FormJsonSelectDBFieldValue>()
                                    };

                                    // Fetch values from DB
                                    var dbParts = key.Split('.');
                                    if (item.Value<string>("type") == "datagrid") // means the entire table
                                    {
                                        var tableName = dbParts.Skip(1).Aggregate((x, y) => x = $"{x}.{y}");
                                        parameterEntity = parameterEntity.Where(x => x.TableName == tableName).ToList();

                                        if (parameterEntity.Any())
                                        {
                                            string whereClause = "WHERE ";
                                            // Check for custom filter and generate where section
                                            parameterEntity.ForEach(param =>
                                            {
                                                if (param.Position.HasValue && param.Position.Value > 0)
                                                {
                                                    // For user's simplicity, parameter list starts at 1, as collections starts at 0, adding 1 prevent index out of range exception
                                                    var value = (paramList.Count + 1) >= param.Position ? paramList[param.Position.Value - 1] : null;
                                                    whereClause += value != null ? $"{param.KeyField}='{value}' AND" : string.Empty;
                                                }
                                            });
                                            whereClause = whereClause.Length == 6 ? string.Empty : whereClause.Substring(0, whereClause.Length - 4);
                                            try
                                            {
                                                // Data type to cast the result into, differs due to DB server type
                                                var dataType = _dbType == DatabaseType.MySql ? "nchar" : "varchar";
                                                // This is a workaround, ideally, the query string as Formattable string should work but it didn't, debugging the EF SqlQuery call...
                                                // ... shows that the parameters list (in this case 2) are being passed as only 1 parameter (object array) so when internally it tries to interpolate, the parameter [1] doesnt exist
                                                // so calling _context.Database.SqlQuery<string>($"SELECT [{tableField}] FROM dbo.{tableName}")   fails with "error around @p1"
                                                // To solve this, I create the formattable string with the string already interpolated and with no parameters (empty array) to avoid the interpolation in SQL.
                                                var queryString =
                                                    $"SELECT JSON_OBJECT({columns.Aggregate((a, b) => a + ", " + b)}) FROM {tableName} {whereClause}";
                                                var query = FormattableStringFactory.Create(queryString.TrimEnd(), new object[0]);
                                                var data = _context.Database.SqlQuery<string>(query).ToList()
                                                    .Select(x => new FormJsonSelectDBFieldValue { Label = "", Value = x });
                                                field.Values.AddRange(data);
                                            }
                                            catch (Exception ex)
                                            {
                                                // Do something
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var tableName = dbParts[1];
                                        var tableField = dbParts[2];
                                        parameterEntity = parameterEntity.Where(x => x.TableName == tableName).ToList();

                                        // Check table name parameter validity
                                        if (parameterEntity.Any())
                                        {
                                            string whereClause = "WHERE ";
                                            // Check for custom filter and generate where section
                                            parameterEntity.ForEach(param =>
                                            {
                                                if (!string.IsNullOrEmpty(param.KeyField) && param.Position.HasValue && param.Position.Value > 0)
                                                {
                                                    // For user's simplicity, parameter list starts at 1, as collections starts at 0, adding 1 prevent index out of range exception
                                                    var value = (paramList.Count + 1) >= param.Position ? paramList[param.Position.Value - 1] : null;
                                                    whereClause += value != null ? $"{param.KeyField}='{value}' AND" : string.Empty;
                                                }
                                            });
                                            whereClause = whereClause.Length == 6 ? string.Empty : whereClause.Substring(0, whereClause.Length - 4);
                                            try
                                            {
                                                // Data type to cast the result into, differs due to DB server type
                                                var dataType = _dbType == DatabaseType.MySql ? "nchar" : "varchar";
                                                // This is a workaround, ideally, the query string as Formattable string should work but it didn't, debugging the EF SqlQuery call...
                                                // ... shows that the parameters list (in this case 2) are being passed as only 1 parameter (object array) so when internally it tries to interpolate, the parameter [1] doesnt exist
                                                // so calling _context.Database.SqlQuery<string>($"SELECT [{tableField}] FROM dbo.{tableName}")   fails with "error around @p1"
                                                // To solve this, I create the formattable string with the string already interpolated and with no parameters (empty array) to avoid the interpolation in SQL.
                                                var queryString = $"SELECT CAST({tableName}.{tableField} as {dataType}) FROM {tableName} {whereClause}";
                                                var query = FormattableStringFactory.Create(queryString.TrimEnd(), new object[0]);
                                                var data = _context.Database.SqlQuery<string>(query).ToList()
                                                    .Select(x => new FormJsonSelectDBFieldValue { Label = x, Value = x });
                                                field.Values.AddRange(data);
                                            }
                                            catch (Exception ex)
                                            {
                                                // Do something
                                            }
                                        }
                                    }

                                    result.Fields.Add(field);
                                }
                            }
                        }
                    });
                }
            }

            return result;
        }

        public async Task<PageResult<FormDefinitionPageDTO>> GetPage(QueryCommand command, CancellationToken ct = default)
        {
            #region Apply filtering by published status, isAdmin and user orgs

            var isPublishedFiltering = false;
            var isPublishedFilteringValue = false;
            if (command.Filters.Any(x => x.PropertyName == "isPublished"))
            {
                isPublishedFiltering = true;
                isPublishedFilteringValue = (command.Filters.FirstOrDefault(x => x.PropertyName == "isPublished") as BooleanFilter)?.Value ?? false;
                command.Filters.RemoveAll(x => x.PropertyName == "isPublished");
            }

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

            var userId = string.Empty;
            if (command.Filters.Any(x => x.PropertyName == "userId"))
            {
                userId = (command.Filters.FirstOrDefault(x => x.PropertyName == "userId") as StringFilter)?.Value ?? string.Empty;
                command.Filters.RemoveAll(x => x.PropertyName == "userId");
            }

            #endregion

            var result = await _dataService.GetPage<FormDefinition, FormDefinitionPageDTO>(command,
                query => FormDefinitionQueryGenerator(query, orgIds, isAdmin, new KeyValuePair<bool, bool>(isPublishedFiltering, isPublishedFilteringValue),
                    userId),
                filter: Filter,
                sorter: Sorter,
                ct: ct);

            foreach (var item in result.Items)
            {
                var orgs = string.Join(" ,",
                    await _dataService.Context.Set<Organization>().Where(x => item.OrganizationIds.Any(id => id == x.Id)).Select(x => x.Name).ToListAsync());
                item.Org = orgs;
                item.Creator = (await _dataService.Context.Set<User>().FirstOrDefaultAsync(x => x.Id == item.ManagerId))?.UserName ?? "-";
                /*
                var user = await _dataService.Context
                    .Set<User>()
                    .Include(x => x.UserOrganizations)
                        .ThenInclude(userOrg => userOrg.Organization)
                    .FirstOrDefaultAsync(x => x.Id == item.ManagerId);

                if (user is null) continue;
                item.Creator = user?.UserName ?? "-";
                item.Org = user!.UserOrganizations.Any()
                            ? string.Join(", ", user!.UserOrganizations!.Select(org => org.Organization.Name))
                            : "-";
                */
            }

            return result;
        }

        private IQueryable<FormDefinition> FormDefinitionQueryGenerator(IQueryable<FormDefinition> query, List<int> orgIds, bool isAdmin,
            KeyValuePair<bool, bool> isPublishedFiltering, string userId)
        {
            // Include required tables
            query = query
                .Include(formDefinition => formDefinition.FormRevisions)
                .Include(formDefinition => formDefinition.FormData)
                .Include(formDefinition => formDefinition.FormDefinitionOrganizations)
                .Include(formDefinition => formDefinition.FormCategory);

            // Apply filter for is published as required
            query = isPublishedFiltering.Key
                ? query.Where(x => isPublishedFiltering.Value ? x.FormDefinitionOrganizations.Any() : !x.FormDefinitionOrganizations.Any())
                : query;
            // apply filter for orgs and admin status
            query = isAdmin
                ? query
                : query.Where(x => x.FormDefinitionOrganizations
                    .Any(org => orgIds.Any(id => id == org.OrganizationId)) || x.ManagerId == userId);
            return query;
        }

        public async Task<FormDefinitionDTO> Update(FormDefinitionDTO dto, CancellationToken ct = default)
        {
            var entityDTO = await _dataService.Get<FormDefinition, FormDefinitionDTO>(dto.Id, ct);

            if (entityDTO == null)
            {
                throw new ArgumentNullException(nameof(dto));
                //entity.CreatedOn = DateTime.UtcNow;
            }

            var definition = await _dataService.Update<FormDefinition, FormDefinitionDTO>(entityDTO, ct);

            var activeRevision = await _dataService.Get<FormRevision, FormRevisionMinDTO>(definition.ActiveRevisionId, ct);
            definition.ActiveRevision = activeRevision;

            // Update form views related data (form revision view name if it's necessary,
            // form definition view, form revision grids, and form revision grid views).
            await _viewService.UpdateViewRelatedData(definition.Id, ct);
            return definition;
        }

        private async Task<string> SetFormCopyName(string name)
        {
            var originalFileName = name;
            string pattern = Regex.Escape(originalFileName) + @" - Copy(\(\d+\))?";

            // Extract the relevant forms that match the pattern
            var matchingFormNames = await _dataService.Context.Set<FormDefinition>()
                .AsNoTracking()
                .Where(f => Regex.IsMatch(f.Name, pattern))
                .Select(f => f.Name)
                .ToListAsync();

            // If the direct copy does not exist, use it
            if (!matchingFormNames.Contains(originalFileName + " - Copy"))
            {
                name = originalFileName + " - Copy";
                return name;
            }

            // Otherwise, find the first available index
            int copyIndex = 2;
            string copyName;
            do
            {
                copyName = $"{originalFileName} - Copy({copyIndex})";
                copyIndex++;
            } while (matchingFormNames.Contains(copyName));

            return copyName;
        }

        public async Task<FormDefinitionDTO> Create(FormDefinitionForNewRequestDTO dto, CancellationToken ct)
        {
            var formDefinitionCreated = new FormDefinitionDTO();

            JObject json = JObject.Parse(dto.FormRevisionData.Json);
            var list = json?.First?.First != null
                ? json?.First?.First?.GetInnerFormDefinitionComponents(x => x.Value<string>("type") == "state-tabs")
                : new List<JToken>();
            if (list?.Count > 1)
            {
                throw new BusinessException("There can be only one 'state-tabs' component");
            }

            try
            {
                // when creating a new FormDefinition
                var formDto = new FormDefinitionDTO()
                {
                    Id = 0,
                    ActiveRevisionId = 0,
                    IsPublished = false,
                    ManagerId = dto.ManagerId ?? _currentUserId,
                    ByRequestOnly = dto.ByRequestOnly,
                    Name = dto.Name,
                    FormCategoryId = dto.FormCategoryId,
                    OrganizationIds = new List<int>(),
                };

                formDefinitionCreated = await _dataService.Create<FormDefinition, FormDefinitionDTO>(formDto, ct);

                // create a initial FormRevision for the FormDefinition
                var createdRevision = await _formioIORevisionService.CreateInitialFormRevision(formDefinitionCreated.Id,
                    new InitialFormRevisionRequestDTO
                    {
                        Json = dto.FormRevisionData.Json,
                        MobileFriendly = dto.FormRevisionData.MobileFriendly,
                        MUFCapable = list?.Any() ?? false,
                    }
                    , ct);

                formDefinitionCreated.ActiveRevisionId = createdRevision.Id;

                // TODO: review
                formDefinitionCreated.ActiveRevision = new FormRevisionMinDTO()
                {
                    Id = createdRevision.Id,
                    DateCreated = createdRevision.DateCreated,
                    Json = createdRevision.Json,
                    //VId = createdRevision.VId
                };

                await _dataService.Update<FormDefinition, FormDefinitionDTO>(formDefinitionCreated, ct);

                // Update form views related data (form revision view name if it's necessary,
                // form definition view, form revision grids, and form revision grid views).
                await _viewService.UpdateViewRelatedData(formDefinitionCreated.Id, ct);
            }
            catch (Exception ex)
            {
                throw new BusinessException(ex.Message);
            }

            return formDefinitionCreated;
        }

        public async Task<int> Copy(FormDefinitionDTO dto, CancellationToken cancellationToken)
        {
            var formDefinitionDTO = new FormDefinitionDTO
            {
                //Json = dto.ActiveRevision!.Json ?? null,
                ActiveRevisionId = 0,
                Id = 0,
                //MajorVersion = 0,
                //MinorVersion = 0,
                FormCategoryId = dto.FormCategoryId,
                ByRequestOnly = dto.ByRequestOnly,
                ManagerId = dto.ManagerId,
                Name = dto.Name
            };

            formDefinitionDTO.Name = await SetFormCopyName(formDefinitionDTO.Name!);

            var copyForm = await _dataService.Create<FormDefinition, FormDefinitionDTO>(formDefinitionDTO, cancellationToken);

            var revisionCreated = await _formioIORevisionService.CreateInitialFormRevision(copyForm.Id,
                new InitialFormRevisionRequestDTO
                {
                    Json = dto.ActiveRevision.Json!,
                    MobileFriendly = dto.ActiveRevision.MobileFriendly,
                    MUFCapable = dto.ActiveRevision.MUFCapable
                }
                , cancellationToken);

            copyForm.ActiveRevisionId = revisionCreated.Id;
            await _dataService.Update<FormDefinition, FormDefinitionDTO>(copyForm, cancellationToken);

            // TODO: review
            copyForm.ActiveRevision = new FormRevisionMinDTO()
            {
                Id = revisionCreated.Id,
                DateCreated = revisionCreated.DateCreated,
                Json = revisionCreated.Json,
                // VId = revisionCreated.VId
            };

            // Update form views related data (form revision view name if it's necessary,
            // form definition view, form revision grids, and form revision grid views).
            await _viewService.UpdateViewRelatedData(copyForm.Id, cancellationToken);
            return copyForm.Id;
        }

        public async Task<FormDefinitionDTO> Get(int id, CancellationToken ct = default)
        {
            var entity = await _dataService.Get<FormDefinition, FormDefinitionDTO>(id, query => query.Include(x => x.FormRevisions), ct);
            return entity;
        }

        public async Task<bool> PublishFormDefinition(PublishFormDefinitionDTO publishFormDefinitionDTO, CancellationToken cancellationToken)
        {
            await _dataService.Context.Set<FormDefinitionOrganization>()
                .Where(x => x.FormDefinitionId == publishFormDefinitionDTO.FormId)
                .DeleteFromQueryAsync(cancellationToken);
            _dataService.Context.Set<FormDefinitionOrganization>()
                .AddRange(publishFormDefinitionDTO.OrgIds
                    .Select(id => new FormDefinitionOrganization { FormDefinitionId = publishFormDefinitionDTO.FormId, OrganizationId = id }));
            var count = await _dataService.Context.SaveChangesAsync();
            await _dataService.Context.Set<FormDefinition>().Where(x => x.Id == publishFormDefinitionDTO.FormId)
                .UpdateFromQueryAsync(x => new FormDefinition { FormCategoryId = publishFormDefinitionDTO.FormCat });
            return count > 0;
        }

        public List<string> GetAvailableVersionsForFiltering(List<int> orgIds, bool isAdmin, string userId, CancellationToken cancellationToken)
        {
            var formDefinitions = _dataService.Context.Set<FormDefinition>().AsQueryable();

            var query = FormDefinitionQueryGenerator(formDefinitions, orgIds, isAdmin, new KeyValuePair<bool, bool>(false, false), userId);

            var activeFormRevisions = query
                .Select(formDefinition => formDefinition.ActiveRevisionId);

            var versions = _dataService.Context.Set<FormRevision>()
                .Where(formRevision => activeFormRevisions.Contains(formRevision.Id))
                .Select(formRevision => new { formRevision.MajorVersion, formRevision.MinorVersion })
                .ToHashSet();

            var availableVersions = versions
                .OrderBy(version => version.MajorVersion)
                .ThenBy(version => version.MinorVersion)
                .Select(valueType => $"{valueType.MajorVersion}.{valueType.MinorVersion}")
                .ToList();

            return availableVersions;
        }

        private IQueryable<FormDefinition> Sorter(IQueryable<FormDefinition> query, ISorter sorter)
        {
            switch (sorter.SortingField)
            {
                case "creator":
                    query = sorter.SortingDirection == OrderDirection.Asc
                        ? query.OrderBy(fDef => fDef.Manager.UserName)
                        : query.OrderByDescending(fDef => fDef.Manager.UserName);
                    sorter.SortingField = null;
                    break;
                case "org":
                    query = sorter.SortingDirection == OrderDirection.Asc
                        ? query.OrderBy(fDef => fDef.Manager.Organization.Name)
                        : query.OrderByDescending(fDef => fDef.Manager.Organization.Name);
                    sorter.SortingField = null;
                    break;
                case "activeRevision.dateCreated":
                    query = sorter.SortingDirection == OrderDirection.Asc
                        ? query.OrderBy(fDef => fDef.FormRevisions.FirstOrDefault(fRev => fRev.Id == fDef.ActiveRevisionId).DateCreated)
                        : query.OrderByDescending(fDef => fDef.FormRevisions.First(fRev => fRev.Id == fDef.ActiveRevisionId).DateCreated);
                    sorter.SortingField = null;
                    break;
                case "category":
                    query = sorter.SortingDirection == OrderDirection.Asc
                        ? query.OrderBy(fDef => fDef.FormCategory.Name)
                        : query.OrderByDescending(fDef => fDef.FormCategory.Name);
                    sorter.SortingField = null;
                    break;
                case "activeRevision.majorVersion":
                    query = sorter.SortingDirection == OrderDirection.Asc
                        ? query.OrderBy(fDef => fDef.FormRevisions.FirstOrDefault(fRev => fRev.Id == fDef.ActiveRevisionId)!.MajorVersion)
                            .ThenBy(fDef => fDef.FormRevisions.FirstOrDefault(fRev => fRev.Id == fDef.ActiveRevisionId)!.MinorVersion)
                        : query.OrderByDescending(fDef => fDef.FormRevisions.First(fRev => fRev.Id == fDef.ActiveRevisionId).MajorVersion)
                            .ThenByDescending(fDef => fDef.FormRevisions.FirstOrDefault(fRev => fRev.Id == fDef.ActiveRevisionId)!.MinorVersion);
                    sorter.SortingField = null;
                    break;
                case "isPublished":
                    // how do you sort by this field?
                    query = sorter.SortingDirection == OrderDirection.Asc
                        ? query.OrderBy(fDef => fDef.FormDefinitionOrganizations!.Any())
                        : query.OrderByDescending(fDef => fDef.FormDefinitionOrganizations!.Any());
                    sorter.SortingField = null;
                    break;
                case "activeRevision.mufCapable":
                    query = sorter.SortingDirection == OrderDirection.Asc
                        ? query.OrderBy(fDef => fDef.FormRevisions.First(fRev => fRev.Id == fDef.ActiveRevisionId).MUFCapable)
                        : query.OrderByDescending(fDef => fDef.FormRevisions.First(fRev => fRev.Id == fDef.ActiveRevisionId).MUFCapable);
                    sorter.SortingField = null;
                    break;
                default:
                    break;
            }

            return query;
        }

        private QueryFilter<FormDefinition> Filter(QueryFilter<FormDefinition> queryFilter)
        {
            return queryFilter
                .Handle<StringFilter>("creator", (query, filter) => query
                    .Where(fDef => fDef.Manager!.UserName != null && fDef.Manager.UserName.ToLower().Contains(filter.Value.ToLower())))
                .Handle<StringFilter>("org", (query, filter) => query
                    .Where(fDef => fDef.Manager!.Organization.Name.ToLower().Contains(filter.Value.ToLower())))
                .Handle<DateFilter>("activeRevision.dateCreated", (query, filter) => query
                    .Where(fDef => fDef.FormRevisions.FirstOrDefault(fRev => fRev.Id == fDef.ActiveRevisionId)!.DateCreated > filter.Value
                                   && fDef.FormRevisions.FirstOrDefault(fRev => fRev.Id == fDef.ActiveRevisionId)!.DateCreated.AddDays(1) <= filter.Value))
                .Handle<NumberFilter>("FormCategoryId", (query, filter) => query.Where(fDef => fDef.FormCategoryId == filter.Value))
                .Handle<StringArrayFilter>("Version", HandleFilterByVersion());
        }

        private static Func<IQueryable<FormDefinition>, StringArrayFilter, IQueryable<FormDefinition>> HandleFilterByVersion()
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

                    var filteredQuery = query
                        .Where(formDefinition => formDefinition.FormRevisions
                            .Where(formRevision => formRevision.Id == formDefinition.ActiveRevisionId)
                            .Any(formRevision => formRevision.MajorVersion == majorVersion && formRevision.MinorVersion == minorVersion));

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

        public async Task<bool> ChangeFormDesignOwnership(ChangeFormDefinitionOwnerDTO changeFormDefinitionOwner, CancellationToken cancellationToken)
        {
            var user = await _context.Set<User>().FindAsync(changeFormDefinitionOwner.NewOwnerId);
            if (user == null)
            {
                throw new BusinessException("User not found");
            }

            return await _context.Set<FormDefinition>().Where(x => x.Id == changeFormDefinitionOwner.FormDefinitionId)
                .UpdateFromQueryAsync(x => new FormDefinition { ManagerId = changeFormDefinitionOwner.NewOwnerId }) > 0;
        }
    }
}