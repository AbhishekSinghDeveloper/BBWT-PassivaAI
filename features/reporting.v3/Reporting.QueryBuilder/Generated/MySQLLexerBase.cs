using Antlr4.Runtime;
using BBF.Reporting.QueryBuilder.Enums;

namespace BBF.Reporting.QueryBuilder.Generated;

/** SQL modes that control parsing behavior. */
/** The base lexer class provides a number of functions needed in actions in the lexer (grammar). */
public class MySqlLexerBase : Lexer
{
    public int ServerVersion = 0;
    public HashSet<MySqlMode> SqlModes = new();

    /** Enable Multi Language Extension support. */
    public bool SupportMle = true;

    public HashSet<string> CharSets = new(); // Used to check repertoires.
    protected bool InVersionComment;

    private readonly Queue<IToken> _pendingTokens = new();

    private const string LongString = "2147483647";
    private const int LongLength = 10;
    private const string SignedLongString = "-2147483648";
    private const string LongLongString = "9223372036854775807";
    private const int LongLongLength = 19;
    private const string SignedLongLongString = "-9223372036854775808";
    private const int SignedLongLongLength = 19;
    private const string UnsignedLongLongString = "18446744073709551615";
    private const int UnsignedLongLongLength = 20;

    private bool _justEmittedDot;

    public override string[] RuleNames => throw new NotImplementedException();

    public override IVocabulary Vocabulary => throw new NotImplementedException();

    public override string GrammarFileName => throw new NotImplementedException();


    protected MySqlLexerBase(ICharStream input, TextWriter output, TextWriter errorOutput)
        : base(input, output, errorOutput)
    {
    }

    public MySqlLexerBase(ICharStream input)
        : base(input)
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

    /**
 * Converts a mode string into individual mode flags.
 *
 * @param modes The input string to parse.
 */
    public void SqlModeFromString(string modes)
    {
        SqlModes = new HashSet<MySqlMode>();

        var parts = modes.ToUpper().Split(",");
        foreach (var mode in parts)
        {
            switch (mode)
            {
                case "ANSI" or "DB2" or "MAXDB" or "MSSQL" or "ORACLE" or "POSTGRESQL":
                    SqlModes.Add(MySqlMode.AnsiQuotes);
                    SqlModes.Add(MySqlMode.PipesAsConcat);
                    SqlModes.Add(MySqlMode.IgnoreSpace);
                    break;
                case "ANSI_QUOTES":
                    SqlModes.Add(MySqlMode.AnsiQuotes);
                    break;
                case "PIPES_AS_CONCAT":
                    SqlModes.Add(MySqlMode.PipesAsConcat);
                    break;
                case "NO_BACKSLASH_ESCAPES":
                    SqlModes.Add(MySqlMode.NoBackslashEscapes);
                    break;
                case "IGNORE_SPACE":
                    SqlModes.Add(MySqlMode.IgnoreSpace);
                    break;
                case "HIGH_NOT_PRECEDENCE" or "MYSQL323" or "MYSQL40":
                    SqlModes.Add(MySqlMode.HighNotPrecedence);
                    break;
            }
        }
    }

    /**
 * Resets the lexer by setting initial values to transient member, resetting the input stream position etc.
 */
    public override void Reset()
    {
        InVersionComment = false;
        base.Reset();
    }

    /**
 * Implements the multi token feature required in our lexer.
 * A lexer rule can emit more than a single token, if needed.
 *
 * @returns The next token in the token stream.
 */
    public override IToken NextToken()
    {
        // First respond with pending tokens to the next token request, if there are any.
        if (_pendingTokens.Count > 0)
            return _pendingTokens.Dequeue();

        // Let the main lexer class run the next token recognition.
        // This might create additional tokens again.
        var next = base.NextToken();

        // If no more tokens were created, return the next token sent by the main lexer.
        if (_pendingTokens.Count == 0) return next;

        // Otherwise, return first token of the queue.
        var pending = _pendingTokens.Dequeue();
        _pendingTokens.Enqueue(next);

        return pending;
    }

