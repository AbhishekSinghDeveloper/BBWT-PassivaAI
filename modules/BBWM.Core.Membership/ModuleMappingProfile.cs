using AutoMapper;

using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Model;

using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace BBWM.Core.Membership;

public class ModuleMappingProfile : Profile
{
    public ModuleMappingProfile()
    {
        #region Identity

        CreateMap<User, UserDTO>()
                .ForMember(d => d.Claims, m => m.MapFrom<UserClaimsResolver>())
                .ForMember(d => d.Roles, m => m.MapFrom(s => s.UserRoles))
                .ForMember(d => d.Permissions, m => m.MapFrom(s => s.UserPermissions))
                .ForMember(d => d.Groups, m => m.MapFrom(s => s.UserGroups))
                .ForMember(d => d.Organizations, m => m.MapFrom(s => s.UserOrganizations))
                .ForMember(d => d.IsSuperAdmin, m => m.MapFrom(s => s.UserRoles.Any(x => x.Role.Name == Core.Roles.SuperAdminRole)))
                .ForMember(d => d.IsSystemAdmin, m => m.MapFrom(s => s.UserRoles.Any(x => x.Role.Name == Core.Roles.SystemAdminRole)))
                .ForMember(d => d.IsSystemTester, m => m.MapFrom(s => s.UserRoles.Any(x => x.Role.Name == Core.Roles.SystemTester)))
                .ForMember(d => d.Password, opts => opts.Ignore())
                .ForMember(d => d.ConfirmPassword, opts => opts.Ignore())
                .ForMember(d => d.IsUserRequiredSetupTwoFactor, opts => opts.Ignore())
            .ReverseMap()
                .ForMember(d => d.NormalizedEmail, m => m.MapFrom(s => s.Email.ToUpperInvariant()))
                .ForMember(d => d.NormalizedUserName, m => m.MapFrom(s => s.UserName.ToUpperInvariant()))
                .ForMember(d => d.Organization, m => m.Ignore())
                .ForMember(d => d.UserRoles, m => m.Ignore())
                .ForMember(d => d.UserPermissions, m => m.Ignore())
                .ForMember(d => d.UserGroups, m => m.Ignore())
                .ForMember(d => d.UserOrganizations, m => m.Ignore())
                .ForMember(d => d.RecoveryCode, m => m.Ignore());

        CreateMap<User, UserSignatureDTO>()
            .ForMember(d => d.Signature, m => m.MapFrom(s => s.UserSignatureJson))
            .ForMember(d => d.Name, m => m.MapFrom(s => $"{s.FirstName} {s.LastName}"))
            .ForMember(d => d.Username, m => m.MapFrom(s => s.UserName));

        CreateMap<UserRole, RoleDTO>()
            .ForMember(d => d.Id, m => m.MapFrom(s => s.Role.Id))
            .ForMember(d => d.Name, m => m.MapFrom(s => s.Role.Name))
            .ForMember(d => d.AuthenticatorRequired, m => m.MapFrom(s => s.Role.AuthenticatorRequired))
            .ForMember(d => d.CheckIp, m => m.MapFrom(s => s.Role.CheckIp))
            .ForMember(d => d.Permissions, opts => opts.Ignore());

        CreateMap<UserPermission, PermissionDTO>()
            .ForMember(d => d.Id, m => m.MapFrom(s => s.Permission.Id))
            .ForMember(d => d.Name, m => m.MapFrom(s => s.Permission.Name));

        CreateMap<Role, RoleDTO>()
            .ForMember(d => d.Permissions, m => m.MapFrom(s => s.RolePermissions))
            .ReverseMap()
            .ForMember(d => d.NormalizedName, m => m.MapFrom(s => s.Name.ToUpperInvariant()))
            .ForMember(d => d.UserRoles, m => m.Ignore())
            .ForMember(d => d.RolePermissions, m => m.Ignore());

        CreateMap<RolePermission, PermissionDTO>()
            .ForMember(d => d.Id, m => m.MapFrom(s => s.PermissionId))
            .ForMember(d => d.Name, m => m.MapFrom(s => s.Permission.Name));

        CreateMap<UserGroup, GroupDTO>()
            .ForMember(d => d.Id, m => m.MapFrom(s => s.Group.Id))
            .ForMember(d => d.Name, m => m.MapFrom(s => s.Group.Name));

        CreateMap<UserOrganization, OrganizationDTO>()
            .ForMember(d => d.Id, m => m.MapFrom(s => s.Organization.Id))
            .ForMember(d => d.Name, m => m.MapFrom(s => s.Organization.Name))
            .ForMember(d => d.Description, m => m.MapFrom(s => s.Organization.Description))
            .ForMember(d => d.Level, m => m.MapFrom(s => s.Organization.Level))
            .ForMember(d => d.AddressId, m => m.MapFrom(s => s.Organization.AddressId))
            .ForMember(d => d.BrandingId, m => m.MapFrom(s => s.Organization.BrandingId))
            .ForMember(d => d.Address, m => m.Ignore())
            .ForMember(d => d.Branding, m => m.Ignore());

        CreateMap<AuthenticationRequest, U2FRegistrationRequestDTO>();

        #endregion Identity

        // Allowed IP
        CreateMap<AllowedIp, AllowedIpDTO>()
            .ForMember(x => x.Users, y => y.MapFrom(z => z.AllowedIpUsers.Select(a => a.User)))
            .ForMember(x => x.Roles, y => y.MapFrom(z => z.AllowedIpRoles.Select(a => a.Role)))
            .ReverseMap()
            .ForMember(x => x.AllowedIpRoles, y => y.Ignore())
            .ForMember(x => x.AllowedIpUsers, y => y.Ignore());

        // Branding
        CreateMap<Branding, BrandingDTO>()
            .ReverseMap()
            .ForMember(x => x.Organization, y => y.Ignore())
            .ForMember(x => x.LogoImage, y => y.Ignore())
            .ForMember(x => x.LogoIcon, y => y.Ignore());
    }
}

public class UserClaimsResolver : IValueResolver<User, UserDTO, Dictionary<string, string>>
{
    private readonly UserManager<User> _userManager;

    public UserClaimsResolver()
    { }

    public UserClaimsResolver(UserManager<User> userManager) => _userManager = userManager;

    public Dictionary<string, string> Resolve(User source, UserDTO destination, Dictionary<string, string> destMember, ResolutionContext context)
    {
        if (_userManager is null) return new Dictionary<string, string>();

        var userClaimsFromDb = (_userManager.GetClaimsAsync(source)).Result;
        return userClaimsFromDb
            .GroupBy(a => a.Type)
            .ToDictionary(k => k.Key, v =>
            {
                var list = v.Select(a => a.Value).ToList();

                return list.Count == 1 ? list[0] : JsonSerializer.Serialize(list);
            });
    }
}