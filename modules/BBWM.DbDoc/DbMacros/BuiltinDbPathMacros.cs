namespace BBWM.DbDoc.DbMacros;

public static class BuiltinDbPathMacros
{
    /// <summary>
    /// A path macro to detect the core oranizations table path to the core users table
    /// </summary>
    public static readonly DbPathMacroDefinition Organization = new()
    {
        Alias = "org",
        Description = "Used to filter users by organization",
        SourceTable = "AspNetUsers",
        TargetTable = "Organizations",
        ExpectedTargetReferencingColumn = "OrganizationId",
    };

    public static readonly DbPathMacroDefinition Test1 = new()
    {
        Alias = "demoPath1",
        Description = "A sample path macro",
        SourceTable = "DbDocFolders",
        TargetTable = "DbDocColumnType",
    };

    public static IEnumerable<DbPathMacroDefinition> GetAll() =>
        new[] { Organization, Test1 };
}