    /**
 * Checks if the version number in the token text is less than or equal to the current server version.
 *
 * @param text The text from a matched token.
 * @returns True if so the number matches, otherwise false.
 */
    protected bool CheckMySqlVersion(string text)
    {
        if (text.Length < 8)
        {
            // Minimum is: /*!12345
            return false;
        }

        // Skip version comment introducer.
        var version = int.Parse(text[3..]);

        if (version > ServerVersion) return false;

        InVersionComment = true;
        return true;
    }

    /**
 * Called when a keyword was consumed that represents an internal MySQL function and checks if that keyword is
 * followed by an open parenthesis. If not then it is not considered a keyword but treated like a normal identifier.
 *
 * @param proposed The token type to use if the check succeeds.
 *
 * @returns If a function call is found then return the proposed token type, otherwise just IDENTIFIER.
 */
    protected int DetermineFunction(int proposed)
    {
        // Skip any whitespace character if the sql mode says they should be ignored,
        // before actually trying to match the open parenthesis.
        var input = (char)InputStream.LA(1);

        if (IsSqlModeActive(MySqlMode.IgnoreSpace))
        {
            while (input is ' ' or '\t' or '\r' or '\n')
            {
                Interpreter.Consume((ICharStream)InputStream);
                Channel = Hidden;
                Type = MySqlLexer.WHITESPACE;
                input = (char)InputStream.LA(1);
            }
        }

        return input == '(' ? proposed : MySqlLexer.IDENTIFIER;
    }

    /**
 * Checks the given text and determines the smallest number type from it. Code has been taken from sql_lex.cc.
 *
 * @param text The text to parse (which must be a number).
 *
 * @returns The token type for that text.
 */
    protected static int DetermineNumericType(string text)
    {
        // The original code checks for leading +/- but actually that can never happen, neither in the
        // server parser (as a digit is used to trigger processing in the lexer) nor in our parser
        // as our rules are defined without signs. But we do it anyway for maximum compatibility.
        var length = text.Length - 1;
        if (length < LongLength)
        {
            // quick normal case
            return MySqlLexer.INT_NUMBER;
        }

        var negative = false;
        var index = 0;

        // Remove sign and pre-zeros
        switch (text[index])
        {
            case '+':
                ++index;
                --length;
                break;

            case '-':
                ++index;
                --length;
                negative = true;
                break;
        }

        while (text[index] == '0' && length > 0)
        {
            ++index;
            --length;
        }

        if (length < LongLength)
        {
            return MySqlLexer.INT_NUMBER;
        }

        int smaller;
        int bigger;
        string cmp;
        if (negative)
        {
            switch (length)
            {
                case LongLength:
                    cmp = SignedLongString[1..];
                    smaller = MySqlLexer.INT_NUMBER; // If <= signed_long_str
                    bigger = MySqlLexer.LONG_NUMBER; // If >= signed_long_str
                    break;

                case < SignedLongLongLength:
                    return MySqlLexer.LONG_NUMBER;

                case > SignedLongLongLength:
                    return MySqlLexer.DECIMAL_NUMBER;

                default:
                    cmp = SignedLongLongString[1..];
                    smaller = MySqlLexer.LONG_NUMBER; // If <= signed_longlong_str
                    bigger = MySqlLexer.DECIMAL_NUMBER;
                    break;
            }
        }
        else
        {
            switch (length)
            {
                case LongLength:
                    cmp = LongString;
                    smaller = MySqlLexer.INT_NUMBER;
                    bigger = MySqlLexer.LONG_NUMBER;
                    break;

                case < LongLongLength:
                    return MySqlLexer.LONG_NUMBER;

                case > LongLongLength and > UnsignedLongLongLength:
                    return MySqlLexer.DECIMAL_NUMBER;

                case > LongLongLength:
                    cmp = UnsignedLongLongString;
                    smaller = MySqlLexer.ULONGLONG_NUMBER;
                    bigger = MySqlLexer.DECIMAL_NUMBER;
                    break;

                default:
                    cmp = LongLongString;
                    smaller = MySqlLexer.LONG_NUMBER;
                    bigger = MySqlLexer.ULONGLONG_NUMBER;
                    break;
            }
        }

        var otherIndex = 0;

        while (index < text.Length && cmp[otherIndex] == text[index])
        {
            otherIndex++;
            index++;
        }

        return text[index - 1] <= cmp[otherIndex - 1] ? smaller : bigger;
    }

