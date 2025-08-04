using BBWM.DbDoc.DbGraph.Algorithms;
using BBWM.DbDoc.DbSchemas.SchemaModels;

namespace BBWM.DbDoc.DbMacros
{
    public interface IDbPathMacroService
    {
        DbPathMacro GetPathMacroByAlias(Guid databaseSourceId, string macroAlias);
        IEnumerable<DbPathMacro> GetPathMacrosAllAliases(Guid databaseSourceId);
        DbPathMacro TracePathMacro(DbPathMacroDefinition macroDef, DbSchema dbSchema, GraphPathSearchFilter searchFilter = default);
    }
}
