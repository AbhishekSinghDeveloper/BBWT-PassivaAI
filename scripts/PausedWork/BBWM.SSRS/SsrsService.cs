using System;
using BBWM.JWT;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using BBWM.Core.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace BBWM.SSRS
{
    public class SsrsService : ISsrsService
    {
        private readonly IJwtService _jwtService;
        private readonly IDbContext _context;
        private readonly SsrsSettings _ssrsConfig;

        public SsrsService(
            ISsrsDataContext context,
            IJwtService jwtService,
            IOptionsSnapshot<SsrsSettings> ssrsConfig)
        {
            _context = context;
            _jwtService = jwtService;
            _ssrsConfig = ssrsConfig.Value;
        }

        /// This is actually a sample code (moved from the demo module), we suppose that the new reporting module (BBWM.Reports)
        /// should handle SSRS in the future.
        /// After the client gets the token from GetReportToken(), it generates the report URL:
        /// reportUrl = than.sanitizer.bypassSecurityTrustResourceUrl(result.reportUrl + "?token=" + result.token);
        public async Task<ReportAccessDataDTO> GetReportToken(string currentUserId, Guid reportId, CancellationToken cancellationToken = default)
        {
            var report = await _context.Set<Catalog>().SingleAsync(x => x.Id == reportId, cancellationToken);
            var data = _jwtService.GenerateReportToken(currentUserId, report.Path);

            var result = new ReportAccessDataDTO()
            {
                ReportUrl = $"{_ssrsConfig.Url}{report.Path}",
                Token = data.Token
            };
            return result;
        }
    }
}