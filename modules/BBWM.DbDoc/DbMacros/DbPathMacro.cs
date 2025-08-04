using BBWM.DbDoc.DbGraph;

namespace BBWM.DbDoc.DbMacros;

public class DbPathMacro
{
    /// <summary>
    /// It can be macro e.g. "user", "org" for reporting to restrict query by end-user's ID/org's ID JOINs & WHERE.
    /// We can agree this macros syntax shown with "@" prefix in UI: @user, @org
    /// </summary>
    public DbPathMacroDefinition Definition { get; set; }

    public IDbGraphPath Path { get; set; }
}
