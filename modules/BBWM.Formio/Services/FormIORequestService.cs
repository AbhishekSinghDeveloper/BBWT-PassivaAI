using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using BBWM.Core.Exceptions;
using BBWM.Core.Membership.Model;
using BBWM.Core.Services;
using BBWM.Core.Web.Extensions;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Interfaces;
using BBWM.FormIO.Models;
using BBWM.Core.Filters;

namespace BBWM.FormIO.Services
{
    public class FormIORequestService : IFormIORequestService
    {
        private readonly string _currentUserId;
        private readonly IDataService _dataService;

        public FormIORequestService(
            IDataService dataService,
            IHttpContextAccessor httpContextAccessor)
        {
            _dataService = dataService;
            _currentUserId = httpContextAccessor.HttpContext.GetUserId();
        }

        public async Task<bool> CreateNewRequest(FormRequestDTO dto, CancellationToken ct = default)
        {
            try
            {
                var currentUser = await _dataService.Context.Set<User>().FirstAsync(x => x.Id == _currentUserId, cancellationToken: ct);

                if (dto.GroupsIds.Any())
                {
                    foreach (var item in dto.GroupsIds)
                    {
                        var groupId = int.Parse(item.Split("-", StringSplitOptions.RemoveEmptyEntries)[0]);
                        var orgId = int.Parse(item.Split("-", StringSplitOptions.RemoveEmptyEntries)[1]);
                        // Get all the users that belong to that combination of group/org
                        var userIds = await _dataService.Context.Set<UserOrganizationGroup>()
                            .Where(x => x.GroupId == groupId && x.OrganizationId == orgId)
                            .Select(x => x.UserId).ToListAsync();
                        // join the resultant user list into the dto.UserIds list and remove duplicates
                        dto.UserIds = dto.UserIds.Concat(userIds).Distinct().ToList();
                    }
                }

                // Fetch the real users out of the userIds list
                var users = await _dataService.Context.Set<User>().Where(x => dto.UserIds.Any(id => id == x.Id)).ToListAsync();

                if (users.Any())
                {
                    var definitionId = await _dataService.Context.Set<FormRevision>()
                        .Where(revision => revision.Id == dto.Id)
                        .Select(revision => revision.FormDefinitionId)
                        .FirstOrDefaultAsync(ct);

                    // For each user create a form Request with its corresponding FormData
                    foreach (var user in users)
                    {
                        var formData = new FormData
                        {
                            FormDefinitionId = definitionId,
                            CreatedOn = DateTime.UtcNow,
                            Json = "{}",
                            OrganizationId = currentUser.OrganizationId,
                            UserId = user.Id,
                        };
                        await _dataService.Context.Set<FormData>().AddAsync(formData);
                        await _dataService.Context.SaveChangesAsync();
                        var request = new FormRequest
                        {
                            FormRevisionId = dto.FormRevisionId,
                            RequestDate = dto.RequestDate,
                            FormDataId = formData.Id,
                            RequesterId = currentUser.Id
                        };
                        await _dataService.Context.Set<FormRequest>().AddAsync(request);
                        await _dataService.Context.SaveChangesAsync();
                    }

                    return true;
                }

                throw new BusinessException("The current user/group selection yielded no targets.");
            }
            catch (Exception ex)
            {
                throw new BusinessException("Form request creation failed.", ex);
            }
        }

        public IQueryable<FormRequest> GetEntityQuery(IQueryable<FormRequest> baseQuery)
        {
            return baseQuery
                .Include(x => x.FormData)
                .Include(x => x.FormRevision)
                .ThenInclude(x => x.FormDefinition)
                .Include(x => x.Requester);
        }

        public async Task<FormRequestTargetsDTO> GetTargets(CancellationToken cancellationToken)
        {
            // Get all the orgs the current user belongs to
            var userOrgs = await _dataService.Context.Set<User>().Where(x => x.Id == _currentUserId)
                .Include(x => x.UserOrganizations)
                .ThenInclude(x => x.Organization)
                .SelectMany(x => x.UserOrganizations).ToListAsync();

            var orgs = userOrgs.Select(x => x.OrganizationId).ToList();

            var groups = await _dataService.Context.Set<UserOrganizationGroup>()
                .Include(x => x.Group)
                .Where(x => orgs.Any(id => x.OrganizationId == id))
                .ToListAsync();
            var users = await _dataService.Context.Set<User>().Include(x => x.UserOrganizations)
                .Where(x => x.UserOrganizations.Any(uo => orgs.Any(id => id == uo.OrganizationId))).ToListAsync();

            return new FormRequestTargetsDTO
            {
                Groups = groups.Select(x => new FormRequestTargetGroupDTO
                {
                    Id = $"{x.GroupId}-{x.OrganizationId}",
                    Name = $"{x.Group.Name} ({userOrgs.FirstOrDefault(o => o.OrganizationId == x.OrganizationId)?.Organization.Name})"
                }).ToList(),
                Users = users.Select(x => new FormRequestTargetUserDTO
                {
                    Id = x.Id,
                    Name = x.UserName
                }).ToList(),
            };
        }

        public async Task<PageResult<FormRequestPageDTO>> GetPage(QueryCommand command, CancellationToken ct = default)
        {
            // filter pagedData for the current user
            return await _dataService.GetPage<FormRequest, FormRequestPageDTO>(
                command,
                query => GetEntityQuery(query).Where(formRequest => formRequest.FormData!.UserId.Equals(_currentUserId)),
                filter: Filter,
                sorter: Sorter,
                ct);
        }

        private IQueryable<FormRequest> Sorter(IQueryable<FormRequest> query, ISorter sorter)
        {
            switch (sorter.SortingField)
            {
                case "formRevision.creatorName":
                    query = sorter.SortingDirection == OrderDirection.Desc
                        ? query.OrderByDescending(request => request.FormRevision!.Creator!.UserName ?? "")
                        : query.OrderBy(request => request.FormRevision!.Creator!.UserName ?? "");
                    sorter.SortingField = null;
                    break;
            }

            return query;
        }

        private QueryFilter<FormRequest> Filter(QueryFilter<FormRequest> filter)
        {
            return filter;
        }
    }
}