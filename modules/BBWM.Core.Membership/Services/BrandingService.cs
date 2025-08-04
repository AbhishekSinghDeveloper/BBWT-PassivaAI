using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Services;
using BBWM.FileStorage;

using Microsoft.EntityFrameworkCore;

namespace BBWM.Core.Membership.Services;

public class BrandingService : IBrandingService
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IDbContext _context;
    private readonly IDataService _dataService;

    public BrandingService(
        IDbContext context,
        IFileStorageService fileStorageService,
        IDataService dataService)
    {
        _context = context;
        _dataService = dataService;
        _fileStorageService = fileStorageService;
    }

    public async Task DeleteLogoIcon(int brandingId, CancellationToken cancellationToken = default)
    {
        var branding = await _context.Set<Branding>().FirstOrDefaultAsync(x => x.Id == brandingId, cancellationToken);

        if (branding is null)
            throw new ObjectNotExistsException("Branding not found.");

        var logoIconId = branding.LogoIconId;
        if (logoIconId is null) return;

        branding.LogoIconId = null;
        branding.LogoIcon = null;
        await _context.SaveChangesAsync(cancellationToken);

        await _fileStorageService.DeleteFile((int)logoIconId, cancellationToken);
    }

    public async Task DeleteLogoImage(int brandingId, CancellationToken cancellationToken = default)
    {
        var branding = await _context.Set<Branding>().FirstOrDefaultAsync(x => x.Id == brandingId, cancellationToken);

        if (branding is null)
            throw new ObjectNotExistsException("Branding not found.");

        var logoImageId = branding.LogoImageId;
        if (logoImageId is null) return;

        branding.LogoImageId = null;
        branding.LogoImage = null;
        await _context.SaveChangesAsync(cancellationToken);

        await _fileStorageService.DeleteFile((int)logoImageId, cancellationToken);
    }

    public Task<BrandingDTO> GetOrganizationBranding(int organizationId, CancellationToken cancellationToken = default)
        => _dataService.Get<Branding, BrandingDTO>(query =>
                GetEntityQuery(query).Where(o => o.Organization.Id == organizationId), cancellationToken);

    public IQueryable<Branding> GetEntityQuery(IQueryable<Branding> baseQuery)
        => baseQuery.Include(x => x.Organization)
            .Include(x => x.LogoIcon)
            .Include(x => x.LogoImage);
}
