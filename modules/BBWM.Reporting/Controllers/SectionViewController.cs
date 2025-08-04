using BBWM.Core.Filters;
using BBWM.Core.Web;
using BBWM.Core.Web.ModelBinders;
using BBWM.Reporting.DTO;
using BBWM.Reporting.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.Reporting.Controllers
{
    // TODO: rename path to api/reporting/section-view
    [Route("api/reporting/section")]
    [Authorize]
    public class SectionViewController : Core.Web.ControllerBase
    {
        private readonly ISectionViewService sectionViewService;


        public SectionViewController(ISectionViewService sectionViewService) =>
            this.sectionViewService = sectionViewService;

        [HttpGet("{sectionId}/display-view")]
        public async Task<IActionResult> GetDisplayView(Guid sectionId, CancellationToken ct)
            => Ok(await sectionViewService.GetDisplayView(sectionId, ct));

        [HttpGet("{sectionId}/filter-options/{filterControlId}")]
        public async Task<IActionResult> GetFilterOptions(
            Guid sectionId,
            [HashedKeyBinder(typeof(FilterControlDTO))] int filterControlId,
            CancellationToken ct)
            => Ok(await sectionViewService.GetFilterOptions(sectionId, filterControlId, ct));

        [HttpGet("{sectionId}/fetch-data")]
        public async Task<IActionResult> GetData(Guid sectionId, [FromQuery] QueryCommand queryCommand, CancellationToken ct)
            => Ok(await sectionViewService.GetData(sectionId, queryCommand, ct));

        [HttpGet("{sectionId}/fetch-total")]
        public async Task<IActionResult> GetTotal(Guid sectionId, [FromQuery] QueryCommand queryCommand, CancellationToken ct)
            => Ok(await sectionViewService.GetTotal(sectionId, queryCommand, ct));

        [HttpGet("{sectionId}/fetch-aggregations")]
        public async Task<IActionResult> GetAggregations(Guid sectionId, [FromQuery] QueryCommand queryCommand, CancellationToken ct)
            => Ok(await sectionViewService.GetAggregations(sectionId, queryCommand, ct));
    }
}