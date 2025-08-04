using AutoMapper;
using BBWM.Core.Data;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.ModelHashing;
using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.Core.Web.Filters;
using BBWM.DbDoc.Interfaces;
using BBWM.Reporting.DTO;
using BBWM.Reporting.Interfaces;
using BBWM.Reporting.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BBWM.Reporting.Controllers;

[Route("api/reporting/report")]
[ReadWriteAuthorize(ReadWriteRoles = Core.Roles.SuperAdminRole + "," + Roles.ReportEditorRole)]
public class ReportController : DataControllerBase<Report, ReportDTO, ReportDTO, Guid>
{
    private readonly IReportService _reportService;
    private readonly IRoleService _roleService;
    private readonly IDbDocFolderService _dbDocFolderService;
    private readonly IColumnTypeService _columnTypeService;
    private readonly IDbContext _dbContext;
    private readonly IMapper _mapper;


    public ReportController(
        IDataService dataService,
        IReportService reportService,
        IRoleService roleService,
        IDbDocFolderService dbDocFolderService,
        IColumnTypeService columnTypeService,
        IDbContext dbContext,
        IMapper mapper)
        : base(dataService, reportService)
    {
        _reportService = reportService;
        _roleService = roleService;
        _dbDocFolderService = dbDocFolderService;
        _columnTypeService = columnTypeService;
        _dbContext = dbContext;
        _mapper = mapper;
    }


    [HttpPost("{reportDraftId}/add-section-to-row/{sectionId}/{rowIndex}/{columnIndex}")]
    public async Task<IActionResult> SetSectionRow(Guid reportDraftId, Guid sectionId, int rowIndex, int columnIndex, CancellationToken ct)
        => Ok(await _reportService.SetSectionPosition(reportDraftId, sectionId, rowIndex, columnIndex, ct));

    public override async Task<IActionResult> Create(
        [FromBody] ReportDTO dto,
        [FromServices] IModelHashingService modelHashingService,
        CancellationToken ct = default) => await Task.FromResult(NotFound());

    [HttpDelete("{reportDraftId}/cancel")]
    public Task<IActionResult> CancelDraft(Guid reportDraftId, CancellationToken ct)
        => NoContent(() => _reportService.CancelDraft(reportDraftId, ct));

    [HttpPost("{reportDraftId}/create-section")]
    public async Task<IActionResult> CreateSection(Guid reportDraftId, [FromBody] SectionDTO dto, CancellationToken ct)
        => Ok(await _reportService.CreateSection(reportDraftId, dto, ct));

    [HttpPost("create-draft")]
    public async Task<IActionResult> CreateDraftOfNewReport([FromBody] ReportDTO dto, CancellationToken ct)
        => Ok(await _reportService.CreateDraftOfNewReport(dto, ct));

    [HttpPost("current-user-report-draft/{publishedReportId}")]
    public async Task<IActionResult> CurrentUserExistingReportDraft(Guid publishedReportId, CancellationToken ct)
        => Ok(await _reportService.GetCurrentUserDraftReport(publishedReportId, ct)
            ?? await _reportService.CreateDraftOfExistingReport(publishedReportId, ct));

    [HttpPost("{reportDraftId}/replace-draft-with-recent")]
    public async Task<IActionResult> ReplaceDraftWithRecent(Guid reportDraftId, CancellationToken ct)
        => Ok(await _reportService.ReplaceDraftWithRecent(reportDraftId, ct));

    [HttpDelete("{reportDraftId}/delete-section/{sectionId}")]
    public async Task<IActionResult> DeleteSection(Guid reportDraftId, Guid sectionId, CancellationToken ct)
        => Ok(await _reportService.DeleteSection(reportDraftId, sectionId, ct));

    [HttpGet("get-current-user-report-draft")]
    public async Task<IActionResult> GetCurrentUserNewReportDraft(CancellationToken ct)
        => Ok(await _reportService.GetCurrentUserDraftReport(ct));