    /**
 * Checks if the given text corresponds to a charset defined in the server (text is preceded by an underscore).
 *
 * @param text The text to check.
 *
 * @returns UNDERSCORE_CHARSET if so, otherwise IDENTIFIER.
 */
    protected int CheckCharset(string text)
    {
        return CharSets.Contains(text) ? MySqlLexer.UNDERSCORE_CHARSET : MySqlLexer.IDENTIFIER;
    }

    /**
 * Creates a DOT token in the token stream.
 */
    protected void EmitDot()
    {
        var source = new Tuple<ITokenSource, ICharStream>(this, (ICharStream)InputStream);
        const int type = MySqlLexer.DOT_SYMBOL;

        var start = TokenStartCharIndex;
        var stop = TokenStartCharIndex;
        var charPositionInLine = Column;

        var token = TokenFactory.Create(source, type, Text, Channel, start, stop, Line, charPositionInLine);

        _pendingTokens.Enqueue(token);

        ++Column;
        _justEmittedDot = true;
    }


    // Version-related methods
    public bool IsServerVersionLt80024() => ServerVersion < 80024;
    public bool IsServerVersionGe80024() => ServerVersion >= 80024;
    public bool IsServerVersionGe80011() => ServerVersion >= 80011;
    public bool IsServerVersionGe80013() => ServerVersion >= 80013;
    public bool IsServerVersionLt80014() => ServerVersion < 80014;
    public bool IsServerVersionGe80014() => ServerVersion >= 80014;
    public bool IsServerVersionGe80017() => ServerVersion >= 80017;
    public bool IsServerVersionGe80018() => ServerVersion >= 80018;

    public bool IsMasterCompressionAlgorithm() => ServerVersion >= 80018 && IsServerVersionLt80024();

    public bool IsServerVersionLt80031() => ServerVersion < 80031;

    // Functions for specific token types
    public void DoLogicalOr()
    {
        Type = IsSqlModeActive(MySqlMode.PipesAsConcat) ? MySqlLexer.CONCAT_PIPES_SYMBOL : MySqlLexer.LOGICAL_OR_OPERATOR;
    }

    public void DoIntNumber()
    {
        Type = DetermineNumericType(Text);
    }

