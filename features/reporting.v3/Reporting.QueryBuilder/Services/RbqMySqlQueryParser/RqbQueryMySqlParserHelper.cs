using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using BBF.Reporting.Core.Enums;
using BBF.Reporting.QueryBuilder.Enums;
using BBF.Reporting.QueryBuilder.Interfaces.RbqMySqlQueryParser;
using BBF.Reporting.QueryBuilder.Model.ParserModels;
using static MySqlParser;

namespace BBF.Reporting.QueryBuilder.Services.RbqMySqlQueryParser;

public class RqbQueryMySqlParserHelper : IRqbQueryMySqlParserHelper
{
    private delegate ControlFlow Visitor(IParseTree node, string code, out SqlParserObject? conversion);

    private delegate ControlFlow Visitor<in TNode, TObject>(TNode node, string code, out TObject? conversion)
        where TNode : IParseTree
        where TObject : SqlParserObject;

    private record VisitorRule(Type ParentContext, Visitor Visitor);

    private readonly ImmutableDictionary<Type, ImmutableList<VisitorRule>> _contextVisitors;
    private readonly ImmutableDictionary<string, ImmutableList<VisitorRule>> _terminalVisitors;

    public RqbQueryMySqlParserHelper()
    {
        _contextVisitors = GetContextVisitors();
        _terminalVisitors = GetTerminalVisitors();
    }

