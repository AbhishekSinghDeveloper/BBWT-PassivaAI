namespace BBWM.Core.Graph.Interfaces;

/// <summary>
/// Represents a directed weighted graph.
/// </summary>
/// <typeparam name="T">Type of the weight data of edges.</typeparam>
public interface IWeightedGraph<T> : IGraph where T : IComparable
{
    /// <summary>
    /// Adds the new edge.
    /// </summary>
    /// <param name="startVertexName">The name of a start vertex.</param>
    /// <param name="endVertexName">The name of a end vertex.</param>
    /// <param name="weight">The weight of a edge.</param>
    /// <returns>Created edge.</returns>
    IWeightedGraphEdge<T> AddEdge(string startVertexName, string endVertexName, T weight);
}
