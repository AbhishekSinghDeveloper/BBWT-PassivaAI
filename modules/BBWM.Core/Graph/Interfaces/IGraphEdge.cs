namespace BBWM.Core.Graph.Interfaces;

/// <summary>
/// Represents an edge of a graph.
/// </summary>
public interface IGraphEdge
{
    /// <summary>
    /// The start vertex.
    /// </summary>
    IGraphVertex Start { get; }

    /// <summary>
    /// The end vertex.
    /// </summary>
    IGraphVertex End { get; }
}

/// <summary>
/// Represents an edge of a graph that able to keep some data.
/// </summary>
/// <typeparam name="TVertexData">Type of the data inside vertex.</typeparam>
/// <typeparam name="TEdgeData">Type of the data inside edge.</typeparam>
public interface IGraphEdge<TVertexData, TEdgeData> : IGraphEdge
    where TVertexData : class
    where TEdgeData : class
{
    /// <summary>
    /// The start vertex.
    /// </summary>
    new IGraphVertex<TVertexData, TEdgeData> Start { get; }

    /// <summary>
    /// The end vertex.
    /// </summary>
    new IGraphVertex<TVertexData, TEdgeData> End { get; }

    /// <summary>
    /// The data of an edge.
    /// </summary>
    TEdgeData Data { get; set; }
}