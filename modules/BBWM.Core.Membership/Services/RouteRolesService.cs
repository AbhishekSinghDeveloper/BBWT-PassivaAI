using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Exceptions;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.Utils;
using BBWM.Core.Web.Filters;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BBWM.Core.Membership.Services;

public class RouteRolesService : IRouteRolesService
{
    private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
    private readonly IUserService _userService;
    private readonly UserManager<User> _userManager;
    private readonly IEnumerable<IRouteRolesModule> _routeRolesModules;


    public RouteRolesService(
        IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
        IUserService userService,
        IEnumerable<IRouteRolesModule> routeRolesModules,
        UserManager<User> userManager)
    {
        _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        _userService = userService;
        _userManager = userManager;
        _routeRolesModules = routeRolesModules;
    }

    public IEnumerable<ApiEndPointInfoDTO> GetApiRoutesRoles() =>
        _actionDescriptorCollectionProvider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
            .Select(GetApiEndPointInfo)
            .OrderBy(a => a.Path)
            .ThenBy(a => a.Method);

    private static ApiEndPointInfoDTO GetApiEndPointInfo(ControllerActionDescriptor descriptor) =>
        new()
        {
            Method = GetApiEndPointMethod(descriptor),
            Path = GetApiEndPointPath(descriptor),
            Roles = GetApiEndPointRoles(descriptor),
            Permissions = GetApiEndPointPermissions(descriptor)
        };

    private static string GetApiEndPointPath(ControllerActionDescriptor descriptor) =>
        descriptor.AttributeRouteInfo?.Template ??
            $"{descriptor.RouteValues["controller"]}/{descriptor.RouteValues["action"]}".ToLowerInvariant();

    private static string GetApiEndPointMethod(ControllerActionDescriptor descriptor) =>
        string.Join(", ",
                descriptor.ActionConstraints?.OfType<HttpMethodActionConstraint>()
                .FirstOrDefault()?
                .HttpMethods
            ?? new[] { "GET" });

    private static List<string> GetApiEndPointPermissions(ControllerActionDescriptor descriptor)
    {
        var assemblyPermissionNames = PermissionsExtractor.GetAllPermissionNamesOfAssembly(descriptor.ControllerTypeInfo.Assembly);
        var authorizeAttribute = descriptor.EndpointMetadata.OfType<AuthorizeAttribute>().ToList();

        return authorizeAttribute
            .Where(x => assemblyPermissionNames.Contains(x.Policy))
            .Select(x => x.Policy)
            .ToList();
    }

    private static List<string> GetApiEndPointRoles(ControllerActionDescriptor descriptor)
    {
        var roles = new List<string>();

        var authorizeAttribute = descriptor.EndpointMetadata.OfType<AuthorizeAttribute>().ToList();

        if (descriptor.FilterDescriptors.Any(fd => fd.Filter is AllowAnonymousFilter) || !descriptor.FilterDescriptors.Any(fd => fd.Filter is AuthorizeFilter))
        {
            roles.Add(AggregatedRoles.Anyone);
        }
        else
        {
            var list = new List<string>();

            if (authorizeAttribute.Any())
            {
                list = authorizeAttribute
                    .Where(attributeItem => attributeItem.Roles is not null)
                    .SelectMany(attributeItem => attributeItem.Roles.Split(","))
                    .Select(roleItem => roleItem.Trim())
                    .ToList();
            }
            else
            {
                var readWriteAuthorizeAttribute = descriptor.EndpointMetadata.OfType<ReadWriteAuthorizeAttribute>().SingleOrDefault();
                if (readWriteAuthorizeAttribute is not null)
                {
                    list = ParseReadWriteAttrubuteRoles(readWriteAuthorizeAttribute, GetApiEndPointMethod(descriptor));
                }
            }

            if (list.Any())
            {
                roles.AddRange(list);
            }

            if (roles.Count == 0)
            {
                roles.Add(AggregatedRoles.Authenticated);
            }
        }

        return roles.Distinct().ToList();
    }

    private static List<string> ParseReadWriteAttrubuteRoles(ReadWriteAuthorizeAttribute attribute, string method)
    {
        var roles = new List<string>();

        var readWriteRoles = attribute.ReadWriteRoles;

        if (readWriteRoles is null)
        {
            switch (method)
            {
                case "GET":
                    readWriteRoles = attribute.ReadRoles;
                    break;
                case "POST":
                case "PUT":
                case "PATCH":
                case "DELETE":
                    readWriteRoles = attribute.WriteRoles;
                    break;

                default:
                    break;
            }
        }

        if (readWriteRoles is not null)
        {
            roles = readWriteRoles.Split(", ").Select(roleItem => roleItem.Trim()).ToList();
        }

        return roles;
    }

    public IEnumerable<PageInfoDTO> GetPagesRoutes()
    {
        var result = new List<PageInfoDTO>();

        // Link project & modules routes with module linker
        var linkers = ModuleLinker.ModuleLinker.GetInstances<IRouteRolesModuleLinkage>();
        linkers.ForEach(o => result.AddRange(o.GetRouteRoles(null)));

        // Link project & modules routes with DI
        foreach (var p in _routeRolesModules)
        {
            result.AddRange(p.GetRouteRoles());
        }

        return result;
    }

    public async Task<string[]> GetPageRoutesForUser(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(x => x.UserGroups).ThenInclude(x => x.Group)
            .Include(x => x.UserRoles).ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(o => o.Id == userId, cancellationToken);

        if (user is null) throw new UserNotExistsException();

        var allUserPermissions = await _userService.GetAllUserPermissions(userId, cancellationToken);

        return GetPagesRoutes()
            .Where(pageInfoItem =>
                (HasAccessByRoles(pageInfoItem, user.UserRoles.Select(x => x.Role.Name)) ||
                 pageInfoItem.Permissions.Intersect(allUserPermissions.Select(o => o.Name)).Any())
                &&
                (!pageInfoItem.Groups.Any() ||
                 pageInfoItem.Groups.Intersect(user.UserGroups.Select(o => o.Group.Name)).Any()))
            .Select(roteRolesItem => roteRolesItem.Path)
            .ToArray();
    }

    private static bool HasAccessByRoles(PageInfoDTO pageInfo, IEnumerable<string> userRoles) =>
        pageInfo.Roles.Contains(AggregatedRoles.Anyone) ||
        pageInfo.Roles.Contains(AggregatedRoles.Authenticated) ||
        pageInfo.Roles.Intersect(userRoles).Any();
}
