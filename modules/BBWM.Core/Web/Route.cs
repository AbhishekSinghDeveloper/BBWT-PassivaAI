namespace BBWM.Core.Web;

public class Route
{
    public Route(string path, string title)
    {
        Path = path;
        Title = title;
    }

    public string Path { get; set; }

    public string Title { get; set; }
}

public class RouteBuilder
{
    private readonly string _basePath;
    private readonly RouteBuilder _parent;

    public RouteBuilder(string basePath, RouteBuilder parent = null)
    {
        _basePath = basePath;
        _parent = parent;
    }

    public Route Build(string path, string title)
    {
        string combinedPath = CombinePaths(_basePath, path);
        return _parent is not null ? _parent.Build(combinedPath, title) : new Route(combinedPath, title);
    }

    private static string CombinePaths(params string[] pathParts)
        => $"/{string.Join('/', pathParts.Select(x => x.Trim('/', '\\')))}";
}
