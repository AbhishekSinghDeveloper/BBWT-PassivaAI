using BBWM.Core.Graph.Interfaces;
using System.Collections.Immutable;

namespace BBWM.Core.Graph;

/// <summary>
/// Implementation of a vertex of a graph.
/// </summary>
public class GraphVertex : IGraphVertex
{
    protected IList<IGraphEdge> EdgesInternal = new List<IGraphEdge>();


    public GraphVertex(string vertexName) => Name = vertexName;


    public string Name { get; }

    public IImmutableList<IGraphEdge> Edges => EdgesInternal.ToImmutableList();


    public IGraphEdge AddEdge(IGraphVertex endVertex)
    {
        var newEdge = new GraphEdge(this, endVertex);
        EdgesInternal.Add(newEdge);
        return newEdge;
    }
}


/// <summary>
/// Implementation of a vertex of a graph that able to keep some data.
/// </summary>
public class GraphVertex<TVertexData, TEdgeData> : GraphVertex, IGraphVertex<TVertexData, TEdgeData>
    where TVertexData : class
    where TEdgeData : class
{
    public GraphVertex(string vertexName, TVertexData data) : base(vertexName) => Data = data;


    public new IImmutableList<IGraphEdge<TVertexData, TEdgeData>> Edges => EdgesInternal.OfType<IGraphEdge<TVertexData, TEdgeData>>().ToImmutableList();

    public TVertexData Data { get; set; }


    public IGraphEdge<TVertexData, TEdgeData> AddEdge(IGraphVertex<TVertexData, TEdgeData> endVertex, TEdgeData data)
    {
        var newEdge = new GraphEdge<TVertexData, TEdgeData>(this, endVertex, data);
        EdgesInternal.Add(newEdge);
        return newEdge;
    }

    public override string ToString() => Name;
}