    public void DoAdddate() => Type = DetermineFunction(MySqlLexer.ADDDATE_SYMBOL);
    public void DoBitAnd() => Type = DetermineFunction(MySqlLexer.BIT_AND_SYMBOL);
    public void DoBitOr() => Type = DetermineFunction(MySqlLexer.BIT_OR_SYMBOL);
    public void DoBitXor() => Type = DetermineFunction(MySqlLexer.BIT_XOR_SYMBOL);
    public void DoCast() => Type = DetermineFunction(MySqlLexer.CAST_SYMBOL);
    public void DoCount() => Type = DetermineFunction(MySqlLexer.COUNT_SYMBOL);
    public void DoCurdate() => Type = DetermineFunction(MySqlLexer.CURDATE_SYMBOL);
    public void DoCurrentDate() => Type = DetermineFunction(MySqlLexer.CURDATE_SYMBOL);
    public void DoCurrentTime() => Type = DetermineFunction(MySqlLexer.CURTIME_SYMBOL);
    public void DoCurtime() => Type = DetermineFunction(MySqlLexer.CURTIME_SYMBOL);
    public void DoDateAdd() => Type = DetermineFunction(MySqlLexer.DATE_ADD_SYMBOL);
    public void DoDateSub() => Type = DetermineFunction(MySqlLexer.DATE_SUB_SYMBOL);
    public void DoExtract() => Type = DetermineFunction(MySqlLexer.EXTRACT_SYMBOL);
    public void DoGroupConcat() => Type = DetermineFunction(MySqlLexer.GROUP_CONCAT_SYMBOL);
    public void DoMax() => Type = DetermineFunction(MySqlLexer.MAX_SYMBOL);
    public void DoMid() => Type = DetermineFunction(MySqlLexer.SUBSTRING_SYMBOL);
    public void DoMin() => Type = DetermineFunction(MySqlLexer.MIN_SYMBOL);
    public void DoNot() => Type = IsSqlModeActive(MySqlMode.HighNotPrecedence) ? MySqlLexer.NOT2_SYMBOL : MySqlLexer.NOT_SYMBOL;
    public void DoNow() => Type = DetermineFunction(MySqlLexer.NOW_SYMBOL);
    public void DoPosition() => Type = DetermineFunction(MySqlLexer.POSITION_SYMBOL);
    public void DoSessionUser() => Type = DetermineFunction(MySqlLexer.USER_SYMBOL);
    public void DoStddevSamp() => Type = DetermineFunction(MySqlLexer.STDDEV_SAMP_SYMBOL);
    public void DoStddev() => Type = DetermineFunction(MySqlLexer.STD_SYMBOL);
    public void DoStddevPop() => Type = DetermineFunction(MySqlLexer.STD_SYMBOL);
    public void DoStd() => Type = DetermineFunction(MySqlLexer.STD_SYMBOL);
    public void DoSubdate() => Type = DetermineFunction(MySqlLexer.SUBDATE_SYMBOL);
    public void DoSubstr() => Type = DetermineFunction(MySqlLexer.SUBSTRING_SYMBOL);
    public void DoSubstring() => Type = DetermineFunction(MySqlLexer.SUBSTRING_SYMBOL);
    public void DoSum() => Type = DetermineFunction(MySqlLexer.SUM_SYMBOL);
    public void DoSysdate() => Type = DetermineFunction(MySqlLexer.SYSDATE_SYMBOL);
    public void DoSystemUser() => Type = DetermineFunction(MySqlLexer.USER_SYMBOL);
    public void DoTrim() => Type = DetermineFunction(MySqlLexer.TRIM_SYMBOL);
    public void DoVariance() => Type = DetermineFunction(MySqlLexer.VARIANCE_SYMBOL);
    public void DoVarPop() => Type = DetermineFunction(MySqlLexer.VARIANCE_SYMBOL);
    public void DoVarSamp() => Type = DetermineFunction(MySqlLexer.VAR_SAMP_SYMBOL);
    public void DoUnderscoreCharset() => Type = CheckCharset(Text);

    public bool IsVersionComment() => CheckMySqlVersion(Text);

    public bool IsBackTickQuotedId()
    {
        return !IsSqlModeActive(MySqlMode.NoBackslashEscapes);
    }

    public bool IsDoubleQuotedText()
    {
        return !IsSqlModeActive(MySqlMode.NoBackslashEscapes);
    }

    public bool IsSingleQuotedText()
    {
        return !IsSqlModeActive(MySqlMode.NoBackslashEscapes);
    }

    public override IToken Emit()
    {
        var source = new Tuple<ITokenSource, ICharStream>(this, (ICharStream)InputStream);

        var text = Text != null ? _justEmittedDot ? Text[1..] : Text : null;
        var start = TokenStartCharIndex + (_justEmittedDot ? 1 : 0);
        var stop = CharIndex - 1;
        var line = TokenStartLine;
        var charPositionInLine = TokenStartColumn;

        var token = TokenFactory.Create(source, Type, text, Channel, start, stop, line, charPositionInLine);

        _justEmittedDot = false;
        base.Emit(token);

        return token;
    }

    public void StartInVersionComment()
    {
        InVersionComment = true;
    }

    public void EndInVersionComment()
    {
        InVersionComment = false;
    }

    public bool IsInVersionComment()
    {
        return InVersionComment;
    }
}