    [HttpGet("folders"), ResponseCache(NoStore = true)]
    public async Task<IActionResult> GetFolders(CancellationToken ct)
        => Ok(await _reportService.GetFolders(ct));

    [HttpGet("folder-tables/{folderId}"), ResponseCache(NoStore = true)]
    public async Task<IActionResult> GetFolderTableMatadata(Guid folderId, CancellationToken ct)
        => Ok(await _reportService.GetFolderTableMatadata(folderId, ct));

    [HttpGet("full-table-metadata/{tableMetadataId}")]
    public async Task<IActionResult> GetFullTableMetadata(int tableMetadataId, CancellationToken ct)
        => Ok(await _reportService.GetFullTableMatadata(tableMetadataId, ct));

    [HttpGet("get-query-rules")]
    public async Task<IActionResult> GetRuleTypes(CancellationToken ct)
        => Ok(_mapper.Map<IEnumerable<QueryRuleDTO>>(await _dbContext.Set<QueryRule>().Include(x => x.RuleTypes).ToListAsync(ct)));

    [HttpGet("{reportId}/get-last-updated-draft-info")]
    public async Task<IActionResult> GetLastUpdatedDraftInfo(Guid reportId, CancellationToken ct)
        => Ok(await _reportService.GetReportLastUpdatedDraftInfo(reportId, ct));

    [HttpPost("publish-draft/{reportDraftId}")]
    public Task<IActionResult> PublishReport(Guid reportDraftId, CancellationToken ct)
        => NoContent(() => _reportService.PublishReportDraft(reportDraftId, ct));

    [HttpGet("url-slug-exists/{urlSlug}")]
    public async Task<IActionResult> ReportUrlSlugExists(string urlSlug, CancellationToken ct)
        => Ok(await _reportService.ReportUrlSlugExists(urlSlug, ct));

    [HttpGet("role-options")]
    public IActionResult GetRoleOptions()
        => Ok(_roleService.GetHardcodedRoles().Concat(_roleService.GetProjectRoles())
            .Select(x => new DropDownOption { Label = x.Name, Value = x.Id }));

    [HttpGet("column-types")]
    public async Task<IActionResult> GetColumnTypes(CancellationToken cancellationToken) =>
        Ok(await _columnTypeService.GetAll(cancellationToken));

    [HttpPost("{reportDraftId}/set-section-row/{sectionId}/{rowIndex}")]
    public async Task<IActionResult> SetSectionRow(Guid reportDraftId, Guid sectionId, int rowIndex, CancellationToken ct)
        => Ok(await _reportService.SetSectionPosition(reportDraftId, sectionId, rowIndex, null, ct));

    [HttpPut("{reportDraftId}/update-section/{sectionId}")]
    public async Task<IActionResult> UpdateSection(
        Guid reportDraftId,
        Guid sectionId,
        [FromBody] SectionDTO dto,
        CancellationToken ct)
        => Ok(await _reportService.UpdateSection(reportDraftId, sectionId, dto, ct));

    // TODO: remove it when Reporting v2 removed
    [HttpGet("use-default-db-folder")]
    public async Task<IActionResult> UseDefaultDbFolder(CancellationToken ct)
    {
        var defaultDbDocFolder = await _dbDocFolderService.GetDefaultFolder(ct);

        if (!defaultDbDocFolder.Owners.Contains(ModuleLinkage.DbDocFolderOwnerName))
        {
            defaultDbDocFolder.Owners.Add(ModuleLinkage.DbDocFolderOwnerName);
            await _dbDocFolderService.UpdateFolder(defaultDbDocFolder.Id, defaultDbDocFolder, ct);
        }

        return NoContent();
    }

    [HttpGet("view/{urlSlug}")]
    [Authorize]
    public async Task<IActionResult> GetReportView(string urlSlug, CancellationToken ct)
        => Ok(await _reportService.GetReportView(urlSlug, ct));
}
