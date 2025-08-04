using AutoMapper;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Filters;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Services;
using BBWM.Core.Web.Extensions;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Enums;
using BBWM.FormIO.Interfaces;
using BBWM.FormIO.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

namespace BBWM.FormIO.Services
{
    public class FormIOParameterListService : DataService, IFormIOParameterListService
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public FormIOParameterListService(
        IDbContext context,
        IMapper mapper) : base(context, mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public Task<FormParameterListDTO> Create(FormParameterListDTO dto, CancellationToken ct = default)
        {
            if (dto?.Position < 0)
            {
                dto.Position = null;
            }
           return base.Create<FormParameterList ,FormParameterListDTO>(dto, ct);
        }

        public Task Delete(int id, CancellationToken ct = default)
        {
            return base.Delete<FormParameterList>(id, ct);
        }

        public IQueryable<FormParameterList> GetEntityQuery(IQueryable<FormParameterList> baseQuery)
        {
            return baseQuery;
        }

        public Task<PageResult<FormParameterListDTO>> GetPage(QueryCommand command, CancellationToken ct = default)
        {
            return base.GetPage<FormParameterList, FormParameterListDTO>(command, ct);
        }

        public Task<FormParameterListDTO> Update(FormParameterListDTO dto, CancellationToken ct = default)
        {
            if (dto?.Position <= 0)
            {
                dto.Position = null;
            }
            return base.Update<FormParameterList, FormParameterListDTO>(dto, ct);
        }
    }
}
