using BBWM.Core.Graph.Interfaces;

namespace BBWM.DbDoc.DbGraph.Algorithms;

// TODO: then to be moved to BBWM.Core/Graphs and refactored to generic class

/// <summary>
/// Dijkstra's algorithm for finding the shortest path in a graph.
/// </summary>
public class GraphDijkstra
{
    private readonly DbGraph _graph;
    private readonly DbGraphVertex _startVertex;
    private List<GraphVertexInfo> _infos;


    public GraphDijkstra(DbGraph graph, string startVertexName)
    {
        if (string.IsNullOrEmpty(startVertexName))
            throw new ArgumentNullException(nameof(startVertexName));

        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        _startVertex = graph.FindVertex(startVertexName);

        if (_startVertex == null)
            throw new ArgumentException($"The vertex with name {startVertexName} is not found.", nameof(startVertexName));

        Calculate();
    }


    public IDbGraphPath GetPathTo(string endVertexName)
    {
        if (endVertexName == null)
            throw new ArgumentNullException(nameof(endVertexName));

        var endVertex = _graph.FindVertex(endVertexName)
            ?? throw new ArgumentException($"The vertex with name {endVertexName} is not found.", nameof(endVertexName));

        var result = new DbGraphPath();
        while (_startVertex != endVertex)
        {
            var vertexInfo = GetVertexInfo(endVertex)
                ?? throw new InvalidOperationException($"Information about vertex with name {endVertex.Name} is not found.");
            var edge = vertexInfo.PreviousVertex.Edges.FirstOrDefault(x => x.End.Name == vertexInfo.Vertex.Name);

            // No path from root table
            if (edge is not null)
            {
                result.Add(edge);
                endVertex = vertexInfo.PreviousVertex;
            }
        }

        return result;
    }

    public DbGraphVertex GetClosestVertex(IEnumerable<string> verticesNames)
    {
        var relatedVerticesInfos = _infos
            .Where(x => x.EdgesWeightSum.HasValue && x.Visited && verticesNames.Contains(x.Vertex.Name))
            .ToList();

        return relatedVerticesInfos.Any()
            ? relatedVerticesInfos
                .OrderBy(x => x.EdgesWeightSum)
                .First()
                .Vertex
            : null;
    }

    public IEnumerable<DbGraphVertex> GetReachableVertices() =>
        _infos.Where(x => x.Visited && x.EdgesWeightSum != 0).Select(x => x.Vertex).ToList();


    private void Calculate()
    {
        _infos = _graph.Vertices.Select(x => new GraphVertexInfo(x)).ToList();

        var firstVertexInfo = GetVertexInfo(_startVertex);
        firstVertexInfo.EdgesWeightSum = 0;

        do
        {
            // Find unvisited vertex with min sum
            var current = _infos
                .Where(x => !x.Visited && x.EdgesWeightSum.HasValue)
                .OrderBy(x => x.EdgesWeightSum).FirstOrDefault();

            if (current is null) break;
            SetSumToNextVertex(current);
        } while (true);
    }

    private void SetSumToNextVertex(GraphVertexInfo info)
    {
        info.Visited = true;
        foreach (var e in info.Vertex.Edges)
        {
            var nextInfo = GetVertexInfo(e.End);
            var sum = info.EdgesWeightSum + e.Weight;

            if (!nextInfo.EdgesWeightSum.HasValue || sum < nextInfo.EdgesWeightSum)
            {
                nextInfo.EdgesWeightSum = sum;
                nextInfo.PreviousVertex = info.Vertex;
            }
        }
    }

    private GraphVertexInfo GetVertexInfo(IGraphVertex v) =>
        _infos.SingleOrDefault(x => x.Vertex.Name.Equals(v.Name));


    /// <summary>
    /// Vertex metadata for algorithm usage only.
    /// </summary>
    private class GraphVertexInfo
    {
        public GraphVertexInfo(DbGraphVertex vertex) => Vertex = vertex;


        public DbGraphVertex Vertex { get; }

        public bool Visited { get; set; }

        public int? EdgesWeightSum { get; set; }

        public DbGraphVertex PreviousVertex { get; set; }
    }
}