using AutoMapper;
using AutoMapper.QueryableExtensions;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Filters;
using BBWM.Core.Filters.TypedFilters;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Services;
using BBWM.Core.Web.Extensions;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Enums;
using BBWM.FormIO.Extensions;
using BBWM.FormIO.Interfaces;
using BBWM.FormIO.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.OData.UriParser;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Twilio.Jwt.AccessToken;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Group = BBWM.Core.Membership.Model.Group;

namespace BBWM.FormIO.Services
{
    public class FormIOMultiUserFormPermissionsService : DataService, IFormIOMultiUserFormPermissionsService
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;
        private readonly string _currentUserId;
        private readonly IDataService _dataService;

        public FormIOMultiUserFormPermissionsService(
        IDbContext context,
        IMapper mapper,
        IDataService dataService,
        IHttpContextAccessor httpContextAccessor) : base(context, mapper)
        {
            _mapper = mapper;
            _context = context;
            _dataService = dataService;
            _currentUserId = httpContextAccessor.HttpContext.GetUserId();
        }

        public IQueryable<FormIOMultiUserFormPermissionsService> GetEntityQuery(IQueryable<FormIOMultiUserFormPermissionsService> baseQuery)
        {
            return baseQuery;
        }

        public async Task<bool> NewMultiUserStagePermission(NewMultiUserFormPermissionDTO dto, CancellationToken cancellationToken)
        {
            try
            {
                await this._dataService.Context.Set<MultiUserFormStagePermissions>().AddAsync(new MultiUserFormStagePermissions
                {
                    Action = (MultiUserFormStagePermissionAction)dto.Action,
                    MultiUserFormStageId = dto.StageId,
                    TabKey = dto.TabKey,
                }, cancellationToken);
                return this._context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
