using AutoMapper;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Model;
using BBWM.Core.Utils;
using BBWM.Reporting.DTO;
using BBWM.Reporting.Model;
using BBWM.Reporting.Services;

using Microsoft.AspNetCore.Identity;

using System.Text.Json.Nodes;

namespace BBWM.Reporting;

public class ModuleMappingProfile : Profile
{
    public ModuleMappingProfile()
    {
        CreateMap<Report, ReportDTO>()
            .ForMember(d => d.Roles, m => m.MapFrom(s => s.ReportRoles))
            .ForMember(d => d.Permissions, m => m.MapFrom(s => s.ReportPermissions))
            .ForMember(d => d.CreatedBy, m => m.MapFrom<ReportUserResolver, string>(nameof(Report.CreatedBy)))
            .ForMember(d => d.UpdatedBy, m => m.MapFrom<ReportUserResolver, string>(nameof(Report.UpdatedBy)))
            .ReverseMap()
            .ForMember(d => d.ReportRoles, m => m.Ignore())
            .ForMember(d => d.ReportPermissions, m => m.Ignore())
            .ForMember(d => d.Sections, m => m.Ignore());

        CreateMap<ReportRole, RoleDTO>()
            .ForMember(d => d.Id, m => m.MapFrom(s => s.Role.Id))
            .ForMember(d => d.Name, m => m.MapFrom(s => s.Role.Name))
            .ForMember(d => d.AuthenticatorRequired, m => m.MapFrom(s => s.Role.AuthenticatorRequired))
            .ForMember(d => d.CheckIp, m => m.MapFrom(s => s.Role.CheckIp))
            .ForMember(d => d.Permissions, opts => opts.Ignore());

        CreateMap<ReportPermission, PermissionDTO>()
            .ForMember(d => d.Id, m => m.MapFrom(s => s.Permission.Id))
            .ForMember(d => d.Name, m => m.MapFrom(s => s.Permission.Name));

        CreateMap<Section, SectionDTO>()
            .ForMember(d => d.Report, m => m.Ignore())
            .ReverseMap()
            .ForMember(d => d.NamedQuery, m => m.Ignore())
            .ForMember(d => d.Query, m => m.Ignore())
            .ForMember(d => d.Report, m => m.Ignore())
            .ForMember(d => d.ReusedSection, m => m.Ignore());

        CreateMap<QueryTable, QueryTableDTO>()
            .ReverseMap()
            .ForMember(d => d.Columns, m => m.Ignore())
            .ForMember(d => d.Query, m => m.Ignore());

        CreateMap<QueryTableColumn, QueryTableColumnDTO>()
            .ReverseMap()
            .ForMember(d => d.QueryTable, m => m.Ignore());

        CreateMap<QueryFilter, QueryFilterDTO>()
            .ForMember(d => d.CustomSqlCodeInserts, m => m.MapFrom(s =>
                QueryBuilderService.GetSqlFilterCodeInserts(s.CustomSqlCodeTemplate)))
            .ReverseMap()
            .ForMember(d => d.QueryTableColumn, m => m.Ignore())
            .ForMember(d => d.QueryFilterSet, m => m.Ignore())
            .ForMember(d => d.QueryRule, m => m.Ignore())
            .ForMember(d => d.QueryFilterBindings, m => m.Ignore());

        CreateMap<QueryFilterSet, QueryFilterSetDTO>()
            .ReverseMap()
            .ForMember(d => d.Query, m => m.Ignore())
            .ForMember(d => d.Parent, m => m.Ignore())
            .ForMember(d => d.ParentQuery, m => m.Ignore())
            .ForMember(d => d.QueryFilters, m => m.Ignore())
            .ForMember(d => d.ChildSets, m => m.Ignore());

        CreateMap<GridView, GridViewDTO>()
            .ReverseMap()
            .ForMember(d => d.DefaultSortColumn, m => m.Ignore())
            .ForMember(d => d.ViewColumns, m => m.Ignore())
            .ForMember(d => d.View, m => m.Ignore());

        CreateMap<GridViewColumn, GridViewColumnDTO>()
            .ForMember(d => d.ExtraSettings, m => m.MapFrom(x => JsonNode.Parse(x.ExtraSettings ?? "{}", default, default)))
            .ForMember(d => d.Footer, m => m.MapFrom(x => JsonNode.Parse(x.Footer ?? "{}", default, default)))
            .ForMember(d => d.GridView, m => m.Ignore())
            .ForMember(d => d.QueryTableColumn, m => m.Ignore())
            .ReverseMap()
            .ForMember(d => d.ExtraSettings, m => m.MapFrom(x => x.ExtraSettings.ToJsonString(JsonSerializerOptionsProvider.Options)))
            .ForMember(d => d.Footer, m => m.MapFrom(x => x.Footer.ToJsonString(JsonSerializerOptionsProvider.Options)))
            .ForMember(d => d.CustomColumnType, m => m.Ignore())
            .ForMember(d => d.QueryTableColumn, m => m.Ignore())
            .ForMember(d => d.GridView, m => m.Ignore());

        CreateMap<FilterControl, FilterControlDTO>()
            .ForMember(d => d.ExtraSettings, m => m.MapFrom(x => JsonNode.Parse(x.ExtraSettings ?? "{}", default, default)))
            .ReverseMap()
            .ForMember(d => d.ExtraSettings, m => m.MapFrom(x => x.ExtraSettings.ToJsonString(JsonSerializerOptionsProvider.Options)))
            .ForMember(d => d.QueryFilterBindings, m => m.Ignore())
            .ForMember(d => d.MasterControl, m => m.Ignore())
            .ForMember(d => d.View, m => m.Ignore());

        CreateMap<QueryFilterBinding, QueryFilterBindingDTO>()
            .ReverseMap()
            .ForMember(d => d.QueryFilter, m => m.Ignore())
            .ForMember(d => d.FilterControl, m => m.Ignore())
            .ForMember(d => d.MasterDetailSection, m => m.Ignore())
            .ForMember(d => d.MasterDetailQueryTableColumn, m => m.Ignore());

        CreateMap<QueryTableJoin, QueryTableJoinDTO>()
            .ReverseMap()
            .ForMember(d => d.Query, m => m.Ignore())
            .ForMember(d => d.FromQueryTable, m => m.Ignore())
            .ForMember(d => d.ToQueryTable, m => m.Ignore());
    }
}

public class ReportUserResolver : IMemberValueResolver<Report, ReportDTO, string, string>
{
    private readonly UserManager<User> _userManager;

    public ReportUserResolver()
    { }

    public ReportUserResolver(UserManager<User> userManager) => _userManager = userManager;

    public string Resolve(
        Report source,
        ReportDTO destination,
        string sourceMember,
        string destMember,
        ResolutionContext context)
    {
        var user = _userManager.FindByIdAsync(sourceMember == nameof(Report.CreatedBy) ? source.CreatedBy : source.UpdatedBy).Result;
        return user != null ? $"{user.FirstName} {user.LastName}" : string.Empty;
    }
}
