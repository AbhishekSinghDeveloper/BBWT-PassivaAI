using BBWM.Core.Graph;
using BBWM.Core.Graph.Interfaces;

namespace BBWM.DbDoc.DbGraph;

/// <summary>
/// Implementation of an edge for a DB graph.
/// </summary>
public class DbGraphEdge : GraphEdge<DbTableInfo, DbTablesRelation>, IWeightedGraphEdge<int>
{
    public DbGraphEdge(DbGraphVertex startVertex, DbGraphVertex endVertex,
        DbTablesRelation data, int weight)
        : base(startVertex, endVertex, data) => Weight = weight;

    public int Weight { get; set; }
}
