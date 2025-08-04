using BBWM.Core.Data;
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
    public class FormIOMultiUserFormStageService : IFormIOMultiUserFormStageService
    {
        private readonly IDbContext _context;
        private readonly string _currentUserId;
        private readonly IDataService _dataService;

        public FormIOMultiUserFormStageService(
        IDbContext context,
        IDataService dataService,
        IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _dataService = dataService;
            _currentUserId = httpContextAccessor.HttpContext.GetUserId();
        }

        public async Task<PageResult<MultiUserFormStageDTO>> GetPage(QueryCommand command, CancellationToken ct = default)
        {
            var result = await _dataService.GetPage<MultiUserFormStage, MultiUserFormStageDTO>(command,query => query
                .Include(x => x.Groups), ct);
            return result;
        }

        public async Task<List<MultiUserFormTargetDTO>> GetPossibleTargets(CancellationToken cancellationToken)
        {
            // Get all the orgs the current user belongs to
            var orgs = await _dataService.Context.Set<User>().Where(x => x.Id == _currentUserId)
                .Include(x => x.UserOrganizations).SelectMany(x => x.UserOrganizations
                    .Select(x => x.OrganizationId)).ToListAsync();
            // Get all the groups that are linked to any of the current user's organizations
            return await _dataService.Context.Set<Group>().Where(x => x.UserOrganizationGroups
                .Any(o => orgs.Any(p => p == o.OrganizationId)))
                .GroupBy(x => x.Id).Select(x => new MultiUserFormTargetDTO
            {
                IdGroup = x.Key,
                Name = $"{x.First().Name}"
            }).ToListAsync();
        }

        public async Task<bool> UpdateMultiUserStage(MultiUserFormStageUpdateDTO dto, CancellationToken cancellationToken)
        {
            var result = false;
            var stage = await _dataService.Context.Set<MultiUserFormStage>()
                .Include(x => x.Groups).FirstOrDefaultAsync(x => x.Id == dto.Id);
            var link = await _dataService.Context.Set<MultiUserFormAssociationLinks>()
                .FirstOrDefaultAsync(x => x.MultiUserFormStageId == dto.Id);

            var groups = new List<Group>();
            if (dto.StageTargetType != StageTargetType.ExternalUsers)
            {
                groups = await _dataService.Context.Set<Group>().Where(x => dto.GroupIds.Any(g => g == x.Id)).ToListAsync();
            }
            if (stage != null)
            {
                stage.Name = dto.Name;
                stage.StageTargetType = dto.StageTargetType;
                stage.Groups = groups;
                stage.ReviewerStage = dto.ReviewerStage;
                stage.SequenceStepIndex = dto.SequenceStepIndex;
                _dataService.Context.Set<MultiUserFormStage>().Update(stage);
                result = await _context.SaveChangesAsync() > 0;
            }
            if (link != null)
            {
                // Adjust all MUF associations to match the new set of Total Steps and recalculate the right activeStep accordingly
                var total = (await _dataService.Context.Set<MultiUserFormAssociationLinks>()
                    .Include(x => x.MultiUserFormStage)
                    .Where(x => x.MultiUserFormAssociationsId == link.MultiUserFormAssociationsId)
                    .OrderByDescending(x => x.MultiUserFormStage.SequenceStepIndex).FirstOrDefaultAsync())
                    ?.MultiUserFormStage.SequenceStepIndex ?? -1;
                if (total > 0)
                {
                    await _dataService.Context.Set<MultiUserFormAssociations>()
                        .Where(x => x.Id == link.MultiUserFormAssociationsId)
                        .UpdateFromQueryAsync(x => new MultiUserFormAssociations { TotalSequenceSteps = total });
                }
                var activeStep = (await _dataService.Context.Set<MultiUserFormAssociationLinks>()
                    .Include(x => x.MultiUserFormStage)
                    .Where(x => x.MultiUserFormAssociationsId == link.MultiUserFormAssociationsId)
                    .Where(x => !x.IsFilled).OrderBy(x => x.MultiUserFormStage.SequenceStepIndex)
                    .FirstOrDefaultAsync())?.MultiUserFormStage?.SequenceStepIndex ?? total;
                await _dataService.Context.Set<MultiUserFormAssociations>().Where(x => x.Id == link.MultiUserFormAssociationsId)
                    .UpdateFromQueryAsync(x => new MultiUserFormAssociations { ActiveStepSequenceIndex = activeStep });
            }
            return result;
        }
    }
}
