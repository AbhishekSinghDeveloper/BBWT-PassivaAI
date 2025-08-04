using BBWM.Core.Web;

namespace BBWM.Core.Membership.DTO;

public class PageInfoDTO
{

    public PageInfoDTO() { }

    public PageInfoDTO(
        string path,
        string title,
        IEnumerable<string> roles)
    => (Path, Title, Roles) =
        (path, title, roles is null ? new List<string>() : new List<string>(roles));

    public PageInfoDTO(
        string path,
        string title,
        string role)
        : this(path, title, new List<string> { role })
    {
    }

    public PageInfoDTO(
        string path,
        string title)
        : this(path, title, new List<string>())
    {
    }

    public PageInfoDTO(
        Route route,
        string role = null)
        : this(route.Path, route.Title, role is not null ? new List<string> { role } : null)
    {
    }

    public PageInfoDTO(
        Route route,
        IEnumerable<string> roles)
        : this(route.Path, route.Title, roles)
    {
    }

    public string Path { get; set; }
    public string Title { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
    public List<string> Permissions { get; set; } = new List<string>();
    public List<string> Groups { get; set; } = new List<string>();

    public PageInfoDTO ForRoles(IEnumerable<string> roles)
    {
        Roles.AddRange(roles);
        return this;
    }

    public PageInfoDTO ForRole(string role) => ForRoles(new List<string> { role });

    public PageInfoDTO ForPermissions(IEnumerable<string> permissions)
    {
        Permissions.AddRange(permissions);
        return this;
    }

    public PageInfoDTO ForPermission(string permission) => ForPermissions(new List<string> { permission });

    public PageInfoDTO ForGroups(IEnumerable<string> groups)
    {
        Groups.AddRange(groups);
        return this;
    }

    public PageInfoDTO ForGroup(string group) => ForGroups(new List<string> { group });

}
