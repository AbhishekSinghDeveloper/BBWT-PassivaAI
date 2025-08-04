using BBWM.Core.Graph.Interfaces;

namespace BBWM.Core.Graph;

/// <summary>
/// Implementation of an edge of a graph.
/// </summary>
public class GraphEdge : IGraphEdge
{
    public GraphEdge(IGraphVertex startVertex, IGraphVertex endVertex)
    {
        Start = startVertex;
        End = endVertex;
    }

    public IGraphVertex Start { get; }

    public IGraphVertex End { get; }
}



/// <summary>
/// Implementation of an edge of a graph that able to keep some data.
/// </summary>
public class GraphEdge<TVertexData, TEdgeData> : GraphEdge, IGraphEdge<TVertexData, TEdgeData>
    where TVertexData : class
    where TEdgeData : class
{
    public GraphEdge(IGraphVertex<TVertexData, TEdgeData> startVertex, IGraphVertex<TVertexData, TEdgeData> endVertex, TEdgeData data)
        : base(startVertex, endVertex) => Data = data;


    public new IGraphVertex<TVertexData, TEdgeData> Start => base.Start as IGraphVertex<TVertexData, TEdgeData>;

    public new IGraphVertex<TVertexData, TEdgeData> End => base.End as IGraphVertex<TVertexData, TEdgeData>;

    public TEdgeData Data { get; set; }
}
