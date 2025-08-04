using BBWM.Core.Web;

namespace BBWM.Demo.Security;

public static class Routes
{
    public static readonly Route SecurityReadMeFirst = new("/app/demo/security/readmefirst", "Read Me First");
    public static readonly Route SecurityAnyAuthenticated = new("/app/demo/security/accessible/any-authenticated", "Any Authenticated");
    public static readonly Route SecurityGroups = new("/app/demo/security/groups", "Groups");
    public static readonly Route SecurityGroupA = new("/app/demo/security/accessible/group/a", "Accessible to Group A");
    public static readonly Route SecurityGroupB = new("/app/demo/security/accessible/group/b", "Accessible to Group B");
    public static readonly Route SecurityNote1 = new("/app/demo/security/accessible/note1", "Accessible to Note 1");
    public static readonly Route SecurityNote2 = new("/app/demo/security/accessible/note2", "Accessible to Note 2");
}
