using System.Diagnostics;

namespace Aadev.ConditionsInterpreter
{
    internal class Token
    {
        [DebuggerStepThrough]
        public Token(TokenType tokenType, object value = null)
        {
            Type = tokenType;
            Value = value;
        }

        public TokenType Type { get; }
        public object Value { get; }

        public override string ToString() => $"[{Type}]{(Value is null ? string.Empty : $": {Value}")}";

    }
}
