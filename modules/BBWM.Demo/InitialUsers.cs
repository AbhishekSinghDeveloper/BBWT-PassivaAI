using BBWM.Core.Membership.DTO;

namespace BBWM.Demo;

public static class InitialUsers
{
    public static readonly UserDTO DemoAdmin = new()
    {
        Email = "demo-admin@bbconsult.co.uk",
        Password = "permanent672Mountain",
        FirstName = "Demo",
        LastName = "Admin"
    };

    public static readonly UserDTO DemoUser = new()
    {
        Email = "demo-user@bbconsult.co.uk",
        Password = "functional528Example",
        FirstName = "Demo",
        LastName = "User"
    };

    public static readonly UserDTO Manager = new()
    {
        Email = "demo-manager@bbconsult.co.uk",
        Password = "fountain961Statue",
        FirstName = "Demo",
        LastName = "Manager"
    };

    public static readonly UserDTO ManagerInGroupA = new()
    {
        Email = "manager-in-group-a@bbconsult.co.uk",
        Password = "permanent3323group",
        FirstName = "Manager",
        LastName = "Group-a"
    };

    public static readonly UserDTO ManagerInGroupB = new()
    {
        Email = "manager-in-group-b@bbconsult.co.uk",
        Password = "functional23group",
        FirstName = "Manager",
        LastName = "Group-b"
    };

    public static readonly UserDTO ManagerInGroupAB = new()
    {
        Email = "manager-in-groups-a-b@bbconsult.co.uk",
        Password = "functional234235group",
        FirstName = "Manager",
        LastName = "Group-ab"
    };
};
