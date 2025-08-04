using BBWM.Core.Exceptions;
using BBWM.Core.Graph;
using BBWM.Core.Graph.Interfaces;
using System.Collections.Immutable;

namespace BBWM.DbDoc.DbGraph;


/// <summary>
/// Implementation of the directed weighted graph to represent relations between tables in a DB.
/// </summary>
public class DbGraph : Graph<DbTableInfo, DbTablesRelation>, IWeightedGraph<int>
{
    public new IImmutableList<DbGraphVertex> Vertices => VerticesInternal.OfType<DbGraphVertex>().ToImmutableList();


    public new DbGraphVertex FindVertex(string vertexName) => Vertices.SingleOrDefault(x => x.Name == vertexName);

    public new DbGraphVertex AddVertex(string vertexName, DbTableInfo data)
    {
        var newVertex = new DbGraphVertex(vertexName, data);
        VerticesInternal.Add(newVertex);
        return newVertex;
    }

    public new (DbGraphEdge edge, DbGraphEdge reverseEdge)
        AddEdge(string sourceVertexName, string destinationVertexName, DbTablesRelation data) =>
        AddEdge(sourceVertexName, destinationVertexName, data, 0);

    public (DbGraphEdge edge, DbGraphEdge reverseEdge)
        AddEdge(string startVertexName, string endVertexName, DbTablesRelation data, int weight)
    {
        var startVertex = FindVertex(startVertexName);
        var endVertex = FindVertex(endVertexName);

        if (startVertex is null)
            throw new DataException($"The graph does not contain a start vertex with the name {startVertexName}.");

        if (endVertex is null)
            throw new DataException($"The graph does not contain an end vertex with the name {endVertexName}.");

        var edge = startVertex.AddEdge(endVertex, data, weight);
        edge.Weight = weight;

        var reverseEdge = endVertex.AddEdge(startVertex,
            edge.Data is null
                ? null
                : new DbTablesRelation
                {
                    StartTableColumn = edge.Data.EndTableColumn,
                    EndTableColumn = edge.Data.StartTableColumn,
                    IsRequired = edge.Data.IsRequired
                },
            weight);

        return (edge, reverseEdge);
    }

    public IWeightedGraphEdge<int> AddEdge(string sourceVertexName, string destinationVertexName, int weight) =>
        AddEdge(sourceVertexName, destinationVertexName, null, weight).edge;
}