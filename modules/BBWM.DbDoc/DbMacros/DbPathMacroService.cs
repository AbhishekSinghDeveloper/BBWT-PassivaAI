using BBWM.Core.Exceptions;
using BBWM.DbDoc.DbGraph;
using BBWM.DbDoc.DbGraph.Algorithms;
using BBWM.DbDoc.DbSchemas;
using BBWM.DbDoc.DbSchemas.Interfaces;
using BBWM.DbDoc.DbSchemas.SchemaModels;

namespace BBWM.DbDoc.DbMacros;

public class DbPathMacroService : IDbPathMacroService
{
    private readonly IDbSchemaManager dbSchemaManager;

    public DbPathMacroService(IDbSchemaManager dbSchemaManager)
    {
        this.dbSchemaManager = dbSchemaManager;
    }

    public DbPathMacro GetPathMacroByAlias(Guid databaseSourceId, string macroAlias)
    {
        var dbSchema = dbSchemaManager.GetDbSchema(databaseSourceId)
            ?? throw new ObjectNotExistsException("Database source not found for specified ID.");

        var macroDef = BuiltinDbPathMacros.GetAll().FirstOrDefault(x => x.Alias == macroAlias)
            ?? throw new ObjectNotExistsException($"Macro definition not found for specified alias '{macroAlias}'.");

        var searchFilter = new GraphPathSearchFilter { MaxDepth = 4, MaxTotal = 1000 };
        return TracePathMacro(macroDef, dbSchema, searchFilter);
    }

    public IEnumerable<DbPathMacro> GetPathMacrosAllAliases(Guid databaseSourceId) =>
        BuiltinDbPathMacros.GetAll().Select(x => GetPathMacroByAlias(databaseSourceId, x.Alias));

    public DbPathMacro TracePathMacro(DbPathMacroDefinition macroDef, DbSchema dbSchema, GraphPathSearchFilter searchFilter = null)
    {
        var macro = new DbPathMacro
        {
            Definition = macroDef,
            Path = new DbGraphPath()
        };

        var graph = dbSchema.ToTablesGraph();
        var pathSearcher = new GraphPathSearcher(graph);

        var sourceTable = dbSchema.Tables.Values.FirstOrDefault(x => x.TableName.Equals(macroDef.SourceTable, StringComparison.InvariantCultureIgnoreCase));
        var targetTable = dbSchema.Tables.Values.FirstOrDefault(x => x.TableName.Equals(macroDef.TargetTable, StringComparison.InvariantCultureIgnoreCase));

        if (sourceTable is not null && targetTable is not null)
        {
            try
            {
                var foundPaths = pathSearcher.SearchPaths(sourceTable.TableId, targetTable.TableId,
                    searchFilter ?? new GraphPathSearchFilter());

                // The target table should be referenced to its PK field because we create macro for
                // calculating target table's record by ID.
                // Therefore we find only paths which references to PK of the last table in the path.
                foundPaths = foundPaths
                    .Where(x => x.Last().End.Data.PrimaryKeyColumn.ColumnId == x.Last().Data.EndTableColumn.ColumnId);

                if (foundPaths.Count() == 1)
                {
                    macro.Path = foundPaths.First();
                }
                else if (foundPaths.Count() > 1)
                {
                    // order by path legth ascending so we select the shortest path as priority choice
                    foundPaths = foundPaths.OrderBy(x => x.Count()).ToList();

                    // If the node that refers to target table has referring column with an expected name
                    // (e.g "OrganizationId" referencing to target "Organizations" table ) then this path is a preferable choice.
                    // Doing so we correctly auto-select Users -> UserOrganizations -> Organizations than
                    // Users.DefaultOrganizationID -> Organizations, for example.
                    // It's just an extra filtering option to automate the search
                    if (!string.IsNullOrEmpty(macroDef.ExpectedTargetReferencingColumn))
                    {
                        macro.Path = foundPaths.FirstOrDefault(x =>
                            x.Last().Data.StartTableColumn.ColumnName.Equals(
                                macroDef.ExpectedTargetReferencingColumn, StringComparison.InvariantCultureIgnoreCase));
                    }

                    if (macro.Path is null || !macro.Path.Any())
                    {
                        macro.Path = foundPaths.First();
                    }
                }
            }
            catch (Exception)
            {
                //
            }
        }

        return macro;
    }
}
