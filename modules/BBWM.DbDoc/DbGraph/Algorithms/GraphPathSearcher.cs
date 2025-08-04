namespace BBWM.DbDoc.DbGraph.Algorithms;

// TODO: then to be moved to BBWM.Core/Graphs and refactored to generic class
public class GraphPathSearcher
{
    private readonly DbGraph graph;
    private int iterateCounter;

    public GraphPathSearcher(DbGraph graph) =>
        this.graph = graph;

    public IEnumerable<IDbGraphPath> SearchPaths(string startVertexName, string endVertexName,
        GraphPathSearchFilter filter = default)
    {
        if (string.IsNullOrEmpty(startVertexName))
            throw new ArgumentNullException(nameof(startVertexName));
        if (string.IsNullOrEmpty(endVertexName))
            throw new ArgumentNullException(nameof(endVertexName));

        var startVertex = graph.FindVertex(startVertexName)
            ?? throw new ArgumentException($"The vertex with name {startVertexName} is not found.", nameof(startVertexName));

        var endVertex = graph.FindVertex(endVertexName)
            ?? throw new ArgumentException($"The vertex with name {endVertexName} is not found.", nameof(endVertexName));



        // When start vertex has more connected edges it expectedly may increase a total number of recurring
        // iterations. In this case we reverse it to start searching from the end vertex and then finally reverse the
        // paths vertex sequences. For particular Org -> Users search this optimization, as it was tested, may end up
        // with x10K-x100K less iterations than without it. 
        var doReverseSearch = endVertex.Edges.Count < startVertex.Edges.Count;

        var chosenStartVertex = doReverseSearch ? endVertex : startVertex;
        var chosenEndVertex = doReverseSearch ? startVertex : endVertex;

        iterateCounter = 0;

        var searchResult = new List<DbGraphPath>();
        RecursivePathSearch(chosenStartVertex, chosenEndVertex, chosenStartVertex, new DbGraphPath(),
            filter ?? new GraphPathSearchFilter(), searchResult);

        Console.Write($"GraphPathSearcher.GetAllPaths({startVertexName} -> {endVertexName}): found paths: " +
            $"{searchResult.Count}. Search iterations: {iterateCounter}");


        if (doReverseSearch)
        {
            return searchResult.Select(x =>
            {
                var l = new DbGraphPath();

                // Reversing the edge by searching the reverse edge in the end vertex's edges list,
                // supposing that this is an undirected graph.
                l.AddRange(x.Select(y => (DbGraphEdge)y.End.Edges.First(z => z.End == y.Start)));

                // Reversing the edges sequence
                l.Reverse();
                return l;
            });
        }

        return searchResult;
    }

    private void RecursivePathSearch(
        DbGraphVertex startVertex,
        DbGraphVertex endVertext,
        DbGraphVertex currVertex,
        DbGraphPath currPath,
        GraphPathSearchFilter filter,
        List<DbGraphPath> foundPaths)
    {
        iterateCounter++;

        if (currVertex == endVertext)
        {
            var path = new DbGraphPath();
            path.AddRange(currPath);
            foundPaths.Add(path);
        }
        else if (filter.MaxDepth == 0 || currPath.Count < filter.MaxDepth)
        {
            var edges = currVertex.Edges
                .Where(x =>
                    !currPath.Any(y => x.End.Name == y.Start.Name)
                    && (filter.IncludeSelfReferences || x.End.Name != x.Start.Name))
                .GroupBy(x => x.End)
                .Select(x => x.First());

            foreach (var edge in edges)
            {
                if (filter.MaxTotal > 0 && foundPaths.Count == filter.MaxTotal)
                    break;

                currPath.Add(edge);

                RecursivePathSearch(startVertex, endVertext, (DbGraphVertex)edge.End, currPath, filter, foundPaths);

                currPath.Remove(edge);
            }
        }
    }
}
