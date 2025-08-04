using AutoMapper;
using BBWM.Core;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Filters;
using BBWM.Core.Membership.Model;
using BBWM.Core.Web.Extensions;
using BBWM.DbDoc.Model;
using BBWM.Reporting.DTO;
using BBWM.Reporting.Enums;
using BBWM.Reporting.Interfaces;
using BBWM.Reporting.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BBWM.Reporting.Services;

public class SectionViewService : ISectionViewService
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly string _currentUserId;
    private readonly IViewBuilderService _viewBuilderService;
    private readonly IQueryDataService _queryDataService;


    public SectionViewService(
        IDbContext context,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        IViewBuilderService viewBuilderService,
        IQueryDataService queryDataService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserId = httpContextAccessor.HttpContext.GetUserId();
        _viewBuilderService = viewBuilderService;
        _queryDataService = queryDataService;
    }

    public async Task<SectionDisplayViewDTO> GetDisplayView(Guid sectionId, CancellationToken ct = default)
    {
        var section = await _context.Set<Section>()
            .Include(x => x.Query)
            .Include(x => x.QueryFilterBindings)
            .Include(x => x.View)
                .ThenInclude(x => x.GridView)
                .ThenInclude(x => x.DefaultSortColumn)
            .Include(x => x.View)
                .ThenInclude(x => x.GridView)
                .ThenInclude(x => x.ViewColumns)
                .ThenInclude(x => x.QueryTableColumn)
                //TODO: hack for demo
                .ThenInclude(x => x.QueryTable)
            .Include(x => x.View)
                .ThenInclude(x => x.GridView)
                .ThenInclude(x => x.ViewColumns)
                .ThenInclude(x => x.CustomColumnType)
                .ThenInclude(x => x.ViewMetadata)
                .ThenInclude(x => x.GridColumnView)
            .Include(x => x.View)
                .ThenInclude(x => x.Filters)
                .ThenInclude(x => x.QueryFilterBindings)
                .ThenInclude(x => x.QueryFilter)
                .ThenInclude(x => x.QueryTableColumn)
            .Include(x => x.View)
                .ThenInclude(x => x.Filters)
                .ThenInclude(x => x.QueryFilterBindings)
                .ThenInclude(x => x.QueryFilter)
                .ThenInclude(x => x.QueryRule)
             .Include(x => x.Query)
                .ThenInclude(x => x.QueryFilterSets)
                .ThenInclude(x => x.QueryFilters)
                .ThenInclude(x => x.QueryFilterBindings)
                .ThenInclude(x => x.MasterDetailSection)
                .ThenInclude(x => x.View)
             .Include(x => x.Query)
                .ThenInclude(x => x.QueryFilterSets)
                .ThenInclude(x => x.QueryFilters)
                .ThenInclude(x => x.QueryFilterBindings)
                .ThenInclude(x => x.MasterDetailQueryTableColumn)
            .AsSplitQuery()
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == sectionId, ct);

        return section is null
            ? throw new ObjectNotExistsException("The section with specified ID does not exist.")
            : await _viewBuilderService.GetSectionView(section, ct);
    }

    public async Task<IEnumerable<DropDownOption>> GetFilterOptions(Guid sectionId, int filterControlId, CancellationToken ct = default)
    {
        var section = await _context.Set<Section>()
            .Include(x => x.Report).ThenInclude(x => x.ReportRoles)
            .Include(x => x.Report).ThenInclude(x => x.ReportPermissions)
            .Include(x => x.Query).ThenInclude(x => x.QueryTables)
            .Include(x => x.View).ThenInclude(x => x.Filters)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == sectionId, ct)
            ?? throw new ObjectNotExistsException("The section with specified ID does not exist.");

        if (section.Query is null || section.View is null || section.Query.DbDocFolderId is null)
            return new List<DropDownOption>();

        await CheckAccessToSection(section, ct);

        var filterControlModel = section.View.Filters.SingleOrDefault(x => x.Id == filterControlId)
            ?? throw new ObjectNotExistsException("The section does not contain a filter control with specified ID.");

        var filterControlDto = _mapper.Map<FilterControlDTO>(filterControlModel);

        if (filterControlDto.InputType != InputType.Dropdown && filterControlDto.InputType != InputType.Multiselect)
            throw new BusinessException("The input type of the filter control is not 'Dropdown'.");

        var optionsRequest = new QueryBuilderOptionsRequest
        {
            SourceTableId = filterControlDto.ExtraSettings["sourceDbDocTableId"].GetValue<string>(),
            LabelColumnId = filterControlDto.ExtraSettings["labelDbDocColumnId"].GetValue<string>(),
            ValueColumnId = filterControlDto.ExtraSettings["valueDbDocColumnId"].GetValue<string>()
        };

        if (string.IsNullOrEmpty(optionsRequest.SourceTableId) ||
            string.IsNullOrEmpty(optionsRequest.LabelColumnId) ||
            string.IsNullOrEmpty(optionsRequest.ValueColumnId))
        {
            return new List<DropDownOption>();
        }

        if (!await _context.Set<TableMetadata>().AnyAsync(x =>
            x.TableId == optionsRequest.SourceTableId && x.FolderId == section.Query.DbDocFolderId, ct))
        {
            throw new BusinessException("Unable to load options from a table that does not exist in the same folder as the query.");
        }

        return await _queryDataService.GetDataAsOptions(optionsRequest, section.Query, ct);
    }

    public async Task<IEnumerable<dynamic>> GetData(Guid sectionId, QueryCommand queryCommand, CancellationToken ct = default)
    {
        var section = await GetSectionForDataLoadingQueryable()
                          .Include(x => x.Report).ThenInclude(x => x.ReportRoles)
                          .Include(x => x.Report).ThenInclude(x => x.ReportPermissions)
                          .AsNoTracking().SingleOrDefaultAsync(x => x.Id == sectionId, ct)
          ?? throw new ObjectNotExistsException("The section with specified ID does not exist.");

        if (section.Query is null) return null;

        await CheckAccessToSection(section, ct);

        section.Query.RootFilterSet = section.Query.QueryFilterSets.Any()
            ? QueryBuilderService.MakeFilterSetsTree(section.Query.QueryFilterSets, false)
            : null;

        return await _queryDataService.GetData(section.Query, queryCommand, ct);
    }

    public async Task<int> GetTotal(Guid sectionId, QueryCommand queryCommand = null, CancellationToken ct = default)
    {
        var fullSection = await GetSectionForDataLoadingQueryable()
          .Include(x => x.Report).ThenInclude(x => x.ReportRoles)
          .Include(x => x.Report).ThenInclude(x => x.ReportPermissions)
          .AsNoTracking()
          .SingleOrDefaultAsync(x => x.Id == sectionId, ct)
          ?? throw new ObjectNotExistsException("The section with specified ID does not exist.");

        if (fullSection.Query is null) return 0;

        fullSection.Query.RootFilterSet = fullSection.Query.QueryFilterSets.Any()
            ? QueryBuilderService.MakeFilterSetsTree(fullSection.Query.QueryFilterSets, false)
            : null;

        return await _queryDataService.GetTotal(fullSection.Query, queryCommand, ct);
    }

    public async Task<dynamic> GetAggregations(Guid sectionId, QueryCommand queryCommand = null, CancellationToken ct = default)
    {
        var fullSection = await GetSectionForAggregationsLoadingQueryable()
          .Include(x => x.Report).ThenInclude(x => x.ReportRoles)
          .Include(x => x.Report).ThenInclude(x => x.ReportPermissions)
          .AsNoTracking()
          .SingleOrDefaultAsync(x => x.Id == sectionId, ct)
          ?? throw new ObjectNotExistsException("The section with specified ID does not exist.");

        if (fullSection.Query is null || fullSection.View?.GridView?.ViewColumns is null) return null;

        fullSection.Query.RootFilterSet = fullSection.Query.QueryFilterSets.Any()
            ? QueryBuilderService.MakeFilterSetsTree(fullSection.Query.QueryFilterSets, false)
            : null;

        var queryTableColumns = fullSection.Query.QueryTables.SelectMany(x => x.Columns);
        var aggregations = new List<dynamic>();
        foreach (var viewColumn in fullSection.View.GridView.ViewColumns)
        {
            var nodeFooter = JsonNode.Parse(viewColumn.Footer ?? "{\"expressions\":[]}", default, default);
            var expressions = nodeFooter["expressions"].Deserialize<string[]>();

            if (expressions is not null && expressions.Any())
            {
                aggregations.Add(new
                {
                    ColumnId = queryTableColumns.Single(x => x.Id == viewColumn.QueryTableColumnId).SourceColumnId,
                    Expressions = expressions
                });
            }
        }

        return await _queryDataService.GetAggregations(fullSection.Query, aggregations, queryCommand, ct);
    }

    private async Task CheckAccessToSection(Section section, CancellationToken ct)
    {
        if (await _context.Set<UserRole>().Include(x => x.Role)
            .AnyAsync(x => (x.Role.Name == Core.Roles.SuperAdminRole || x.Role.Name == Roles.ReportEditorRole) && x.UserId == _currentUserId, ct)) return;

        if (section.Report.Access == AggregatedRoles.Authenticated) return;

        if (section.Report.ReportRoles.Any() || section.Report.ReportPermissions.Any())
        {
            var sectionRolesArray = section.Report.ReportRoles.Select(x => x.RoleId);
            var sectionPermissionsArray = section.Report.ReportPermissions.Select(x => x.PermissionId);
            if (_context.Set<UserRole>().All(x => x.UserId != _currentUserId || !sectionRolesArray.Contains(x.RoleId)) &&
                _context.Set<UserPermission>().All(x => x.UserId != _currentUserId || !sectionPermissionsArray.Contains(x.PermissionId)))
            {
                throw new ForbiddenException("You don't have access rights.");
            }
        }

        if (section.Query.DbDocFolderId is not null)
        {
            var containingFolder = await _context.Set<Folder>()
                .SingleOrDefaultAsync(x => x.Id == section.Query.DbDocFolderId, ct);

            if (containingFolder.Owners != null && !containingFolder.Owners.Contains(ModuleLinkage.DbDocFolderOwnerName))
                throw new BusinessException("The requesting table is in a folder that doesn't contain the 'Report' owner.");
        }
    }

    private IQueryable<Section> GetSectionForDataLoadingQueryable() =>
       _context.Set<Section>()
        .Include(x => x.Query).ThenInclude(x => x.QueryTables)
            .ThenInclude(x => x.Columns)
        .Include(x => x.Query).ThenInclude(x => x.QueryFilterSets).ThenInclude(x => x.QueryFilters)
            .ThenInclude(x => x.QueryRule)
        .Include(x => x.Query).ThenInclude(x => x.QueryFilterSets).ThenInclude(x => x.QueryFilters)
            .ThenInclude(x => x.QueryFilterBindings).ThenInclude(x => x.FilterControl)
        .Include(x => x.Query).ThenInclude(x => x.QueryTableJoins).ThenInclude(x => x.FromQueryTable).ThenInclude(x => x.Columns)
        .Include(x => x.Query).ThenInclude(x => x.QueryTableJoins).ThenInclude(x => x.ToQueryTable).ThenInclude(x => x.Columns)
        .Include(x => x.Query).ThenInclude(x => x.QueryTableJoins).ThenInclude(x => x.FromQueryTableColumn).ThenInclude(x => x.QueryTable)
        .Include(x => x.Query).ThenInclude(x => x.QueryTableJoins).ThenInclude(x => x.ToQueryTableColumn).ThenInclude(x => x.QueryTable);

    private IQueryable<Section> GetSectionForAggregationsLoadingQueryable() =>
       _context.Set<Section>()
        .Include(x => x.Query).ThenInclude(x => x.QueryTables).ThenInclude(x => x.Columns)
        .Include(x => x.Query).ThenInclude(x => x.QueryFilterSets)
            .ThenInclude(x => x.QueryFilters).ThenInclude(x => x.QueryRule)
        .Include(x => x.Query).ThenInclude(x => x.QueryFilterSets).ThenInclude(x => x.QueryFilters)
            .ThenInclude(x => x.QueryFilterBindings).ThenInclude(x => x.FilterControl)
        .Include(x => x.View).ThenInclude(x => x.GridView).ThenInclude(x => x.ViewColumns)
        .Include(x => x.Query).ThenInclude(x => x.QueryTableJoins).ThenInclude(x => x.FromQueryTable).ThenInclude(x => x.Columns)
        .Include(x => x.Query).ThenInclude(x => x.QueryTableJoins).ThenInclude(x => x.ToQueryTable).ThenInclude(x => x.Columns)
        .Include(x => x.Query).ThenInclude(x => x.QueryTableJoins).ThenInclude(x => x.FromQueryTableColumn).ThenInclude(x => x.QueryTable)
        .Include(x => x.Query).ThenInclude(x => x.QueryTableJoins).ThenInclude(x => x.ToQueryTableColumn).ThenInclude(x => x.QueryTable);
}