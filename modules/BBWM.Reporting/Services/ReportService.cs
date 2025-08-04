using AutoMapper;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Filters;
using BBWM.Core.Filters.TypedFilters;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Services;
using BBWM.Core.Web.Extensions;
using BBWM.DbDoc.DTO;
using BBWM.DbDoc.Interfaces;
using BBWM.Reporting.DTO;
using BBWM.Reporting.Enums;
using BBWM.Reporting.Interfaces;
using BBWM.Reporting.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace BBWM.Reporting.Services;

public class ReportService : DataService, IReportService
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly string _currentUserId;
    private readonly IDataService _dataService;
    private readonly UserManager<User> _userManager;
    private readonly IUserService _userService;
    private readonly IDbDocFolderService _dbDocFolderService;
    private readonly IDbDocService _dbDocService;


    public ReportService(
        IDbContext context,
        IMapper mapper,
        IDataService dataService,
        IHttpContextAccessor httpContextAccessor,
        UserManager<User> userManager,
        IUserService userService,
        IDbDocFolderService dbDocFolderService,
        IDbDocService dbDocService) : base(context, mapper)
    {
        _mapper = mapper;
        _context = context;
        _dataService = dataService;
        _userManager = userManager;
        _userService = userService;
        _dbDocFolderService = dbDocFolderService;
        _dbDocService = dbDocService;

        _currentUserId = httpContextAccessor.HttpContext.GetUserId();
    }


    public async Task CancelDraft(Guid reportDraftId, CancellationToken ct = default)
    {
        var report = await GetFullReport(x => x.Id == reportDraftId, true, ct);

        if (report is null)
            throw new ObjectNotExistsException("The report with the specified ID does not exist.");

        if (report.CreatedBy != _currentUserId)
            throw new ForbiddenException("It's forbidden to cancel a report that wasn't created by you.");

        if (!report.IsDraft)
            throw new BusinessException("An already published report cannot be canceled.");

        await DeleteFullReport(report, ct);
    }

    public async Task<ReportDTO> CreateDraftOfNewReport(ReportDTO dto, CancellationToken ct = default)
    {
        if (await UserHasReportDraft(_currentUserId, ct))
            throw new BusinessException($"The user already has an unpublished report.");

        ValidateReportDto(dto);

        if (await ReportUrlSlugExists(dto.UrlSlug, ct))
            throw new BusinessException($"Report with URL slug '{dto.UrlSlug}' already exists.");

        dto.Id = default;
        dto.CreatedBy = _currentUserId;
        dto.UpdatedBy = _currentUserId;
        dto.CreatedOn = DateTime.UtcNow;
        dto.UpdatedOn = dto.CreatedOn;
        dto.IsDraft = true;

        return await _dataService.Create<Report, ReportDTO>(dto,
            (entity, ctx) =>
            {
                foreach (var role in dto.Roles)
                {
                    _context.Set<ReportRole>().Add(
                        new ReportRole { Report = entity, RoleId = role.Id });
                }

                foreach (var permission in dto.Permissions)
                {
                    _context.Set<ReportPermission>().Add(
                        new ReportPermission { Report = entity, PermissionId = permission.Id });
                }
            }, ct);
    }

    public async Task<ReportDTO> ReplaceDraftWithRecent(Guid reportDraftId, CancellationToken ct = default)
    {
        var draft = await GetFullReport(x => x.Id == reportDraftId, true, ct);

        if (draft is null)
            throw new ObjectNotExistsException($"The report draft with the specified ID does not exist.");

        if (!draft.IsDraft)
            throw new BusinessException("The specified draft is already published.");

        if (draft.PublishedReportId is null)
            throw new BusinessException("The specified draft is the draft of a new report.");

        var recentDraft = await GetFullReportQueryable()
            .Where(x => x.PublishedReportId == draft.PublishedReportId && x.IsDraft)
            .OrderByDescending(x => x.UpdatedOn)
            .AsNoTracking()
            .FirstAsync(ct);

        if (recentDraft.Id == reportDraftId)
            throw new BusinessException("The specified draft is already last modified.");

        await DeleteFullReport(draft, ct);

        recentDraft.CreatedBy = draft.CreatedBy;
        recentDraft.UpdatedBy = draft.UpdatedBy;
        ClearReportCopy(recentDraft);
        recentDraft.Id = reportDraftId;
        await _context.Set<Report>().AddAsync(recentDraft, ct);
        await _context.SaveChangesAsync(ct);

        return _mapper.Map<Report, ReportDTO>(recentDraft);
    }

    public async Task<ReportDTO> CreateDraftOfExistingReport(Guid id, CancellationToken ct = default)
    {
        var existingReport = await GetFullReport(x => x.Id == id, false, ct);

        if (existingReport is null)
            throw new ObjectNotExistsException($"The report with the specified ID does not exist.");

        if (existingReport.IsDraft)
            throw new BusinessException("The specified existing report is a draft itself.");

        if (await UserHasReportDraft(_currentUserId, id, ct))
            throw new BusinessException($"The user already has a draft for this report.");

        existingReport.CreatedOn = DateTime.UtcNow;
        existingReport.UpdatedOn = DateTime.UtcNow;
        existingReport.CreatedBy = _currentUserId;
        existingReport.UpdatedBy = _currentUserId;
        existingReport.IsDraft = true;
        existingReport.PublishedReportId = existingReport.Id;
        ClearReportCopy(existingReport);
        await _context.Set<Report>().AddAsync(existingReport, ct);
        await _context.SaveChangesAsync(ct);

        return _mapper.Map<Report, ReportDTO>(await GetCommonReportData(x => x.Id == existingReport.Id, false, ct));
    }

    public async Task<ReportChangeResult> CreateSection(Guid reportDraftId, SectionDTO dto, CancellationToken ct = default)
    {
        var report = await _context.Set<Report>()
            .Include(x => x.Sections)
            .SingleOrDefaultAsync(x => x.Id == reportDraftId, ct);

        if (!await _context.Set<Report>().AnyAsync(x => x.Id == reportDraftId, ct))
            throw new ObjectNotExistsException("The report with the specified ID does not exist.");

        if (!report.IsDraft)
            throw new BusinessException("It's forbidden to change a published report.");

        ValidateSectionDto(dto);

        dto.ReportId = reportDraftId;
        dto.RowIndex = report.Sections.Any() ? report.Sections.Max(x => x.RowIndex) + 1 : 1;
        dto.ColumnIndex = 1;

        report.UpdatedOn = DateTime.UtcNow;

        var entity = _mapper.Map<Section>(dto);

        entity.Id = default;
        entity.View = new View
        {
            GridView = new GridView(),
        };

        var queryFilterSet = new QueryFilterSet { ConditionalOperator = QueryConditionalOperator.And };
        entity.Query = new Query
        {
            RootFilterSet = queryFilterSet,
            QueryFilterSets = new List<QueryFilterSet> { queryFilterSet }
        };

        await _context.Set<Section>().AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);

        return new ReportChangeResult
        {
            ReportUpdatedOn = report.UpdatedOn,
            RequestTargetPart = _mapper.Map<SectionDTO>(entity)
        };
    }

    public async Task<bool> Exists(Guid reportId, CancellationToken ct = default) =>
        await _context.Set<Report>().AnyAsync(x => x.Id == reportId, ct);

    public async Task Delete(Guid id, CancellationToken ct = default)
    {
        var report = await GetFullReport(x => x.Id == id, true, ct);

        if (report is null)
            throw new ObjectNotExistsException("The report with the specified ID does not exist.");

        foreach (var draftId in _context.Set<Report>().Where(x => x.PublishedReportId == id).Select(x => x.Id))
        {
            var fullDraft = await GetFullReport(x => x.Id == draftId, true, ct);
            await DeleteFullReport(fullDraft, ct);
        }

        await DeleteFullReport(report, ct);
    }

    public async Task<ReportChangeResult> DeleteSection(Guid reportDraftId, Guid sectionId, CancellationToken ct = default)
    {
        var report = await _context.Set<Report>()
            .Include(x => x.Sections)
            .SingleOrDefaultAsync(x => x.Id == reportDraftId, ct);

        if (!await _context.Set<Report>().AnyAsync(x => x.Id == reportDraftId, ct))
            throw new ObjectNotExistsException("The report with the specified ID does not exist.");

        if (!report.IsDraft)
            throw new BusinessException("It's forbidden to change a published report.");

        var section = await _context.Set<Section>()
            .Include(x => x.Query).ThenInclude(x => x.QueryFilterSets)
            .SingleOrDefaultAsync(x => x.Id == sectionId, ct);

        if (section.ReportId != report.Id)
            throw new BusinessException("The section with specified ID does not belong to specified report draft.");

        if (section is null)
            throw new ObjectNotExistsException("The section with specified ID doesn't exist.");

        MarkQueryAsDeleted(section.Query);

        report.UpdatedOn = DateTime.UtcNow;

        if (report.Sections.Count(x => x.RowIndex == section.RowIndex) == 1)
        {
            foreach (var sectionItem in report.Sections.Where(x => x.RowIndex > section.RowIndex))
            {
                sectionItem.RowIndex--;
            }
        }
        else
        {
            foreach (var sectionItem in report.Sections.Where(x => x.RowIndex == section.RowIndex && x.ColumnIndex > section.ColumnIndex))
            {
                sectionItem.ColumnIndex--;
            }
        }

        await _dataService.Delete<Section, Guid>(
            section.Id,
            (entity, context) =>
            {
                foreach (var binding in context.Set<QueryFilterBinding>().Where(x => x.MasterDetailSectionId == entity.Id))
                {
                    context.Set<QueryFilterBinding>().Remove(binding);
                }
            },
            ct);

        return new ReportChangeResult { ReportUpdatedOn = report.UpdatedOn };
    }

    public async Task<ReportDTO> GetCurrentUserDraftReport(CancellationToken ct = default) =>
        _mapper.Map<Report, ReportDTO>(await GetCommonReportData(x =>
            x.CreatedBy == _currentUserId && x.IsDraft && !x.PublishedReportId.HasValue, false, ct));

    public async Task<ReportDTO> GetCurrentUserDraftReport(Guid publishedReportId, CancellationToken ct = default)
    {
        if (_context.Set<Report>().All(x => x.Id != publishedReportId))
            throw new ObjectNotExistsException("Published report does not exist.");

        return _mapper.Map<Report, ReportDTO>(await GetCommonReportData(x =>
            x.CreatedBy == _currentUserId && x.IsDraft && x.PublishedReportId.Value == publishedReportId, false, ct));
    }

    public IQueryable<Report> GetEntityQuery(IQueryable<Report> baseQuery)
        => baseQuery
            .Include(x => x.ReportRoles).ThenInclude(x => x.Role)
            .Include(x => x.ReportPermissions).ThenInclude(x => x.Permission)
            .Where(x => !x.IsDraft);

    public Task<PageResult<ReportDTO>> GetPage(QueryCommand command, CancellationToken ct = default)
        => _dataService.GetPage<Report, ReportDTO>(command, GetEntityQuery,
            queryFilter => queryFilter
                .Handle<StringFilter>(nameof(Report.UpdatedBy),
                    (query, filter) => string.IsNullOrEmpty(filter.Value)
                        ? query
                        : query.Where(x => EF.Functions.Like(x.UpdatedByUser.FirstName, $"%{filter.Value}%") ||
                            EF.Functions.Like(x.UpdatedByUser.LastName, $"%{filter.Value}%") ||
                            EF.Functions.Like(x.UpdatedByUser.Email, $"%{filter.Value}%"))),
            ct: ct);

    public async Task<ReportLastUpdatedDraftInfo> GetReportLastUpdatedDraftInfo(Guid reportId, CancellationToken ct = default)
    {
        if (await _context.Set<Report>().AllAsync(x => x.Id != reportId, ct))
            throw new ObjectNotExistsException($"The report with specified ID doesn't exist.");

        var lastUpdatedDraft = await _context.Set<Report>()
            .Include(x => x.UpdatedByUser)
            .AsNoTracking()
            .Where(x => x.IsDraft && x.PublishedReportId == reportId)
            .OrderByDescending(x => x.UpdatedOn)
            .FirstAsync(ct);

        return new ReportLastUpdatedDraftInfo
        {
            DraftId = lastUpdatedDraft.Id,
            Owner = $"{lastUpdatedDraft.UpdatedByUser.FirstName} {lastUpdatedDraft.UpdatedByUser.LastName} ({lastUpdatedDraft.UpdatedByUser.Email})",
            UpdatedOn = lastUpdatedDraft.UpdatedOn
        };
    }

    public async Task<ReportViewDTO> GetReportView(string urgSlug, CancellationToken ct)
    {
        var report = await _context.Set<Report>()
            .Include(x => x.Sections)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.UrlSlug == urgSlug && !x.IsDraft, ct);

        if (report is null)
            throw new ObjectNotExistsException($"The report with URL slug '{urgSlug}' doesn't exist.");

        await CurrentUserHasReportAccess(urgSlug, ct);

        return new ReportViewDTO
        {
            Name = report.Name,
            ShowTitle = report.ShowTitle,
            Sections = _mapper.Map<IEnumerable<SectionDTO>>(report.Sections)
        };
    }

    public Task<IEnumerable<FolderDTO>> GetFolders(CancellationToken ct) =>
        _dbDocFolderService.GetOwnerFolders(ModuleLinkage.DbDocFolderOwnerName, ct);

    public Task<IEnumerable<TableMetadataDTO>> GetFolderTableMatadata(Guid folderId, CancellationToken ct = default) =>
        _dbDocFolderService.GetFolderTableMatadata(folderId, ct);

    public async Task<TableMetadataDTO> GetFullTableMatadata(int tableMetadataId, CancellationToken ct = default)
        => await _dbDocService.GetTableMetadata(tableMetadataId, ct);

    public async Task<Guid> PublishReportDraft(Guid reportDraftId, CancellationToken ct = default)
    {
        var draft = await _context.Set<Report>().SingleOrDefaultAsync(x => x.Id == reportDraftId, ct);

        if (draft is null)
            throw new ObjectNotExistsException("The draft with specified ID does not exist.");

        if (!draft.IsDraft)
            throw new BusinessException("The specified report is already published.");

        if (draft.PublishedReportId is null)
        {
            draft.IsDraft = false;
            draft.CreatedBy = _currentUserId;
            draft.UpdatedBy = _currentUserId;
            draft.CreatedOn = DateTime.UtcNow;
            draft.UpdatedOn = draft.CreatedOn;
            await _context.SaveChangesAsync(ct);

            return draft.Id;
        }

        var updatedReport = await GetFullReport(x => x.Id == draft.Id, false, ct);

        var publishedReportId = (Guid)updatedReport.PublishedReportId;

        ClearReportCopy(updatedReport);
        updatedReport.Id = publishedReportId;
        updatedReport.PublishedReportId = null;
        updatedReport.UpdatedBy = _currentUserId;
        updatedReport.UpdatedOn = DateTime.UtcNow;
        updatedReport.IsDraft = false;

        var deleteReportQueryable = _context.Set<Report>()
            .Include(x => x.Sections).ThenInclude(x => x.Query).ThenInclude(x => x.QueryFilterSets);

        var otherDrafts = await _context.Set<Report>()
            .Where(x => x.PublishedReportId == publishedReportId && x.Id != draft.Id)
            .ToListAsync(ct);

        foreach (var otherDraft in otherDrafts)
        {
            otherDraft.PublishedReportId = null;
        }

        var fullDraft = await deleteReportQueryable.SingleOrDefaultAsync(x => x.Id == draft.Id, ct);
        await DeleteFullReport(fullDraft, ct);

        var fullPublishedReport = await deleteReportQueryable.SingleOrDefaultAsync(x => x.Id == draft.PublishedReportId, ct);
        await DeleteFullReport(fullPublishedReport, ct);

        await _context.Set<Report>().AddAsync(updatedReport, ct);

        foreach (var otherDraft in otherDrafts)
        {
            otherDraft.PublishedReportId = publishedReportId;
        }

        await _context.SaveChangesAsync(ct);

        return updatedReport.Id;
    }

    public async Task<bool> ReportUrlSlugExists(string urlSlug, CancellationToken ct = default) =>
        await _context.Set<Report>().AnyAsync(o => o.UrlSlug == urlSlug.Trim() && !o.IsDraft, ct);

    public async Task<IDictionary<string, dynamic>> SetSectionPosition(
        Guid reportDraftId,
        Guid sectionId,
        int rowIndex,
        int? columnIndex = null,
        CancellationToken ct = default)
    {
        var draft = await _context.Set<Report>()
            .Include(x => x.Sections)
            .SingleOrDefaultAsync(x => x.Id == reportDraftId, ct);

        if (draft is null)
            throw new ObjectNotExistsException("The draft with specified ID does not exist.");

        if (!draft.IsDraft)
            throw new BusinessException("The specified report is already published.");

        if (draft.CreatedBy != _currentUserId)
            throw new ForbiddenException("It's forbidden to change a someone else's draft.");

        var movedSection = draft.Sections.SingleOrDefault(x => x.Id == sectionId);

        if (movedSection is null)
            throw new ObjectNotExistsException("The draft does not contain a section with the specified ID.");

        var result = new Dictionary<string, dynamic>();

        var fixedRowIndex = Math.Min(Math.Max(rowIndex, 1), draft.Sections.Where(x => x.Id != movedSection.Id).Max(x => x.RowIndex) + 1);
        var fixedColumnIndex = columnIndex.HasValue
            ? Math.Min(Math.Max(columnIndex.Value, 1), draft.Sections.Where(x => x.RowIndex == rowIndex).Max(y => y.ColumnIndex) + 1)
            : 1;

        if (movedSection.RowIndex == fixedRowIndex && movedSection.ColumnIndex < fixedColumnIndex)
        {
            fixedColumnIndex--;
        }

        if (movedSection.RowIndex < fixedRowIndex && draft.Sections.Count(x => x.RowIndex == movedSection.RowIndex) == 1)
        {
            fixedRowIndex--;
        }

        var structure = draft.Sections
            .Where(x => x.Id != sectionId)
            .OrderBy(y => y.RowIndex).ThenBy(y => y.ColumnIndex)
            .GroupBy(x => x.RowIndex)
            .Select(x => x.ToList())
            .ToList();

        if (!columnIndex.HasValue)
        {
            structure.Insert(fixedRowIndex - 1, new List<Section> { movedSection });
        }
        else
        {
            structure[fixedRowIndex - 1].Insert(fixedColumnIndex - 1, movedSection);
        }

        for (var ri = 1; ri <= structure.Count; ri++)
        {
            for (var ci = 1; ci <= structure[ri - 1].Count; ci++)
            {
                if (structure[ri - 1][ci - 1].RowIndex != ri || structure[ri - 1][ci - 1].ColumnIndex != ci)
                {
                    structure[ri - 1][ci - 1].RowIndex = ri;
                    structure[ri - 1][ci - 1].ColumnIndex = ci;
                    result[structure[ri - 1][ci - 1].Id.ToString()] = new { RowIndex = ri, ColumnIndex = ci };
                }
            }
        }

        await _context.SaveChangesAsync(ct);

        return result;
    }

    public async Task<ReportDTO> Update(ReportDTO dto, CancellationToken ct = default)
    {
        var entity = await _context.Set<Report>().FirstOrDefaultAsync(x => x.Id.Equals(dto.Id), ct);
        if (entity is null)
            throw new EntityNotFoundException();

        if (!entity.IsDraft)
            throw new BusinessException("It's forbidden to change a published report.");

        if (entity.CreatedBy != _currentUserId)
            throw new ForbiddenException("It's forbidden to change a someone else's draft.");

        // DTO input cleanup
        dto.UrlSlug = dto.UrlSlug.Trim();

        ValidateReportDto(dto);

        if (await _context.Set<Report>()
                .AnyAsync(x => x.UrlSlug == dto.UrlSlug && x.Id != dto.Id && x.Id != dto.PublishedReportId && !x.IsDraft, ct))
            throw new BusinessException($"The report with URL slug '{dto.UrlSlug}' already exists.");

        dto.PublishedReport = null;
        dto.PublishedReportId = entity.PublishedReportId;
        dto.IsDraft = true;
        dto.CreatedBy = entity.CreatedBy;
        dto.UpdatedBy = _currentUserId;
        dto.UpdatedOn = DateTime.UtcNow;


        await UpdateRoles(dto.Id, dto.Roles.Select(x => x.Id), ct);
        await UpdatePermissions(dto.Id, dto.Permissions.Select(x => x.Id), ct);
        await _context.SaveChangesAsync(ct);

        await _dataService.Update<Report, ReportDTO, Guid>(dto, ct);

        return _mapper.Map<ReportDTO>(await GetCommonReportData(x => x.Id == entity.Id, false, ct));
    }

    public async Task<ReportChangeResult> UpdateSection(Guid reportDraftId, Guid sectionId, SectionDTO dto, CancellationToken ct = default)
    {
        var report = await _context.Set<Report>().SingleOrDefaultAsync(x => x.Id == reportDraftId, ct);

        if (!await _context.Set<Report>().AnyAsync(x => x.Id == reportDraftId, ct))
            throw new ObjectNotExistsException("The report with the specified ID does not exist.");

        if (!report.IsDraft)
            throw new BusinessException("It's forbidden to change a published report.");

        ValidateSectionDto(dto);

        var section = _context.Set<Section>().SingleOrDefault(x => x.Id == dto.Id);

        if (section is null)
            throw new ObjectNotExistsException("The section with specified ID doen't exist.");

        if (dto.RowIndex < section.RowIndex)
        {
            await _context.Set<Section>()
                .Where(x => x.RowIndex >= dto.RowIndex && x.RowIndex < section.RowIndex)
                .ForEachAsync(x => x.RowIndex++, ct);
        }

        if (dto.RowIndex > section.RowIndex)
        {
            await _context.Set<Section>()
                .Where(x => x.RowIndex > section.RowIndex && x.RowIndex <= dto.RowIndex)
                .ForEachAsync(x => x.RowIndex--, ct);
        }

        report.UpdatedOn = DateTime.UtcNow;

        return new ReportChangeResult
        {
            ReportUpdatedOn = report.UpdatedOn,
            RequestTargetPart = await _dataService.Update<Section, SectionDTO, Guid>(dto, ct)
        };
    }


    private void ClearReportCopy(Report report)
    {
        var tablesMap = report.Sections
            .Where(x => x.Query != null)
            .SelectMany(x => x.Query.QueryTables)
            .ToDictionary(x => x.Id, x => x);

        var columnsMap = report.Sections
            .Where(x => x.Query != null)
            .SelectMany(x => x.Query.QueryTables)
            .SelectMany(x => x.Columns)
            .ToDictionary(x => x.Id, x => x);

        var filtersMap = report.Sections
            .Where(x => x.Query != null)
            .SelectMany(x => x.Query.QueryFilterSets)
            .SelectMany(x => x.QueryFilters)
            .ToDictionary(x => x.Id, x => x);

        foreach (var reportRole in report.ReportRoles)
        {
            reportRole.Role = default;
            reportRole.Report = report;
            reportRole.ReportId = default;
        }

        foreach (var reportPermission in report.ReportPermissions)
        {
            reportPermission.Permission = default;
            reportPermission.Report = report;
            reportPermission.ReportId = default;
        }

        foreach (var section in report.Sections)
        {
            if (section.Query is not null)
            {
                foreach (var queryTable in section.Query.QueryTables)
                {
                    foreach (var queryTableColumn in queryTable.Columns)
                    {
                        columnsMap[queryTableColumn.Id] = queryTableColumn;
                        queryTableColumn.Id = default;
                        queryTableColumn.QueryTableId = default;
                    }

                    queryTable.Id = default;
                    queryTable.QueryId = default;
                }

                foreach (var queryTableJoin in section.Query.QueryTableJoins)
                {
                    queryTableJoin.Id = default;
                    queryTableJoin.QueryId = default;
                    queryTableJoin.FromQueryTable = tablesMap[(int)queryTableJoin.FromQueryTableId];
                    queryTableJoin.FromQueryTableId = default;
                    queryTableJoin.ToQueryTable = tablesMap[(int)queryTableJoin.ToQueryTableId];
                    queryTableJoin.ToQueryTableId = default;
                    queryTableJoin.FromQueryTableColumn = columnsMap[(int)queryTableJoin.FromQueryTableColumnId];
                    queryTableJoin.FromQueryTableColumnId = default;
                    queryTableJoin.ToQueryTableColumn = columnsMap[(int)queryTableJoin.ToQueryTableColumnId];
                    queryTableJoin.ToQueryTableColumnId = default;
                }

                var rootFilterSet = section.Query.QueryFilterSets.Single(x => x.ParentQueryId == section.Query.Id);
                rootFilterSet.ParentQuery = section.Query;
                HandleQueryFilterTreeNode(
                    rootFilterSet,
                    section.Query.QueryFilterSets,
                    section.Query,
                    columnsMap,
                    filtersMap);

                section.Query.Id = default;
            }

            if (section.View is not null)
            {
                if (section.View.GridView is not null)
                {
                    foreach (var viewColumn in section.View.GridView.ViewColumns)
                    {
                        viewColumn.QueryTableColumn = columnsMap[viewColumn.QueryTableColumnId];
                        viewColumn.QueryTableColumnId = default;
                        viewColumn.Id = default;
                        viewColumn.GridViewId = default;
                    }

                    foreach (var queryFilterBinding in section.QueryFilterBindings)
                    {
                        queryFilterBinding.MasterDetailQueryTableColumn = columnsMap[(int)queryFilterBinding.MasterDetailQueryTableColumnId];
                        queryFilterBinding.MasterDetailQueryTableColumnId = default;
                        queryFilterBinding.MasterDetailSectionId = default;

                        queryFilterBinding.Id = default;
                        queryFilterBinding.QueryFilter = filtersMap[queryFilterBinding.QueryFilterId];
                        queryFilterBinding.QueryFilterId = default;
                    }

                    if (section.View.GridView.DefaultSortColumnId is not null)
                    {
                        section.View.GridView.DefaultSortColumn = columnsMap[(int)section.View.GridView.DefaultSortColumnId];
                        section.View.GridView.DefaultSortColumnId = default;
                    }

                    section.View.GridView.Id = default;
                    section.View.GridView.ViewId = default;
                }

                foreach (var filterControl in section.View.Filters)
                {
                    foreach (var queryFilterBinding in filterControl.QueryFilterBindings)
                    {
                        queryFilterBinding.Id = default;
                        queryFilterBinding.FilterControlId = default;
                        queryFilterBinding.QueryFilter = filtersMap[queryFilterBinding.QueryFilterId];
                        queryFilterBinding.QueryFilterId = default;
                    }

                    filterControl.MasterControlId = default;
                    filterControl.Id = default;
                    filterControl.ViewId = default;
                }

                section.View.Id = default;
                section.View.SectionId = default;
            }

            if (section.ReusedSectionId is not null)
            {
                var reusedSection = report.Sections.SingleOrDefault(x => x.Id == section.ReusedSectionId);

                if (reusedSection is null)
                    reusedSection = report.Sections.SingleOrDefault(x => x.PublishedSectionId == section.ReusedSectionId);

                if (reusedSection is null)
                {
                    section.ReusedSection = null;
                }
                else
                {
                    section.ReusedSection = reusedSection;
                    section.ReusedSectionId = default;
                }
            }

            section.PublishedSectionId = section.Id;
            section.NamedQuery = null;
            section.QueryId = default;
            section.Id = default;
            section.ReportId = default;
        }

        report.Id = default;


        void HandleQueryFilterTreeNode(
            QueryFilterSet queryFilterSet,
            IList<QueryFilterSet> allQueryFilterSets,
            Query query,
            IDictionary<int, QueryTableColumn> columnsMap,
            IDictionary<int, QueryFilter> filtersMap)
        {
            foreach (var child in allQueryFilterSets.Where(x => x.ParentId == queryFilterSet.Id))
            {
                queryFilterSet.ChildSets.Add(child);
                HandleQueryFilterTreeNode(child, allQueryFilterSets, query, columnsMap, filtersMap);
            }

            foreach (var queryFilter in queryFilterSet.QueryFilters)
            {
                filtersMap[queryFilter.Id] = queryFilter;
                queryFilter.Id = default;
                queryFilter.QueryFilterSetId = default;
                queryFilter.QueryRule = null;
                queryFilter.QueryTableColumn = queryFilter.QueryTableColumnId.HasValue
                    ? columnsMap[queryFilter.QueryTableColumnId.Value]
                    : null;
                queryFilter.QueryTableColumnId = default;
                queryFilter.QueryFilterBindings = new List<QueryFilterBinding>();
            }

            queryFilterSet.Id = default;
            queryFilterSet.ParentQueryId = default;
            queryFilterSet.QueryId = default;
            queryFilterSet.ParentId = default;
            queryFilterSet.Query = query;
        }
    }

    private async Task DeleteFullReport(Report report, CancellationToken ct)
    {
        foreach (var section in report.Sections)
        {
            MarkQueryAsDeleted(section.Query);
        }

        await _dataService.Delete<Report, Guid>(report.Id);
    }

    private async Task<Report> GetFullReport(
        Expression<Func<Report, bool>> singleOrDefaultExpression,
        bool trackable = false,
        CancellationToken ct = default)
    {
        var result = GetFullReportQueryable();

        return await (trackable ? result : result.AsNoTracking())
            .SingleOrDefaultAsync(singleOrDefaultExpression, ct);
    }

    private IQueryable<Report> GetFullReportQueryable() =>
        _context.Set<Report>()
            .Include(x => x.ReportRoles).ThenInclude(x => x.Role)
            .Include(x => x.ReportPermissions).ThenInclude(x => x.Permission)
            .Include(x => x.Sections).ThenInclude(x => x.QueryFilterBindings)
            .Include(x => x.Sections).ThenInclude(x => x.Query).ThenInclude(x => x.QueryTableJoins)
                .ThenInclude(x => x.FromQueryTable)
            .Include(x => x.Sections).ThenInclude(x => x.Query).ThenInclude(x => x.QueryTableJoins)
                .ThenInclude(x => x.FromQueryTableColumn)
            .Include(x => x.Sections).ThenInclude(x => x.Query).ThenInclude(x => x.QueryTableJoins)
                .ThenInclude(x => x.ToQueryTable)
            .Include(x => x.Sections).ThenInclude(x => x.Query).ThenInclude(x => x.QueryTableJoins)
                .ThenInclude(x => x.ToQueryTableColumn)
            .Include(x => x.Sections).ThenInclude(x => x.Query).ThenInclude(x => x.QueryTables)
                .ThenInclude(x => x.Columns)
            .Include(x => x.Sections).ThenInclude(x => x.Query).ThenInclude(x => x.QueryFilterSets)
                .ThenInclude(x => x.QueryFilters).ThenInclude(x => x.QueryRule)
            .Include(x => x.Sections).ThenInclude(x => x.Query).ThenInclude(x => x.QueryFilterSets)
                    .ThenInclude(x => x.QueryFilters).ThenInclude(x => x.QueryFilterBindings)
            .Include(x => x.Sections).ThenInclude(x => x.View).ThenInclude(x => x.GridView)
                .ThenInclude(x => x.DefaultSortColumn)
            .Include(x => x.Sections).ThenInclude(x => x.View).ThenInclude(x => x.GridView)
                .ThenInclude(x => x.ViewColumns).ThenInclude(x => x.QueryTableColumn)
            .Include(x => x.Sections).ThenInclude(x => x.View).ThenInclude(x => x.Filters)
                .ThenInclude(x => x.QueryFilterBindings)
            // Important to use AsSplitQuery here! Significantly reduces request time (5x)
            .AsSplitQuery();

    private async Task<Report> GetCommonReportData(
        Expression<Func<Report, bool>> singleOrDefaultExpression,
        bool trackable = false,
        CancellationToken ct = default)
    {
        var result = _context.Set<Report>()
            .Include(x => x.ReportRoles).ThenInclude(x => x.Role)
            .Include(x => x.ReportPermissions).ThenInclude(x => x.Permission)
            .Include(x => x.Sections)
            .Include(x => x.PublishedReport);

        return await (trackable ? result : result.AsNoTracking())
            .SingleOrDefaultAsync(singleOrDefaultExpression, ct);
    }

    private void MarkQueryAsDeleted(Query query)
    {
        if (query is null) return;

        foreach (var filterSet in query.QueryFilterSets)
        {
            _context.Set<QueryFilterSet>().Remove(filterSet);
        }
        _context.Set<Query>().Remove(query);
    }

    private async Task UpdatePermissions(Guid reportId, IEnumerable<int> newPermissionsIds, CancellationToken ct)
    {
        newPermissionsIds = newPermissionsIds.ToList();
        var existingPermissions = _context.Set<ReportPermission>().Where(x => x.ReportId == reportId);

        foreach (var existingPermission in existingPermissions)
        {
            if (newPermissionsIds.All(x => x != existingPermission.PermissionId))
                _context.Set<ReportPermission>().Remove(existingPermission);
        }

        foreach (var newPermissionId in newPermissionsIds)
        {
            if (await existingPermissions.AllAsync(x => x.PermissionId != newPermissionId, ct))
            {
                await _context.Set<ReportPermission>().AddAsync(
                    new ReportPermission
                    {
                        ReportId = reportId,
                        PermissionId = newPermissionId
                    },
                    ct);
            }
        }
    }

    private async Task UpdateRoles(Guid reportId, IEnumerable<string> newRolesIds, CancellationToken ct)
    {
        newRolesIds = newRolesIds.ToList();
        var existingRoles = _context.Set<ReportRole>().Where(x => x.ReportId == reportId);

        foreach (var existingRole in existingRoles)
        {
            if (newRolesIds.All(x => x != existingRole.RoleId))
                _context.Set<ReportRole>().Remove(existingRole);
        }

        foreach (var newRoleId in newRolesIds)
        {
            if (await existingRoles.AllAsync(x => x.RoleId != newRoleId, ct))
            {
                await _context.Set<ReportRole>().AddAsync(
                    new ReportRole
                    {
                        ReportId = reportId,
                        RoleId = newRoleId
                    },
                    ct);
            }
        }
    }

    private async Task<bool> UserHasReportDraft(string userId, CancellationToken ct) =>
        await UserHasReportDraft(userId, null, ct);

    private async Task<bool> UserHasReportDraft(string userId, Guid? publishedReportId, CancellationToken ct) =>
        await _context.Set<Report>().AnyAsync(x =>
            x.IsDraft && x.CreatedBy == userId && x.PublishedReportId == publishedReportId, ct);

    /// <summary>
    /// Report access validation for user.
    /// Empty Access value means we use specific roles/permissions.
    /// </summary>
    private async Task CurrentUserHasReportAccess(string urlSlug, CancellationToken ct)
    {
        var report = await Get<Report, ReportDTO>(query => query
            .Include(a => a.ReportRoles).ThenInclude(x => x.Role)
            .Include(a => a.ReportPermissions).ThenInclude(x => x.Permission)
            .Where(o => o.UrlSlug.Equals(urlSlug)), ct);

        if (string.IsNullOrEmpty(report.Access))
        {
            var user = await _userManager.Users
                .Include(x => x.UserRoles).ThenInclude(x => x.Role)
                .SingleOrDefaultAsync(o => o.Id == _currentUserId, ct);

            if (report.Roles.Select(o => o.Name).Intersect(user.UserRoles.Select(o => o.Role.Name)).Any())
            {
                return;
            }

            var allUserPermissions = await _userService.GetAllUserPermissions(_currentUserId, ct);
            if (report.Permissions.Select(o => o.Name).Intersect(allUserPermissions.Select(o => o.Name)).Any())
            {
                return;
            }

            throw new ForbiddenException();
        }
    }

    private void ValidateReportDto(ReportDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException(new ValidationResult($"The field '{nameof(dto.Name)}' of the section is empty.", new[] { nameof(dto.Name) }), null, null);

        if (string.IsNullOrWhiteSpace(dto.UrlSlug))
            throw new ValidationException(new ValidationResult($"The field '{nameof(dto.UrlSlug)}' of the section is empty.", new[] { nameof(dto.UrlSlug) }), null, null);
    }

    private void ValidateSectionDto(SectionDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ValidationException(new ValidationResult($"The '{nameof(dto.Title)}' of the section is empty.", new[] { nameof(dto.Title) }), null, null);
    }

    private async Task CheckAccessToReport(Report report, CancellationToken ct)
    {
        if (report.ReportRoles.Any() || report.ReportPermissions.Any())
        {
            var sectionRolesArray = report.ReportRoles.Select(x => x.ReportId.ToString());
            var sectionPermissionsArray = report.ReportPermissions.Select(x => x.PermissionId);
            if (_context.Set<UserRole>().All(x => x.UserId != _currentUserId || !sectionRolesArray.Contains(x.RoleId)) &&
                _context.Set<UserPermission>().All(x => x.UserId != _currentUserId || !sectionPermissionsArray.Contains(x.PermissionId)))
            {
                throw new ForbiddenException("You don't have access rights.");
            }
        }
    }
}
