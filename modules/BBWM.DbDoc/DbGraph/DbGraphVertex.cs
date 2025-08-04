using BBWM.Core.Graph;
using BBWM.Core.Graph.Interfaces;
using System.Collections.Immutable;

namespace BBWM.DbDoc.DbGraph;

/// <summary>
/// Implementation of a vertex for a DB graph.
/// </summary>
public class DbGraphVertex : GraphVertex<DbTableInfo, DbTablesRelation>, IWeightedGraphVertex<int>
{
    public DbGraphVertex(string vertexName, DbTableInfo data) : base(vertexName, data)
    {
    }

    public new IImmutableList<DbGraphEdge> Edges => EdgesInternal.OfType<DbGraphEdge>().ToImmutableList();

    IImmutableList<IWeightedGraphEdge<int>> IWeightedGraphVertex<int>.Edges => EdgesInternal.OfType<IWeightedGraphEdge<int>>().ToImmutableList();

    public DbGraphEdge AddEdge(DbGraphVertex endVertex, DbTablesRelation data) =>
        AddEdge(endVertex, data, 0);

    public IWeightedGraphEdge<int> AddEdge(IWeightedGraphVertex<int> endVertex, int weight) =>
        AddEdge(endVertex as DbGraphVertex, null, weight);

    public DbGraphEdge AddEdge(DbGraphVertex endVertex, DbTablesRelation data, int weight)
    {
        var newEdge = new DbGraphEdge(this, endVertex, data, weight);
        EdgesInternal.Add(newEdge);
        return newEdge;
    }
}
