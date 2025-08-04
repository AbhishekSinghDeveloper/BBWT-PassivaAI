using BBWM.Core.Exceptions;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Services;

using Microsoft.EntityFrameworkCore;

using Organization = BBWM.Core.Membership.Model.Organization;

namespace BBWM.Core.Membership.Services;

public class OrganizationService : IOrganizationService
{
    private readonly IDataService _dataService;
    public OrganizationService(IDataService dataService)
    {
        _dataService = dataService;
    }

    public IQueryable<Organization> GetEntityQuery(IQueryable<Organization> baseQuery)
        => baseQuery.Include(x => x.Address)
            .Include(x => x.Branding)
            .Include(x => x.Branding.LogoImage)
            .Include(x => x.Branding.LogoIcon);

    public Task<OrganizationDTO> Get(string name, int skipId, CancellationToken ct)
        => _dataService.Get<Organization, OrganizationDTO>(query =>
        {
            query = query
                .Include(x => x.Address)
                .AsQueryable();

            query = skipId > 0 ? query.Where(x => x.Id != skipId) : query;
            query = !string.IsNullOrEmpty(name) ? query.Where(x => x.Name == name) : query;

            return query;
        }, ct);

    public Task Delete(int id, CancellationToken ct = default)
        => _dataService.Delete<Organization>(id, (entity, ctx) =>
        {
            if (ctx.Set<User>().Where(x => x.OrganizationId == id).Any())
            {
                throw new BusinessException("The attempted deletion has been rejected because there are users " +
                    "that depend on the organization you are trying to delete.");
            }
        }, ct);
}
