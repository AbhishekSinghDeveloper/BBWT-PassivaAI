using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Filters;
using BBWM.Core.Filters.TypedFilters;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Services;
using BBWM.Core.Web.Extensions;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Interfaces;
using BBWM.FormIO.Models;
using BBWM.Messages.Templates;
using BBWM.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Specialized;
using BBWM.Core.ModelHashing;
using BBWM.FormIO.Interfaces.FormViewInterfaces;
using Z.EntityFramework.Plus;

namespace BBWM.FormIO.Services
{
    public class FormIOMultiUserFormAssociationsService : IFormIOMultiUserFormAssociationsService
    {
        private readonly IDbContext _context;
        private readonly string _currentUserId;
        private readonly UserManager<User> _userManager;
        private readonly IDataService _dataService;
        private readonly IEmailSender _emailSender;
        private readonly ISecurityService _securityService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IModelHashingService _modelHashingService;

        private string DomainUrl => _httpContextAccessor.HttpContext.GetDomainUrl();
        private const string FormioExternalUserRole = "FormioExternalUser";

        public FormIOMultiUserFormAssociationsService(
            IDbContext context,
            IModelHashingService modelHashingService,
            IDataService dataService,
            UserManager<User> userManager,
            ISecurityService securityService,
            IHttpContextAccessor httpContextAccessor,
            IEmailSender emailSender,
            IEmailTemplateService emailTemplateService)
        {
            _context = context;
            _emailSender = emailSender;
            _emailTemplateService = emailTemplateService;
            _modelHashingService = modelHashingService;
            _userManager = userManager;
            _securityService = securityService;
            _dataService = dataService;
            _currentUserId = httpContextAccessor.HttpContext.GetUserId();
            _httpContextAccessor = httpContextAccessor;
        }

        public IQueryable<MultiUserFormAssociations> GetEntityQuery(IQueryable<MultiUserFormAssociations> baseQuery)
        {
            return baseQuery.Include(x => x.MultiUserFormAssociationLinks);
        }

        public static string SecurityCodeGenerator()
        {
            var rnd = new Random(Environment.TickCount);
            var code = "";
            for (var i = 0; i < 6; i++)
            {
                code += rnd.Next(0, 9).ToString();
            }

            return code;
        }

