using BBWM.Core.Exceptions;
using BBWM.Core.Graph.Interfaces;
using System.Collections.Immutable;

namespace BBWM.Core.Graph;

/// <summary>
/// Implementation of a directed graph.
/// </summary>
public class Graph : IGraph
{
    protected IList<IGraphVertex> VerticesInternal = new List<IGraphVertex>();


    public IImmutableList<IGraphVertex> Vertices => VerticesInternal.ToImmutableList();


    public IGraphVertex FindVertex(string vertexName) => Vertices.SingleOrDefault(x => x.Name.Equals(vertexName));

    public IGraphVertex AddVertex(string vertexName)
    {
        var newVertex = new GraphVertex(vertexName);
        VerticesInternal.Add(newVertex);
        return newVertex;
    }

    public IGraphEdge AddEdge(string startVertexName, string endVertexName)
    {
        var startVertex = FindVertex(startVertexName);
        var endVertex = FindVertex(endVertexName);

        if (startVertex is null)
            throw new DataException($"The graph does not contain a start vertex with the name {startVertexName}.");

        if (endVertex is null)
            throw new DataException($"The graph does not contain a end vertex with the name {endVertexName}.");

        return startVertex.AddEdge(endVertex);
    }
}


/// <summary>
/// Implementation of a directed graph that able to keep some data in vertices and edges.
/// </summary>
public class Graph<TVertexData, TEdgeData> : Graph, IGraph<TVertexData, TEdgeData>
    where TVertexData : class
    where TEdgeData : class
{
    public new IImmutableList<IGraphVertex<TVertexData, TEdgeData>> Vertices => VerticesInternal.OfType<IGraphVertex<TVertexData, TEdgeData>>().ToImmutableList();


    public new IGraphVertex<TVertexData, TEdgeData> FindVertex(string vertexName) => Vertices.SingleOrDefault(x => x.Name == vertexName);

    public IGraphVertex<TVertexData, TEdgeData> AddVertex(string vertexName, TVertexData data)
    {
        var newVertex = new GraphVertex<TVertexData, TEdgeData>(vertexName, data);
        VerticesInternal.Add(newVertex);
        return newVertex;
    }

    public IGraphEdge<TVertexData, TEdgeData> AddEdge(string startVertexName, string endVertexName, TEdgeData data)
    {
        var startVertex = FindVertex(startVertexName);
        var endVertex = FindVertex(endVertexName);

        if (startVertex is null)
            throw new DataException($"The graph does not contain a start vertex with the name {startVertexName}.");

        if (endVertex is null)
            throw new DataException($"The graph does not contain an end vertex with the name {endVertexName}.");

        return startVertex.AddEdge(endVertex, data);
    }
}

