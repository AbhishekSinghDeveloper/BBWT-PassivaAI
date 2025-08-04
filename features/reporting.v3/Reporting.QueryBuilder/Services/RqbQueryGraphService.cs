using BBF.Reporting.QueryBuilder.Interfaces;
using BBF.Reporting.QueryBuilder.Model;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.DbDoc.DbGraph;
using BBWM.DbDoc.DbSchemas;
using BBWM.DbDoc.DbSchemas.Interfaces;
using BBWM.DbDoc.DbSchemas.SchemaModels;
using BBWM.DbDoc.Model;
using NPOI.Util;

namespace BBF.Reporting.QueryBuilder.Services;

public class RqbQueryGraphService : IRqbQueryGraphService
{
    private readonly IDbContext _context;
    private readonly IDbSchemaManager _schemaManager;
    private readonly IDictionary<Guid, DbGraph> _graphs;

    public RqbQueryGraphService(IDbContext context, IDbSchemaManager schemaManager)
    {
        _context = context;
        _schemaManager = schemaManager;
        _graphs = new Dictionary<Guid, DbGraph>();
    }

    public IEnumerable<TablesRelation>? GetShortestPath(IEnumerable<string> sourceTablesNames,
        string destinyTableName, IEnumerable<TablesRelationExclusion>? exclusions = null)
    {
        // TODO: later we should consider that all tables of the graph are within a single DB.
        var databaseSources = _context.Set<DatabaseSource>().Select(source => source.Id).ToList();

        // Get destiny table id from its name.
        var tables = databaseSources
            .Select(source => _schemaManager.GetDbSchema(source))
            .SelectMany(schema => schema.Tables.Values);
        var destinyTableId = FindTableId(tables, destinyTableName);

        // If there is no destiny table, throw error.
        if (destinyTableId == null)
            throw new BusinessException($"{destinyTableName} table not found.");

        // Get destiny table schema.
        var schema = _schemaManager.GetTableDbSchema(destinyTableId);
        var databaseSourceId = schema.DatabaseSource.Id;

        // Register this schema's graph if it wasn't registered yet.
        if (!_graphs.ContainsKey(databaseSourceId))
            _graphs[databaseSourceId] = schema.ToTablesGraph();

        // Get source tables ids from its names.
        var sourceTablesIds = sourceTablesNames
            .Select(tableName => FindTableId(schema.Tables.Values, tableName))
            .Where(tableId => tableId != null)!.ToList<string>();

        // Get ids of exclusion edges ends from its names.
        var excludedRelationships = exclusions?.Select(exclusion =>
            {
                var startTableId = FindTableId(schema.Tables.Values, exclusion.StartTableIdentifier);
                var endTableId = FindTableId(schema.Tables.Values, exclusion.EndTableIdentifier);
                if (startTableId == null) return null;

                return new TablesRelationExclusion
                {
                    StartTableIdentifier = startTableId,
                    EndTableIdentifier = endTableId,
                    ExclusionMode = exclusion.ExclusionMode
                };
            })
            .Where(edge => edge != null)!.ToList<TablesRelationExclusion>();

        // If there is no source tables that belong to this schema, then there is no path.
        // Otherwise, get shortest path from some source table to destiny table.
        return !sourceTablesIds.Any() ? null : GetShortestPath(databaseSourceId, sourceTablesIds, destinyTableId, excludedRelationships);
    }

    private static string? FindTableId(IEnumerable<DbSchemaTable> tables, string? tableName)
    {
        if (string.IsNullOrEmpty(tableName)) return null;

        return tables.FirstOrDefault(table => string.Equals(table.TableName,
            tableName, StringComparison.InvariantCultureIgnoreCase))?.TableId;
    }

    private static bool IsExcludedEdge(ICollection<TablesRelationExclusion>? exclusions, DbGraphEdge edge)
    {
        if (exclusions == null) return false;

        var relatedExclusions = exclusions
            .Where(exclusion => edge.Start.Name == exclusion.StartTableIdentifier).ToList();

        return relatedExclusions.Any(exclusion =>
                   exclusion.ExclusionMode == ExclusionMode.ExcludeAll) ||
               relatedExclusions.Any(exclusion =>
                   exclusion.ExclusionMode == ExclusionMode.ExcludeOnlyEndTable && edge.End.Name == exclusion.EndTableIdentifier) ||
               relatedExclusions.Any(exclusion =>
                   exclusion.ExclusionMode == ExclusionMode.ExcludeAllExceptEndTable && edge.End.Name != exclusion.EndTableIdentifier);
    }

    private IEnumerable<TablesRelation>? GetShortestPath(Guid databaseSourceId, ICollection<string> sourceTablesIds,
        string destinyTableId, ICollection<TablesRelationExclusion>? exclusions = null)
    {
        var graph = _graphs[databaseSourceId];
        var distances = sourceTablesIds.ToDictionary(id => id, _ => 0);
        var paths = sourceTablesIds.ToDictionary(id => id, _ => null as DbGraphEdge);
        var queue = new Queue<DbGraphVertex>(sourceTablesIds.Select(id => graph.FindVertex(id)));

        // If destiny tables is also source table, return an empty path.
        if (sourceTablesIds.Contains(destinyTableId)) return Enumerable.Empty<TablesRelation>();

        while (queue.Count > 0)
        {
            // Get table data of the table id in front of the queue.
            var vertex = queue.Dequeue();
            var distance = distances[vertex.Name];

            // Visit all vertices adjacent to this one.
            foreach (var edge in vertex.Edges)
            {
                // If this edge was excluded, ignore it.
                if (IsExcludedEdge(exclusions, edge)) continue;

                // If this edge doesn't point to a valid db graph vertex, ignore it.
                if (edge.End is not DbGraphVertex adjacent) continue;

                // If there is a path to this node, shorter than this path, ignore this path.
                if (distances.TryGetValue(adjacent.Name, out var adjacentDistance) && adjacentDistance <= distance + 1) continue;

                // Otherwise, set this path as the shortest path from source to this node.
                paths[adjacent.Name] = edge;
                distances[adjacent.Name] = distance + 1;

                // If destiny is found, stop searching.
                if (adjacent.Name == destinyTableId)
                {
                    queue.Clear();
                    break;
                }

                // Otherwise, add this table reference to the queue.
                queue.Enqueue(adjacent);
            }
        }

        // If no path was found, return null.
        if (!paths.TryGetValue(destinyTableId, out var fatherEdge)) return null;

        // Otherwise, build the path from source to destiny table.
        var path = new Stack<DbTablesRelation>();
        while (fatherEdge != null)
        {
            path.Push(fatherEdge.Data);
            fatherEdge = paths[fatherEdge.Start.Name];
        }

        // Return a copy of each relation to avoid modifications to original ones.
        return path.Select(relation => new TablesRelation
        {
            StartTableColumn = relation.StartTableColumn.Copy(),
            EndTableColumn = relation.EndTableColumn.Copy(),
        });
    }
}