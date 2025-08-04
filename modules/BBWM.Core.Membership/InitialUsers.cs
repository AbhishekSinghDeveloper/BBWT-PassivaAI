using BBWM.Core.Membership.DTO;

namespace BBWM.Core.Membership;

public static class InitialUsers
{
    public static readonly UserDTO SystemAdmin = new()
    {
        Email = "systemadmin@bbconsult.co.uk",
        Password = "cover918Surface",
        FirstName = "System",
        LastName = "Admin",
    };

    public static readonly UserDTO SuperAdmin = new()
    {
        Email = "superadmin@bbconsult.co.uk",
        Password = "increase762Value",
        FirstName = "Super",
        LastName = "Admin",
    };

    // TO consider: we can collect them automatically
    public static List<UserDTO> GetAll() => new() { SystemAdmin, SuperAdmin };

    public static List<string> NotAllowedAsNewUserPassword()
    {
        var res = GetAll().Select(x => x.Password).ToList();

        // These passwords are already known because used in initial users records of the BBWM.Demo module. Therefore we don't allow them as well 
        res.AddRange(new string[] {
            "permanent672Mountain", "functional528Example", "fountain961Statue", "permanent3323group", "functional23group", "functional234235group"
        });

        return res;
    }
};
