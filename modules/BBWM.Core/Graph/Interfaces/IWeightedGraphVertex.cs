using System.Collections.Immutable;

namespace BBWM.Core.Graph.Interfaces;

/// <summary>
/// Represents a vertex of a directed weighted graph.
/// </summary>
/// <typeparam name="T">Type of the weight data of an edge.</typeparam>
public interface IWeightedGraphVertex<T> : IGraphVertex where T : IComparable
{
    /// <summary>
    /// The list of outgoing edges.
    /// </summary>
    new IImmutableList<IWeightedGraphEdge<T>> Edges { get; }

    /// <summary>
    /// Adds the new edge for a vertex.
    /// </summary>
    /// <param name="endVertex">The end vertex.</param>
    /// <param name="weight">The weight of an edge.</param>
    /// <returns>Created edge.</returns>
    IWeightedGraphEdge<T> AddEdge(IWeightedGraphVertex<T> endVertex, int weight);
}
