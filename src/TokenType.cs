namespace Aadev.ConditionsInterpreter
{
    internal enum TokenType
    {
        And,
        Or,
        Xor,
        Not,
        Equal,
        NotEqual,
        Less,
        Greater,
        StringBeginning,
        StringEnding,
        Number,
        Keyword,
        String,
        Variable,
        LParentheses,
        RParentheses,
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo
    }
}
