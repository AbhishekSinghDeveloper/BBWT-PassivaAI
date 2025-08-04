using BBF.Reporting.QueryBuilder.Model;

namespace BBF.Reporting.QueryBuilder.Interfaces;

public interface IRqbQueryGraphService
{
    IEnumerable<TablesRelation>? GetShortestPath(IEnumerable<string> sourceTablesNames,
        string destinyTableName, IEnumerable<TablesRelationExclusion>? exclusions = null);
}