        public async Task<bool> NewMultiUserFormAssociation(NewMultiUserFormAssociationsDTO dto, CancellationToken ct)
        {
            var muf = await _context.Set<MultiUserFormDefinition>()
                .Include(multiUserForm => multiUserForm.FormRevision)
                .FirstOrDefaultAsync(multiUserForm => multiUserForm.Id == dto.MultiUserFormDefinitionId);

            if (muf == null) throw new ArgumentNullException(nameof(muf));

            var data = await _context.Set<FormData>().AddAsync(new FormData
            {
                CreatedOn = dto.Created,
                Json = "{}",
                UserId = _currentUserId,
                FormDefinitionId = muf.FormRevision?.FormDefinitionId,
            });
            _context.SaveChanges();

            var dataId = _modelHashingService.HashProperty(typeof(FormDataDTO), nameof(FormDataDTO.Id), data.Entity.Id);
            var mufAssoc = await _context.Set<MultiUserFormAssociations>().AddAsync(new MultiUserFormAssociations
            {
                Created = dto.Created,
                Description = dto.Description,
                FormDataId = data.Entity.Id,
                MultiUserFormDefinitionId = muf.Id,
                ActiveStepSequenceIndex = 1,
                TotalSequenceSteps = dto.MultiUserFormAssociationLinks.Count
            });
            _context.SaveChanges();
            var mufAssocId =
                _modelHashingService.HashProperty(typeof(MultiUserFormAssociationsDTO), nameof(MultiUserFormAssociationsDTO.Id), mufAssoc.Entity.Id);
            foreach (var item in dto.MultiUserFormAssociationLinks)
            {
                // Create the user if it doesnt exist
                var otp = SecurityCodeGenerator();
                var newPassword = _securityService.GetHashedPassword(otp);
                if (!string.IsNullOrEmpty(item.ExternalUserEmail))
                {
                    // if this assoc link points to a first line stage and is an external user? then send the email.
                    var startsNow = await _context.Set<MultiUserFormStage>().AnyAsync(x => x.Id == item.StageId && x.SequenceStepIndex == 1);
                    var url = $"app/formio/multiuser/external?mufId={mufAssocId}&formDataId={dataId}&target={item.ExternalUserEmail}";
                    var user = await _userManager.FindByEmailAsync(item.ExternalUserEmail);
                    if (user != null)
                    {
                        try
                        {
                            await _userManager.RemovePasswordAsync(user).ConfigureAwait(false);
                            var result = await _userManager.AddPasswordAsync(user, newPassword).ConfigureAwait(false);
                            if (result.Succeeded)
                            {
                                // Send Email only when the stage is about to be enabled
                                if (startsNow) await SendEmail(item.ExternalUserEmail, otp, url, $"{muf.Name}({dto.Description})", ct);
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    else
                    {
                        user = new User
                        {
                            UserName = item.ExternalUserEmail,
                            FirstName = "External",
                            LastName = "User",
                            Email = item.ExternalUserEmail
                        };
                        var result = await _userManager.CreateAsync(user, newPassword);
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, FormioExternalUserRole);
                            await _context.Set<User>().UpdateFromQueryAsync(x => new User
                                { EmailConfirmed = true, AccountStatus = Core.Membership.Enums.AccountStatus.Active });
                            try
                            {
                                // Send Email only when the stage is about to be enabled
                                if (startsNow) await SendEmail(item.ExternalUserEmail, otp, url, $"{muf.Name}({dto.Description})", ct);
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }
                }

                await _context.Set<MultiUserFormAssociationLinks>().AddAsync(new MultiUserFormAssociationLinks
                {
                    MultiUserFormAssociationsId = mufAssoc.Entity.Id,
                    MultiUserFormStageId = item.StageId,
                    ExternalUserEmail = string.IsNullOrEmpty(item.ExternalUserEmail) ? null : item.ExternalUserEmail,
                    SecurityCode = string.IsNullOrEmpty(item.ExternalUserEmail) ? string.Empty : otp,
                    UserId = string.IsNullOrEmpty(item.UserId) ? null : item.UserId,
                });
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task Delete(int id, CancellationToken ct = default)
        {
            var mufassoc = await _context.Set<MultiUserFormAssociations>().Include(x => x.FormData).Include(x => x.MultiUserFormAssociationLinks)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (mufassoc == null) throw new BusinessException("MUF_Assoc not found");
            if (mufassoc.FormData.Json.Length > 6) throw new BusinessException("This MUF association is in use. Can't be deleted.");
            await _context.Set<FormData>().Where(x => x.Id == mufassoc.FormDataId).DeleteAsync();
            mufassoc.MultiUserFormAssociationLinks.ToList().ForEach(x =>
            {
                _context.Set<MultiUserFormAssociationLinks>().Where(x => x.MultiUserFormAssociationsId == mufassoc.Id).DeleteAsync();
            });
            _context.SaveChanges();
        }

        public async Task<PageResult<MultiUserFormAssociationsDTO>> GetPage(QueryCommand command, CancellationToken ct = default)
        {
            #region CustomFilter Management

            var userId = "";
            if (command.Filters.Any(x => x.PropertyName == "userId"))
            {
                userId = (command.Filters.FirstOrDefault(x => x.PropertyName == "userId") as StringFilter)?.Value ?? "";
                command.Filters.RemoveAll(x => x.PropertyName == "userId");
            }

            #endregion

            var result = await _dataService.GetPage<MultiUserFormAssociations, MultiUserFormAssociationsDTO>(
                command,
                query => MultiUserFormAssociationsQueryGenerator(command, query, userId),
                filter: Filter,
                sorter: Sorter,
                ct);
            foreach (var item in result.Items)
            {
                // Check for wrong sequence step index
                if (item.MultiUserFormAssociationLinks.Any(x => !x.IsFilled) && (item.MultiUserFormAssociationLinks
                        .FirstOrDefault(x => x.MultiUserFormStage.SequenceStepIndex == item.ActiveStepSequenceIndex)?.IsFilled ?? false))
                {
                    var index = item.MultiUserFormAssociationLinks.Where(x => !x.IsFilled).OrderBy(x => x.MultiUserFormStage.SequenceStepIndex).FirstOrDefault()
                        ?.MultiUserFormStage.SequenceStepIndex ?? -1;
                    if (index != -1)
                    {
                        item.ActiveStepSequenceIndex = index;
                    }
                }
            }

            return result;
        }

        private IQueryable<MultiUserFormAssociations> MultiUserFormAssociationsQueryGenerator(QueryCommand command, IQueryable<MultiUserFormAssociations> query,
            string userId)
        {
            query = query.Include(x => x.MultiUserFormDefinition).Include(x => x.MultiUserFormAssociationLinks).ThenInclude(x => x.MultiUserFormStage);
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(x => x.MultiUserFormAssociationLinks.Any(l => l.UserId == userId));
            }

            return query;
        }


        public async Task<MultiUserFormAssociationsDTO> GetMUFDataForRendering(int id, string targetUserId, CancellationToken ct)
        {
            var user = await _dataService.Context.Set<User>().FindAsync(targetUserId);
            var userId = user != null ? user.Id : targetUserId;

            var mufAssoc = await _dataService.Get<MultiUserFormAssociations, MultiUserFormAssociationsDTO>(
                id,
                queryHandler => queryHandler
                    .Include(x => x.FormData)
                    .ThenInclude(x => x.FormDefinition)
                    .Include(x => x.MultiUserFormDefinition)
                    .Include(x => x.MultiUserFormAssociationLinks
                        .Where(links => !links.IsFilled && (links.ExternalUserEmail == userId || links.UserId == userId)))
                    .ThenInclude(x => x.MultiUserFormStage)
                    .ThenInclude(x => x.MultiUserFormStagePermissions),
                ct);
            if (mufAssoc != null && mufAssoc.MultiUserFormAssociationLinks.Any())
                mufAssoc.MultiUserFormAssociationLinks = new List<MultiUserFormAssociationLinks>()
                    { mufAssoc.MultiUserFormAssociationLinks.OrderBy(x => x.MultiUserFormStageId).First() };
            return mufAssoc;
        }

        private IQueryable<MultiUserFormAssociations> Sorter(IQueryable<MultiUserFormAssociations> query, ISorter sorter)
        {
            switch (sorter.SortingField)
            {
                // TODO: how this sorter should work?
                case "activeStageAssociation":
                    query = sorter.SortingDirection == OrderDirection.Desc
                        ? query.OrderByDescending(muf => muf.MultiUserFormAssociationLinks.Any(links => links.IsFilled))
                        : query.OrderBy(muf => muf.MultiUserFormAssociationLinks.Any(links => links.IsFilled));
                    sorter.SortingField = null;
                    break;
            }

            return query;
        }

        private Core.Filters.QueryFilter<MultiUserFormAssociations> Filter(Core.Filters.QueryFilter<MultiUserFormAssociations> queryFilter)
        {
            return queryFilter;
        }

        private async Task SendEmail(string email, string otp, string url, string name, CancellationToken ct)
        {
            // Send email
            var emailTemplate = await _emailTemplateService.GetByCode("FormIOExternalUserNotification", ct);
            if (emailTemplate is null)
                throw new ConflictException("Email template for the user invitation not found.");

            // Generate callbackUrl
            var callbackUrl = $"{DomainUrl}/{url}";

            var tagValues = new NameValueCollection
            {
                { "$UserName", "dear friend" },
                { "$CallbackUrl", callbackUrl },
                { "$OTP", otp },
                { "$FormIOName", name }
            };
            _emailTemplateService.BuildEmail(emailTemplate, tagValues);

            await _emailSender.SendEmail(emailTemplate.Subject, emailTemplate.Body, from: emailTemplate.From, to: email);
        }
    }
}