    /// Defined mappings between ANTLR terminal models and SqlParserObjects.
    private static ImmutableDictionary<string, ImmutableList<VisitorRule>> GetTerminalVisitors()
    {
        return new Dictionary<string, List<VisitorRule>>
            {
                ["*"] = new() { GetBasicVisitorRule<TerminalNodeImpl, WildcardExpression, SelectClause>(ControlFlow.Break) }
            }
            .ToImmutableDictionary(pair => pair.Key, pair => pair.Value.ToImmutableList(),
                StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.IgnoreCase));
    }

    /// Defined mappings between ANTLR context models and SqlParserObjects.
    private ImmutableDictionary<Type, ImmutableList<VisitorRule>> GetContextVisitors()
    {
        return new Dictionary<Type, List<VisitorRule>>
            {
                [typeof(QuerySpecificationContext)] = new() { GetVisitorRule<QuerySpecificationContext, QuerySpecification>(VisitQuerySpecification) },
                [typeof(QueryExpressionBodyContext)] = new() { GetVisitorRule<QueryExpressionBodyContext, QueryBinaryExpression>(VisitQueryBinaryExpression) },
                [typeof(SelectItemContext)] = new() { GetVisitorRule<SelectItemContext, SelectExpression>(VisitSelectExpression) },
                [typeof(ColumnRefContext)] = new() { GetVisitorRule<ColumnRefContext, ColumnReference>(VisitColumnReference) },
                [typeof(TableWildContext)] = new() { GetVisitorRule<TableWildContext, TableWildcardExpression>(VisitTableWildcardExpression) },
                [typeof(TableReferenceContext)] = new() { GetVisitorRule<TableReferenceContext, TableJoinExpression>(VisitTableJoinExpression) },
                [typeof(SingleTableContext)] = new() { GetVisitorRule<SingleTableContext, TableReference>(VisitTableReference) },
                [typeof(DerivedTableContext)] = new() { GetVisitorRule<DerivedTableContext, DerivedTableExpression>(VisitDerivedTableDeclaration) },
                [typeof(PrimaryExprCompareContext)] = new() { GetVisitorRule<PrimaryExprCompareContext, ComparisonExpression>(VisitComparisonExpression) },
                [typeof(LimitClauseContext)] = new() { GetVisitorRule<LimitClauseContext, LimitClause>(VisitLimitClause) },

                [typeof(SelectStatementContext)] = new() { GetBasicVisitorRule<SelectStatementContext, SelectStatement>(ControlFlow.Continue) },
                [typeof(SimpleExprFunctionContext)] = new() { GetBasicVisitorRule<SimpleExprFunctionContext, FunctionCall>(ControlFlow.Continue) },
                [typeof(FromClauseContext)] = new() { GetBasicVisitorRule<FromClauseContext, FromClause>(ControlFlow.Continue) },
                [typeof(OrderClauseContext)] = new() { GetBasicVisitorRule<OrderClauseContext, OrderByClause>(ControlFlow.Continue) },
                [typeof(GroupByClauseContext)] = new() { GetBasicVisitorRule<GroupByClauseContext, GroupByClause>(ControlFlow.Continue) },
                [typeof(WhereClauseContext)] = new() { GetBasicVisitorRule<WhereClauseContext, WhereClause>(ControlFlow.Continue) },
                [typeof(ExprOrContext)] = new() { GetBasicVisitorRule<ExprOrContext, OrExpression>(ControlFlow.Continue) },
                [typeof(ExprAndContext)] = new() { GetBasicVisitorRule<ExprAndContext, AndExpression>(ControlFlow.Continue) },
                [typeof(LimitOptionContext)] = new() { GetBasicVisitorRule<LimitOptionContext, NumericLiteral>(ControlFlow.Break) },
                [typeof(NumLiteralContext)] = new() { GetBasicVisitorRule<NumLiteralContext, NumericLiteral>(ControlFlow.Break) },
                [typeof(TextLiteralContext)] = new() { GetBasicVisitorRule<TextLiteralContext, StringLiteral>(ControlFlow.Break) },
                [typeof(BoolLiteralContext)] = new() { GetBasicVisitorRule<BoolLiteralContext, BooleanLiteral>(ControlFlow.Break) },
                [typeof(TemporalLiteralContext)] = new() { GetBasicVisitorRule<TemporalLiteralContext, TemporalLiteral>(ControlFlow.Break) },
                [typeof(NullLiteralContext)] = new() { GetBasicVisitorRule<NullLiteralContext, NullLiteral>(ControlFlow.Break) },
                [typeof(EmittedVariableContext)] = new() { GetBasicVisitorRule<EmittedVariableContext, EmittedVariableReference>(ControlFlow.Break) },
                [typeof(ContextVariableContext)] = new() { GetBasicVisitorRule<ContextVariableContext, ContextVariableReference>(ControlFlow.Break) },

                // An order expression context can only be parsed as order by expression if parent context is an order by clause.
                [typeof(OrderExpressionContext)] = new()
                {
                    GetVisitorRule<OrderExpressionContext, OrderByExpression, OrderByClause>(VisitOrderByExpression)
                },

                // A predicate context can only be parsed as in expression if parent context is a where clause or another where expression.
                [typeof(PredicateContext)] = new()
                {
                    GetVisitorRule<PredicateContext, InExpression, WhereClause>(VisitInExpression),
                    GetVisitorRule<PredicateContext, InExpression, WhereExpression>(VisitInExpression)
                },

                // An expression list context can only be parsed as list expression if parent context is an in expression.
                [typeof(ExprListContext)] = new() { GetBasicVisitorRule<ExprListContext, ListExpression, InExpression>(ControlFlow.Continue) }
            }
            .ToImmutableDictionary(pair => pair.Key, pair => pair.Value.ToImmutableList());
    }

    // Transform a visitor function that consider parent context into a base visitor delegate.
    private static VisitorRule GetVisitorRule<TNode, TObject, TParent>(Visitor<TNode, TObject> visitor)
        where TNode : IParseTree
        where TObject : SqlParserObject
        where TParent : SqlParserObject
    {
        var ruleContext = typeof(TParent);
        var ruleVisitor = ConvertToBaseVisitor(visitor);
        return new VisitorRule(ruleContext, ruleVisitor);
    }

    // Transform a visitor function that ignores parent context into a base visitor delegate.
    private static VisitorRule GetVisitorRule<TNode, TObject>(Visitor<TNode, TObject> visitor)
        where TNode : IParseTree
        where TObject : SqlParserObject
        => GetVisitorRule<TNode, TObject, SqlParserObject>(visitor);

    // Makes a naive transformation from an ANTLR model to a SqlParserObject, considering parent context.
    private static VisitorRule GetBasicVisitorRule<TNode, TObject, TParent>(ControlFlow controlFlow)
        where TNode : IParseTree
        where TObject : SqlParserObject, new()
        where TParent : SqlParserObject
    {
        return GetVisitorRule<TNode, TObject, TParent>(BasicVisitor);

        ControlFlow BasicVisitor(TNode node, string code, out TObject? conversion)
        {
            conversion = node.GetSqlParserObject<TObject>(code);
            return conversion == null ? ControlFlow.Break : controlFlow;
        }
    }

    // Makes a naive transformation from an ANTLR model to a SqlParserObject, ignoring parent context.
    private static VisitorRule GetBasicVisitorRule<TNode, TObject>(ControlFlow controlFlow)
        where TNode : IParseTree
        where TObject : SqlParserObject, new()
        => GetBasicVisitorRule<TNode, TObject, SqlParserObject>(controlFlow);

    /// Converts a generic visitor into a base type visitor.
    private static Visitor ConvertToBaseVisitor<TNode, TObject>(Visitor<TNode, TObject> visitor)
        where TNode : IParseTree
        where TObject : SqlParserObject
    {
        return Visitor;

        ControlFlow Visitor(IParseTree tree, string code, out SqlParserObject? parserObject)
        {
            // Set default value to out parameter.
            parserObject = null;

            // If given parse tree is not of TNode type, break the visit due to conversion error.
            if (tree is not TNode node) return ControlFlow.Break;

            // Otherwise, visit the corresponding node using the given visitor.
            var flow = visitor(node, code, out var conversion);

            // Convert the parsed object to its base type and set is as out parameter.
            parserObject = conversion;

            // Return the obtained control flow instruction.
            return flow;
        }
    }

    /// Get the corresponding visitor given context type and parent object type.
    private Visitor? GetVisitor(IParseTree tree, SqlParserObject? parent)
    {
        var parentType = parent?.GetType() ?? typeof(SqlParserObject);

        // Get visitor rules corresponding to this tree node type.
        var visitorRules = tree switch
        {
            ParserRuleContext context when _contextVisitors.TryGetValue(context.GetType(), out var rules) => rules,
            TerminalNodeImpl terminal when _terminalVisitors.TryGetValue(terminal.GetText(), out var rules) => rules,
            _ => null
        };

        // Otherwise, search for the first visitor rule corresponding to this node type and parent context.
        var rule = visitorRules?.FirstOrDefault(rule => rule.ParentContext.IsAssignableFrom(parentType));

        return rule?.Visitor;
    }

    /// Parses a MySQL code and returns the corresponding SqlParserObject tree.
    public List<SqlParserObject> Parse(string code)
    {
        if (string.IsNullOrEmpty(code)) return new List<SqlParserObject>();

        // Parse the SQL query.
        var node = Parse(code, 80200, "ANSI_QUOTES");

        // Convert the three and return.
        return node != null ? Visit(node, null, code) : new List<SqlParserObject>();
    }

    /// Parses a MySQL code and returns the corresponding ANTLR tree.
    private static IParseTree? Parse(string input, int version, string modes)
    {
        var stream = new AntlrInputStream(input);
        var lexer = new MySqlLexer(stream) { ServerVersion = version };
        lexer.SqlModeFromString(modes);
        lexer.RemoveErrorListeners();

        var tokens = new CommonTokenStream(lexer);
        var parser = new MySqlParser(tokens) { ServerVersion = lexer.ServerVersion, SqlModes = lexer.SqlModes };
        parser.RemoveErrorListeners();

        return parser.queries();
    }

    /// Converts an ANTLR tree into a SqlParserObject tree using conversion functions.
    private List<SqlParserObject> Visit(IParseTree node, SqlParserObject? parent, string code)
    {
        while (node != null)
        {
            // Ignore error nodes.
            if (node is ErrorNodeImpl) return new List<SqlParserObject>();

            // Default action is to not convert node and just continue.
            SqlParserObject? parserObject = null;
            var controlFlow = ControlFlow.Continue;

            // Try to get the visitor corresponding to this node type and parent object type.
            var visitor = GetVisitor(node, parent);

            // If visitor exists, update default action with visitor result.
            if (visitor != null) controlFlow = visitor(node, code, out parserObject);

            // If visitor is not null, convert the node to a sql parser object if possible.
            switch (controlFlow)
            {
                case ControlFlow.Continue when parserObject is null:
                {
                    // If it's indicated to continue and this node has more than one child, visit the children recursively.
                    if (node.ChildCount > 1) return VisitChildren(node, parent, code);

                    // Otherwise, just move to the only child of this node, to avoid recursion.
                    node = node.GetChild(0);
                    continue;
                }
                case ControlFlow.Continue:
                {
                    parserObject.Parent = parent;

                    // Visit and get the children of this object recursively.
                    parserObject.Children.AddRange(VisitChildren(node, parserObject, code));

                    return new List<SqlParserObject> { parserObject };
                }
                case ControlFlow.Break when parserObject is null:
                {
                    return new List<SqlParserObject>();
                }
                case ControlFlow.Break:
                {
                    parserObject.Parent = parent;
                    return new List<SqlParserObject> { parserObject };
                }
                default: return new List<SqlParserObject>();
            }
        }

        // Return an empty array if no convertible node was found.
        return new List<SqlParserObject>();
    }

    private List<SqlParserObject> VisitChildren(IParseTree node, SqlParserObject? parent, string code)
        => node.GetNonErrorChildren().SelectMany(child => Visit(child, parent, code)).ToList();

    /// Convert a QueryExpressionBodyContext in the corresponding QueryBinaryExpression object if applies.
    /// This conversion is complete (breaks the visit).
    private ControlFlow VisitQueryBinaryExpression(QueryExpressionBodyContext context, string code, out QueryBinaryExpression? conversion)
    {
        // Set default value to out parameter.
        conversion = null;

        // If the query expression doesn't have query primary context or other query expressions, continue the visit
        // (it is a parenthesis context expression or a single query specification).
        if (context.GetChildOfType<QueryPrimaryContext>() is not { } queryPrimaryContext ||
            context.GetChildrenOfType<QueryExpressionBodyContext>().ToList() is not { Count: > 0 } queryExpressionContextsList)
            return ControlFlow.Continue;

        // Build query specification corresponding to the left side of the set operation.
        ParserRuleContext leftContext = queryPrimaryContext;
        var leftExpression = Visit(leftContext, null, code).FirstOrDefault();
        if (leftExpression is null) return ControlFlow.Break;

        // Build the query binary expression corresponding to each query expression body context.
        foreach (var queryExpressionContext in queryExpressionContextsList)
        {
            // Create the query binary expression object.
            var queryExpression = leftContext.GetSqlParserObjectUpTo<QueryBinaryExpression>(queryExpressionContext, code);
            if (queryExpression is null) return ControlFlow.Break;

            // Place the current left expression as first child.
            queryExpression.Children.Add(leftExpression);

            // Calculate the remaining children recursively.
            queryExpression.Children.AddRange(Visit(queryExpressionContext, queryExpression, code));

            // Update left expression parent with this query binary expression and place
            // the latter as new left expression (for the next set operation).
            leftExpression.Parent = queryExpression;
            leftExpression = queryExpression;
            leftContext = queryExpressionContext;
        }

        // Update out parameter with the query binary expression obtained
        // (at this point, left expression is always a QueryBinaryExpression).
        conversion = leftExpression as QueryBinaryExpression;
        return ControlFlow.Break;
    }

    /// Convert a QuerySpecificationContext in the corresponding SelectStatement object.
    /// This conversion is complete (breaks the visit).
    private ControlFlow VisitQuerySpecification(QuerySpecificationContext context, string code, out QuerySpecification? conversion)
    {
        // Set default value to out parameter.
        conversion = null;

        // Create SELECT statement object and put select clause object as first child.
        var querySpecification = context.GetSqlParserObject<QuerySpecification>(code);
        if (querySpecification is null) return ControlFlow.Break;

        // Get SELECT clause.
        // If conversion fails, return the control flow instructions
        // returned by the visit method, to handle the error correctly.
        var flow = VisitSelectClause(context, code, out var selectClause);
        if (selectClause is null) return flow;

        // Calculate this object children recursively. Prepend select clause.
        var children = context.GetNonErrorChildren()
            .Where(child => child is not TerminalNodeImpl and not SelectItemListContext)
            .SelectMany(child => Visit(child, querySpecification, code))
            .Prepend(selectClause);

        // Complete sql parser object with references to parent and children.
        selectClause.Parent = querySpecification;
        querySpecification.Children = children.ToList();

        // Update out parameter with the SELECT statement obtained.
        conversion = querySpecification;
        return ControlFlow.Break;
    }

    /// Convert a QuerySpecificationContext in the corresponding SelectStatement object.
    /// This conversion is complete (breaks the visit).
    private ControlFlow VisitSelectClause(QuerySpecificationContext context, string code, out SelectClause? conversion)
    {
        // Set default value to out parameter.
        conversion = null;

        // If terminal token "SELECT" is not found, return null.
        if (context.GetChildOfType<TerminalNodeImpl>() is not { } selectToken) return ControlFlow.Break;

        // If columns declaration token is not found return null.
        if (context.GetChildOfType<SelectItemListContext>() is not { } columnsContext) return ControlFlow.Break;

        // Create SELECT clause object.
        var selectClause = selectToken.GetSqlParserObjectUpTo<SelectClause>(columnsContext, code);
        if (selectClause is null) return ControlFlow.Break;

        // Calculate selection columns recursively.
        selectClause.Children = Visit(columnsContext, selectClause, code);

        // Search for selection options.
        if (context.GetChildOfType<SelectOptionContext>() is { } optionContext)
        {
            // Otherwise, get selection option tokens.
            var options = optionContext.GetDescendentsOfType<TerminalNodeImpl>();

            // Search if DISTINCT qualifier is among selection options.
            selectClause.Distinct = options.Any(option =>
                string.Equals("DISTINCT", option.GetText(), StringComparison.InvariantCultureIgnoreCase));
        }

        // Update out parameter with the SELECT clause obtained.
        conversion = selectClause;
        return ControlFlow.Break;
    }

    /// Convert a SelectItemContext in the corresponding AliasedExpression object if applies.
    /// This conversion is partial (continues the visit).
    private static ControlFlow VisitSelectExpression(SelectItemContext context, string code, out SelectExpression? conversion)
    {
        // Set default value to out parameter.
        conversion = null;

        // Ignore this selection item if it is just a table wildcard expression.
        if (context.GetChildOfType<TableWildContext>() is not null) return ControlFlow.Continue;

        var selectionItem = context.GetSqlParserObject<SelectExpression>(code);
        if (selectionItem is null) return ControlFlow.Break;

        // Find column alias between this token descendents.
        if (context.GetChildOfType<SelectAliasContext>() is { } aliasContext)
            selectionItem.Alias = aliasContext.GetChildOfType<IdentifierContext>()?.GetText() ??
                                  aliasContext.GetChildOfType<TextStringLiteralContext>()?.GetText();

        // Update out parameter with the aliased expression obtained.
        conversion = selectionItem;
        return ControlFlow.Continue;
    }

    /// Convert a SimpleExprColumnRefContext in the corresponding ColumnDeclaration object.
    /// This conversion is complete (breaks the visit).
    private static ControlFlow VisitColumnReference(ColumnRefContext context, string code, out ColumnReference? conversion)
    {
        // Set default value to out parameter.
        conversion = null;

        var column = context.GetSqlParserObject<ColumnReference>(code);
        if (column is null) return ControlFlow.Break;

        // Search for column identifier and set it as column name.
        // If it is not found, then this is not a valid column reference.
        if (context.GetChildOfType<FieldIdentifierContext>() is not { } identifierContext) return ControlFlow.Break;
        column.Name = identifierContext.GetText();

        // Update out parameter with the column declaration obtained.
        conversion = column;
        return ControlFlow.Break;
    }

    /// Convert an TableWildContext in the corresponding TableWildcardExpression object.
    /// This conversion is complete (breaks the visit).
    private static ControlFlow VisitTableWildcardExpression(TableWildContext context, string code, out TableWildcardExpression? conversion)
    {
        // Set default value to out parameter.
        conversion = null;

        var tableWildcardExpression = context.GetSqlParserObject<TableWildcardExpression>(code);
        if (tableWildcardExpression is null) return ControlFlow.Break;

        // Search for table identifier and set it as table alias.
        // If it is not found, then this is not a valid table wildcard expression.
        if (context.GetChildOfType<IdentifierContext>() is not { } identifierContext) return ControlFlow.Break;
        tableWildcardExpression.TableAlias = identifierContext.GetText();

        // Update out parameter with the table wildcard expression obtained.
        conversion = tableWildcardExpression;
        return ControlFlow.Break;
    }

    /// Convert a TableReferenceContext in the corresponding TableJoinExpression object if applies.
    /// This conversion is complete (breaks the visit).
    private ControlFlow VisitTableJoinExpression(TableReferenceContext context, string code, out TableJoinExpression? conversion)
    {
        // Set default value to out parameter.
        conversion = null;

        // If the table reference doesn't have table factor or join contexts, continue the visit
        // (is another kind of table declaration or a single table reference).
        if (context.GetChildOfType<TableFactorContext>() is not { } tableFactorContext ||
            context.GetChildrenOfType<JoinedTableContext>().ToList() is not { Count: > 0 } joinContextList) return ControlFlow.Continue;

        // Build table declaration corresponding to the left side of the first join.
        ParserRuleContext leftContext = tableFactorContext;
        var leftExpression = Visit(leftContext, null, code).FirstOrDefault();
        if (leftExpression is null) return ControlFlow.Break;

        // Build the join expression corresponding to each join context.
        foreach (var joinContext in joinContextList)
        {
            // Create the JOIN expression object.
            var joinExpression = leftContext.GetSqlParserObjectUpTo<TableJoinExpression>(joinContext, code);
            if (joinExpression is null) return ControlFlow.Break;

            // Place the current left expression as first child.
            joinExpression.Children.Add(leftExpression);

            // Calculate the remaining children recursively.
            joinExpression.Children.AddRange(Visit(joinContext, joinExpression, code));

            // Update left expression parent with this join expression and place
            // the latter as new left expression (for the next join).
            leftExpression.Parent = joinExpression;
            leftExpression = joinExpression;
            leftContext = joinContext;
        }

        // Update out parameter with the table join expression obtained
        // (at this point, left expression is always a TableJoinExpression).
        conversion = leftExpression as TableJoinExpression;
        return ControlFlow.Break;
    }

    /// Convert a SingleTableContext in the corresponding TableDeclaration object.
    /// This conversion is complete (breaks the visit).
    private static ControlFlow VisitTableReference(SingleTableContext context, string code, out TableReference? conversion)
    {
        // Set default value to out parameter.
        conversion = null;

        var table = context.GetSqlParserObject<TableReference>(code);
        if (table is null) return ControlFlow.Break;

        // Find table reference between this token children.
        if (context.GetChildOfType<TableRefContext>() is not { } tableRefContext) return ControlFlow.Break;
        table.Name = tableRefContext.GetText();

        // Find table alias between this token children.
        if (context.GetChildOfType<TableAliasContext>() is { } aliasContext)
            table.Alias = aliasContext.GetChildOfType<IdentifierContext>()?.GetText();

        // Update out parameter with the table declaration obtained.
        conversion = table;
        return ControlFlow.Break;
    }

    /// Convert a DerivedTableContext in the corresponding TableDeclaration object.
    /// This conversion is partial (continues the visit).
    private static ControlFlow VisitDerivedTableDeclaration(DerivedTableContext context, string code, out DerivedTableExpression? conversion)
    {
        // Set default value to out parameter.
        conversion = null;

        var table = context.GetSqlParserObject<DerivedTableExpression>(code);
        if (table is null) return ControlFlow.Break;

        // Find table alias between this token descendents.
        if (context.GetChildOfType<TableAliasContext>() is { } aliasContext)
            table.Alias = aliasContext.GetChildOfType<IdentifierContext>()?.GetText();

        // Update out parameter with the derived table obtained.
        conversion = table;
        return ControlFlow.Continue;
    }

    /// Convert an OrderExpressionContext in the corresponding OrderByExpression object.
    /// This conversion is partial (continues the visit).
    private static ControlFlow VisitOrderByExpression(OrderExpressionContext context, string code, out OrderByExpression? conversion)
    {
        // Set default value to out parameter.
        conversion = null;

        var orderByExpression = context.GetSqlParserObject<OrderByExpression>(code);
        if (orderByExpression is null) return ControlFlow.Break;

        // If order by expression doesn't contain direction expressions, return.
        if (context.GetChildOfType<DirectionContext>() is { } directionContext)
        {
            // Otherwise, find sort direction reference between this token descendents.
            if (string.Equals("ASC", directionContext.GetText(), StringComparison.InvariantCultureIgnoreCase))
                orderByExpression.Direction = SortOrder.Asc;

            if (string.Equals("DESC", directionContext.GetText(), StringComparison.InvariantCultureIgnoreCase))
                orderByExpression.Direction = SortOrder.Desc;
        }

        // Update out parameter with the order by expression obtained.
        conversion = orderByExpression;
        return ControlFlow.Continue;
    }

    /// Convert a PredicateContext in the corresponding InExpression object if applies.
    /// This conversion is partial (continues the visit).
    private static ControlFlow VisitInExpression(PredicateContext context, string code, out InExpression? conversion)
    {
        // Set default value to out parameter.
        conversion = null;

        // Find the in context corresponding to this predicate context.
        // If it is not found, as this predicate context is not an in expression, continue the visit.
        if (context.GetChildOfType<PredicateExprInContext>() is null) return ControlFlow.Continue;

        var inExpression = context.GetSqlParserObject<InExpression>(code);
        if (inExpression is null) return ControlFlow.Break;

        // Update out parameter with the in expression obtained.
        conversion = inExpression;
        return ControlFlow.Continue;
    }

    /// Convert an PrimaryExprCompareContext in the corresponding ComparisonExpression object.
    /// This conversion is partial (continues the visit).
    private static ControlFlow VisitComparisonExpression(PrimaryExprCompareContext context, string code, out ComparisonExpression? conversion)
    {
        // Set default value to out parameter.
        conversion = null;

        // Find the operation corresponding to this comparison context.
        if (context.GetChildOfType<CompOpContext>() is not { } operatorContext ||
            !TryParseEnum<ComparisonOperation>(operatorContext.GetText(), out var operation))
            return ControlFlow.Break;

        var comparisonExpression = context.GetSqlParserObject<ComparisonExpression>(code);
        if (comparisonExpression is null) return ControlFlow.Break;

        comparisonExpression.Operation = operation.Value;

        // Update out parameter with the comparison expression obtained.
        conversion = comparisonExpression;
        return ControlFlow.Continue;
    }

    /// Convert a LimitClauseContext in the corresponding LimitClause object.
    /// This conversion is partial (continues the visit).
    private static ControlFlow VisitLimitClause(LimitClauseContext context, string code, out LimitClause? conversion)
    {
        // Set default value to out parameter.
        conversion = null;

        // Create LIMIT clause object.
        var limitClause = context.GetSqlParserObject<LimitClause>(code);
        if (limitClause is null) return ControlFlow.Break;

        // Search for selection options.
        if (context.GetChildOfType<LimitOptionsContext>() is { } optionContext)
        {
            // Get selection option tokens.
            var options = optionContext.GetDescendentsOfType<TerminalNodeImpl>().ToList();

            // Search if OFFSET qualifier is specified explicitly.
            limitClause.ExplicitNotation = options.Count == 1 || options.Any(option =>
                string.Equals("OFFSET", option.GetText(), StringComparison.InvariantCultureIgnoreCase));
        }

        // Update out parameter with the SELECT clause obtained.
        conversion = limitClause;
        return ControlFlow.Continue;
    }

    /// Checks if a given string represents a SqlComparisonExpressionType.
    /// Returns the corresponding SqlComparisonExpressionType if true, and null otherwise.
    public static bool TryParseEnum<T>(string value, [NotNullWhen(true)] out T? expressionType) where T : struct, Enum
    {
        expressionType = typeof(T).GetFields()
            .FirstOrDefault(field => field.GetCustomAttribute<EnumMemberAttribute>()?.Value == value)
            ?.GetValue(null) as T?;

        return expressionType is not null;
    }
}