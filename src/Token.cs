using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Aadev.ConditionsInterpreter
{
    internal struct Token : IEquatable<Token>
    {
        [DebuggerStepThrough]
        public Token(TokenType tokenType, object? value = null)
        {
            Type = tokenType;
            Value = value;
        }

        public TokenType Type { get; }
        public object? Value { get; }

        public override bool Equals(object? obj) => obj is Token token && Equals(token);
        public bool Equals(Token other) => Type == other.Type && EqualityComparer<object?>.Default.Equals(Value, other.Value);
        public override int GetHashCode() => HashCode.Combine(Type, Value);
        public override string ToString() => $"[{Type}]{(Value is null ? string.Empty : $": {Value}")}";

        public static bool operator ==(Token left, Token right) => left.Equals(right);
        public static bool operator !=(Token left, Token right) => !(left == right);
    }
}
