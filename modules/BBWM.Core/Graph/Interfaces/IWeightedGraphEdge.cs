namespace BBWM.Core.Graph.Interfaces;

/// <summary>
/// Represents an edge of a directed weighted graph.
/// </summary>
/// <typeparam name="T">Type of the weight data.</typeparam>
public interface IWeightedGraphEdge<T> : IGraphEdge where T : IComparable
{
    /// <summary>
    /// The weight of an edge.
    /// </summary>
    T Weight { get; set; }
}
