/*
 * Copyright © 2024, Oracle and/or its affiliates
 */

using Antlr4.Runtime;
using BBF.Reporting.QueryBuilder.Enums;

namespace BBF.Reporting.QueryBuilder.Generated;

public abstract class MySqlParserBase : Parser
{
    // To parameterize the parsing process.
    public int ServerVersion = 0;
    public HashSet<MySqlMode> SqlModes = new();

    /** Enable Multi Language Extension support. */
    public bool SupportMle = true;

    protected MySqlParserBase(ITokenStream input, TextWriter output, TextWriter errorOutput) : base(input, output, errorOutput)
    {
    }

    /**
* Determines if the given SQL mode is currently active in the lexer.
*
* @param mode The mode to check.
*
* @returns True if the mode is one of the currently active modes.
*/
    public bool IsSqlModeActive(MySqlMode mode)
    {
        return SqlModes.Contains(mode);
    }

    public bool IsPureIdentifier()
    {
        return IsSqlModeActive(MySqlMode.AnsiQuotes);
    }

    public bool IsTextStringLiteral()
    {
        return !IsSqlModeActive(MySqlMode.AnsiQuotes);
    }

    public bool IsStoredRoutineBody()
    {
        return ServerVersion >= 80032 && SupportMle;
    }

    public bool IsSelectStatementWithInto()
    {
        return ServerVersion is >= 80024 and < 80031;
    }
}