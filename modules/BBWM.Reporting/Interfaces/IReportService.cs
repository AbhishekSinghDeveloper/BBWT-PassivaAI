using BBWM.Core.Services;
using BBWM.DbDoc.DTO;
using BBWM.Reporting.DTO;
using BBWM.Reporting.Model;

namespace BBWM.Reporting.Interfaces;

public interface IReportService :
    IEntityQuery<Report>,
    IEntityUpdate<ReportDTO>,
    IEntityDelete<Guid>,
    IEntityPage<ReportDTO>
{
    Task CancelDraft(Guid reportDraftId, CancellationToken ct = default);

    Task<ReportDTO> CreateDraftOfExistingReport(Guid reportId, CancellationToken ct = default);

    Task<ReportDTO> CreateDraftOfNewReport(ReportDTO dto, CancellationToken ct = default);

    Task<ReportChangeResult> CreateSection(Guid reportDraftId, SectionDTO dto, CancellationToken ct = default);

    Task<ReportChangeResult> DeleteSection(Guid reportDraftId, Guid sectionId, CancellationToken ct = default);

    Task<bool> Exists(Guid reportId, CancellationToken ct = default);

    Task<ReportDTO> GetCurrentUserDraftReport(CancellationToken ct = default);

    Task<ReportDTO> GetCurrentUserDraftReport(Guid publishedReportId, CancellationToken ct = default);

    Task<IEnumerable<FolderDTO>> GetFolders(CancellationToken ct);

    Task<IEnumerable<TableMetadataDTO>> GetFolderTableMatadata(Guid folderId, CancellationToken ct = default);

    Task<TableMetadataDTO> GetFullTableMatadata(int tableMetadataId, CancellationToken ct = default);

    Task<ReportLastUpdatedDraftInfo> GetReportLastUpdatedDraftInfo(Guid reportId, CancellationToken ct = default);

    /// <summary>
    /// Publishes report draft. The published record is copied from the draft.
    /// </summary>
    /// <param name="reportDraftId">Draft report ID</param>
    /// <returns>Published report ID</returns>
    Task<Guid> PublishReportDraft(Guid reportDraftId, CancellationToken ct = default);

    Task<ReportDTO> ReplaceDraftWithRecent(Guid reportDraftId, CancellationToken ct = default);

    Task<bool> ReportUrlSlugExists(string urlSlug, CancellationToken ct = default);

    Task<IDictionary<string, dynamic>> SetSectionPosition(
        Guid reportDraftId,
        Guid sectionId,
        int rowIndex,
        int? columnIndex = null,
        CancellationToken ct = default);

    Task<ReportChangeResult> UpdateSection(Guid reportDraftId, Guid sectionId, SectionDTO dto, CancellationToken ct = default);

    Task<ReportViewDTO> GetReportView(string urlSlug, CancellationToken ct);
}