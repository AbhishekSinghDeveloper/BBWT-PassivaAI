using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Filters;
using BBWM.Core.Membership.Model;
using BBWM.Core.Services;
using BBWM.Core.Web.Extensions;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Enums;
using BBWM.FormIO.Interfaces;
using BBWM.FormIO.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Group = BBWM.Core.Membership.Model.Group;

namespace BBWM.FormIO.Services
{
    public class FormIOMultiUserFormDefinitionService : IFormIOMultiUserFormDefinitionService
    {
        private readonly IDbContext _context;
        private readonly string _currentUserId;
        private readonly IDataService _dataService;

        public FormIOMultiUserFormDefinitionService(
            IDbContext context,
            IDataService dataService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _dataService = dataService;
            _currentUserId = httpContextAccessor.HttpContext.GetUserId();
        }

        public async Task Delete(int id, CancellationToken ct = default)
        {
            var target = await _dataService.Context.Set<MultiUserFormDefinition>()
                .Include(x => x.MultiUserFormAssociations)
                .Where(x => x.Id == id && !x.MultiUserFormAssociations.Any())
                .FirstOrDefaultAsync();

            if (target != null)
            {
                _dataService.Context.Set<MultiUserFormDefinition>().Remove(target);
                await _dataService.Context.SaveChangesAsync();
            }
            else
            {
                throw new BusinessException("The selected Multi-User Form has data associated. Can't be deleted.");
            }
        }

        public IQueryable<MultiUserFormDefinition> GetEntityQuery(IQueryable<MultiUserFormDefinition> baseQuery)
        {
            return baseQuery.Include(x => x.FormRevision)
                .ThenInclude(x => x.FormDefinition)
                .Include(x => x.MultiUserFormStages)
                .ThenInclude(x => x.Groups);
        }

        public async Task<List<FormDefinitionDTO>> GetFormDefinitions(CancellationToken cancellationToken)
        {
            var currentUser = await _dataService.Context.Set<User>().Include(x => x.UserOrganizations)
                .FirstAsync(x => x.Id == _currentUserId);

            var orgs = currentUser.UserOrganizations.Select(x => x.OrganizationId).ToList();

            var data = (await _dataService.GetAll<FormDefinition, FormDefinitionDTO>(query =>
                query
                    .Include(x => x.FormRevisions)
                    .Include(x => x.FormDefinitionOrganizations)
                    .Where(x => x.ManagerId == _currentUserId
                                || x.FormDefinitionOrganizations.Any(y => orgs.Any(id => id == y.OrganizationId))
                    ), cancellationToken)).ToList();
            var result = data.Where(x => x.ActiveRevision?.MUFCapable ?? false).ToList();

            return result;
        }

        public async Task<List<MultiUserFormTargetDTO>> GetInstanceTargets(int id, CancellationToken cancellationToken)
        {
            // Get all the orgs the current user belongs to
            var orgs = await _dataService.Context.Set<User>()
                .Where(x => x.Id == _currentUserId).Include(x => x.UserOrganizations)
                .SelectMany(x => x.UserOrganizations.Select(x => x.OrganizationId)).ToListAsync();

            var users = (await _dataService.Context.Set<MultiUserFormStage>()
                    .Include(s => s.Groups)
                    .ThenInclude(g => g.UserOrganizationGroups)
                    .Where(x => x.MultiUserFormDefinitionId == id)
                    .SelectMany(x => x.Groups)
                    .SelectMany(x => x.UserOrganizationGroups)
                    .Where(x => orgs.Any(o => o == x.OrganizationId))
                    .Join(_dataService.Context.Set<User>(),
                        userOrgGroup => userOrgGroup.UserId,
                        user => user.Id,
                        (userOrgGroup, user) => new MultiUserFormTargetDTO
                        {
                            Name = $"{user.UserName} ({userOrgGroup.Group.Name})",
                            Id = user.Id,
                            IdGroup = userOrgGroup.GroupId
                        }
                    )
                    .ToListAsync())
                .DistinctBy(x => new { x.Id, x.IdGroup })
                .ToList();
            return users;
        }

