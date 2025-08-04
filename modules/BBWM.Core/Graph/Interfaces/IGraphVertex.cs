using System.Collections.Immutable;

namespace BBWM.Core.Graph.Interfaces;

/// <summary>
/// Represents a vertex of a graph.
/// </summary>
public interface IGraphVertex
{
    /// <summary>
    /// The name of a vertex.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The list of outgoing edges.
    /// </summary>
    IImmutableList<IGraphEdge> Edges { get; }

    /// <summary>
    /// Adds the new edge for a vertex.
    /// </summary>
    /// <param name="endVertex">The end vertex.</param>
    /// <returns>Created edge.</returns>
    IGraphEdge AddEdge(IGraphVertex endVertex);
}


/// <summary>
/// Represents a vertex of a graph that able to keep some data.
/// </summary>
/// <typeparam name="TVertexData">Type of the data inside vertex.</typeparam>
/// <typeparam name="TEdgeData">Type of the data inside edge.</typeparam>
public interface IGraphVertex<TVertexData, TEdgeData> : IGraphVertex
    where TVertexData : class
    where TEdgeData : class
{
    /// <summary>
    /// The list of outgoing edges.
    /// </summary>
    new IImmutableList<IGraphEdge<TVertexData, TEdgeData>> Edges { get; }

    /// <summary>
    /// The data of a vertex.
    /// </summary>
    TVertexData Data { get; set; }

    /// <summary>
    /// Adds the new edge for a vertex.
    /// </summary>
    /// <param name="endVertex">The end vertex.</param>
    /// <param name="data">The data of the edge.</param>
    /// <returns>Created edge.</returns>
    IGraphEdge<TVertexData, TEdgeData> AddEdge(IGraphVertex<TVertexData, TEdgeData> endVertex, TEdgeData data);
}