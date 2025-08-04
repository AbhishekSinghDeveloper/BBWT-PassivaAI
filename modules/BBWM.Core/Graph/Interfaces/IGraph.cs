using System.Collections.Immutable;

namespace BBWM.Core.Graph.Interfaces;

/// <summary>
/// Represents a directed graph.
/// </summary>
public interface IGraph
{
    /// <summary>
    /// The list of vertices of a graph.
    /// </summary>
    IImmutableList<IGraphVertex> Vertices { get; }

    /// <summary>
    /// Searches for a vertex by the name.
    /// </summary>
    /// <param name="vertexName">The name of a vertex.</param>
    IGraphVertex FindVertex(string vertexName);

    /// <summary>
    /// Adds the new vertex.
    /// </summary>
    /// <param name="vertexName">The name of a vertex.</param>
    /// <returns>Created vertex.</returns>
    IGraphVertex AddVertex(string vertexName);

    /// <summary>
    /// Adds the new edge.
    /// </summary>
    /// <param name="startVertexName">The name of a start vertex.</param>
    /// <param name="endVertexName">The name of a end vertex.</param>
    /// <returns>Created edge.</returns>
    IGraphEdge AddEdge(string startVertexName, string endVertexName);
}


/// <summary>
/// Represents a directed graph that able to keep some data in vertices and edges.
/// </summary>
/// <typeparam name="TVertexData">Type of the data inside vertices.</typeparam>
/// <typeparam name="TEdgeData">Type of the data inside edges.</typeparam>
public interface IGraph<TVertexData, TEdgeData> : IGraph
    where TVertexData : class
    where TEdgeData : class
{
    /// <summary>
    /// The list of vertices of a graph.
    /// </summary>
    new IImmutableList<IGraphVertex<TVertexData, TEdgeData>> Vertices { get; }

    /// <summary>
    /// Searches for the vertex by the name.
    /// </summary>
    /// <param name="vertexName">The name of a vertex.</param>
    new IGraphVertex<TVertexData, TEdgeData> FindVertex(string vertexName);

    /// <summary>
    /// Adds the new vertex.
    /// </summary>
    /// <param name="vertexName">The name of a vertex.</param>
    /// <param name="data">The data of a vertex.</param>
    /// <returns>Created vertex.</returns>
    IGraphVertex<TVertexData, TEdgeData> AddVertex(string vertexName, TVertexData data);

    /// <summary>
    /// Adds the new edge.
    /// </summary>
    /// <param name="startVertexName">The name of a start vertex.</param>
    /// <param name="endVertexName">The name of a end vertex.</param>
    /// <param name="data">The data of an edge.</param>
    /// <returns>Created edge.</returns>
    IGraphEdge<TVertexData, TEdgeData> AddEdge(string startVertexName, string endVertexName, TEdgeData data);
}