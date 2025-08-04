using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using BBF.Reporting.QueryBuilder.Model.ParserModels;

namespace BBF.Reporting.QueryBuilder.Services.RbqMySqlQueryParser;

public static class RqbQueryMySqlParserExtensions
{
    /// Return the children of this tree node.
    public static IEnumerable<IParseTree> GetChildren(this IParseTree tree)
        => Enumerable.Range(0, tree.ChildCount).Select(tree.GetChild);

    public static IEnumerable<IParseTree> GetNonErrorChildren(this IParseTree tree)
        => tree.GetChildren().Where(child => child is not ErrorNodeImpl);

    /// Return the children of T type of this node.
    public static IEnumerable<T> GetChildrenOfType<T>(this IParseTree tree) where T : class, IParseTree
        => tree.GetChildren().OfType<T>();

    /// Return the descendent of T type of this node.
    public static IEnumerable<T> GetDescendentsOfType<T>(this IParseTree tree) where T : class, IParseTree
    {
        if (tree.ChildCount == 0) yield break;

        // Minor optimization to avoid recursion on subtrees that are path graphs.
        // Loops while the current node has a single child (no recursion needed).
        while (tree.ChildCount == 1)
        {
            // If this child is of T type, return it and stop searching.
            if (tree.GetChild(0) is T node)
            {
                yield return node;
                yield break;
            }

            // Otherwise, move to this node children and continue searching.
            tree = tree.GetChild(0);
        }

        // Return the children of T type of this node.
        foreach (var child in GetChildrenOfType<T>(tree))
            yield return child;

        // Search for the descendents of T type, among the descendents of remaining children.
        foreach (var child in tree.GetChildren().Where(child => child is not T))
        foreach (var descendant in GetDescendentsOfType<T>(child))
            yield return descendant;
    }

    /// Return the first child of T type if exists.
    public static T? GetChildOfType<T>(this IParseTree tree) where T : class, IParseTree
        => tree.GetChildrenOfType<T>().FirstOrDefault();

    /// Return this token bounds.
    private static Range? GetRange(IToken startToken, IToken endToken)
    {
        var start = startToken.StartIndex;
        var end = endToken.StopIndex + 1;
        if (start < 0 || end < 0 || start >= end) return null;
        return new Range(start, end);
    }

    public static Range? GetRange(this ParserRuleContext context)
        => GetRange(context.Start, context.Stop);

    public static Range? GetRange(this TerminalNodeImpl terminal)
        => GetRange(terminal.Symbol, terminal.Symbol);

    public static Range? GetRange(this IParseTree tree) => tree switch
    {
        TerminalNodeImpl terminal => terminal.GetRange(),
        ParserRuleContext context => context.GetRange(),
        _ => null
    };

    public static Range? GetRangeUpTo(this IParseTree startNode, IParseTree endNode)
    {
        var start = startNode.GetRange()?.Start.Value;
        var end = endNode.GetRange()?.End.Value;
        if (start == null || end == null || start > end) return null;
        return new Range(start.Value, end.Value);
    }

    /// Convert a token into the corresponding sql parser object.
    private static T? GetSqlParserObject<T>(Range? range, string code) where T : SqlParserObject, new()
        => range?.End.Value <= code.Length ? new T { Range = range.Value, Sql = code[range.Value] } : null;

    public static T? GetSqlParserObject<T>(this IParseTree tree, string code) where T : SqlParserObject, new()
        => GetSqlParserObject<T>(tree.GetRange(), code);

    public static T? GetSqlParserObjectUpTo<T>(this IParseTree startNode, IParseTree endNode, string code) where T : SqlParserObject, new()
        => GetSqlParserObject<T>(startNode.GetRangeUpTo(endNode), code);
}