        public async Task<List<MultiUserFormTargetDTO>> GetPossibleTargets(CancellationToken cancellationToken)
        {
            // Get all the orgs the current user belongs to
            var orgs = await _dataService.Context.Set<User>().Where(x => x.Id == _currentUserId)
                .Include(x => x.UserOrganizations)
                .SelectMany(x => x.UserOrganizations.Select(x => x.OrganizationId)).ToListAsync();
            // Get all the users that belong to any of the current user's organizations
            return await _dataService.Context.Set<User>().Where(x => x.UserOrganizations
                    .Any(o => orgs.Any(p => p == o.OrganizationId)))
                .Select(x => new MultiUserFormTargetDTO
                {
                    Id = x.Id,
                    Name = $"{x.FirstName} {x.LastName}"
                }).ToListAsync();
        }

        public async Task<bool> NewMultiUserForm(NewMultiUserFormDefinitionDTO dto, CancellationToken cancellationToken)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            // if (dto.TargetsId == null || dto.TargetsId.Count == 0) throw new BusinessException("You must set the targets of this form");
            var currentUser = await _dataService.Context.Set<User>().FirstAsync(x => x.Id == _currentUserId);
            var formDef = await _dataService.Context.Set<FormDefinition>().FirstAsync(x => x.Id == dto.FormDefinitionId);
            if (currentUser == null || formDef == null)
            {
                throw new BusinessException("Invalid data detected");
            }

            var mufDef = new MultiUserFormDefinition
            {
                Name = dto.Name,
                CreatorId = _currentUserId,
                FormRevisionId = formDef.ActiveRevisionId,
            };
            await _dataService.Context.Set<MultiUserFormDefinition>().AddAsync(mufDef, cancellationToken);
            await _dataService.Context.SaveChangesAsync();
            // Creating default tabs and permissions
            for (int i = 0; i < dto.Tabs.Count; i++)
            {
                var tab = dto.Tabs[i];
                var stage = await _dataService.Context.Set<MultiUserFormStage>().AddAsync(new MultiUserFormStage
                {
                    Groups = new List<Group>(),
                    Name = $"Stage-{tab.InnerTab} on {tab.TabComponent}",
                    InnerTabKey = tab.InnerTab,
                    SequenceStepIndex = i + 1, // start numbering the stages at 1, not 0
                    TabComponentKey = tab.TabComponent,
                    StageTargetType = StageTargetType.InnerGroups,
                    MultiUserFormDefinitionId = mufDef.Id
                });
                await _dataService.Context.SaveChangesAsync();

                var perm = await _dataService.Context.Set<MultiUserFormStagePermissions>()
                    .AddAsync(new MultiUserFormStagePermissions
                {
                    TabKey = tab.InnerTab,
                    Action = MultiUserFormStagePermissionAction.Edit,
                    MultiUserFormStageId = stage.Entity.Id,
                });
                await _dataService.Context.SaveChangesAsync();
            }

            return false;
        }

        public async Task<PageResult<MultiUserFormDefinitionDTO>> GetPage(QueryCommand command, CancellationToken ct = default)
        {
            var pagedResult = await _dataService.GetPage<MultiUserFormDefinition, MultiUserFormDefinitionDTO>(
                command,
                GetEntityQuery,
                filter: Filter,
                sorter: Sorter,
                ct);

            return pagedResult;
        }

        private IQueryable<MultiUserFormDefinition> Sorter(IQueryable<MultiUserFormDefinition> query, ISorter sorter)
        {
            switch (sorter.SortingField)
            {
                case "setupReady":
                    query = sorter.SortingDirection == OrderDirection.Desc
                        ? query.OrderByDescending(x => !x.MultiUserFormStages
                            .Any(x => !x.Groups.Any() && x.StageTargetType == StageTargetType.InnerGroups))
                        : query.OrderBy(x => x.MultiUserFormStages
                            .Any(x => !x.Groups.Any() && x.StageTargetType == StageTargetType.InnerGroups));
                    sorter.SortingField = null;
                    break;
                case "isPublished":
                    query = sorter.SortingDirection == OrderDirection.Desc
                        ? query.OrderByDescending(x => !x.MultiUserFormStages.Any())
                        : query.OrderBy(x => x.MultiUserFormStages.Any());
                    sorter.SortingField = null;
                    break;
            }

            return query;
        }

        private QueryFilter<MultiUserFormDefinition> Filter(QueryFilter<MultiUserFormDefinition> filter)
        {
            return filter;
        }

        public async Task<bool> IsMUFReady(int id, CancellationToken cancellationToken)
        {
            return await _context.Set<MultiUserFormStage>()
                .Where(x => x.MultiUserFormDefinitionId == id)
                .AllAsync(x => x.StageTargetType != StageTargetType.InnerGroups || x.Groups.Any());
        }
    }
}