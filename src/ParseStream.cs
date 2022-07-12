using System;
using System.Linq;

namespace Aadev.ConditionsInterpreter
{
    internal class ParseStream
    {
        public ParseStream(LexingStream stream)
        {
            this.stream = stream;
            NextToken();
        }
        private Token? currentToken;
        private readonly LexingStream stream;

        public Token? NextToken() => currentToken = stream.NextToken();

        public Node Parse() => CreateStage7();


        private Node CreateStage0()
        {
            if (currentToken is null)
                throw new Exception("Token is null");
            Token token = (Token)currentToken;

            if (token.Type is TokenType.Subtract)
            {
                NextToken();
                return new SingleOpNode(CreateStage0(), token.Type);
            }
            if (token.Type is TokenType.Keyword)
            {
                NextToken();
                switch ((Keywords)token.Value!)
                {
                    case Keywords.True:
                        return new BoolNode(true);
                    case Keywords.False:
                        return new BoolNode(false);
                    default:
                        break;
                }
            }
            if (token.Type is TokenType.Number)
            {
                NextToken();
                return new NumberNode((double)token.Value!);
            }
            if (token.Type is TokenType.String)
            {
                NextToken();
                return new StringNode((string)token.Value!);
            }
            if (token.Type is TokenType.Not)
            {
                NextToken();
                return new SingleOpNode(CreateStage0(), token.Type);
            }
            if (token.Type is TokenType.LParentheses)
            {
                NextToken();
                Node expression = CreateStage7();

                if (currentToken is null)
                    throw new Exception("Token is null");

                if (currentToken?.Type is TokenType.RParentheses)
                {
                    NextToken();
                    return expression;
                }
                throw new Exception("Missing ')'");
            }


            if (token.Type is TokenType.StringBeginning)
            {
                NextToken();
                if (currentToken is null)
                    throw new Exception("Token is null");
                if (currentToken?.Type is TokenType.StringEnding)
                {
                    NextToken();
                    return StringNode.Empty;
                }

                Node expression = CreateStage7();
                if (currentToken?.Type is TokenType.StringEnding)
                {
                    NextToken();
                    return expression;
                }
                throw new Exception("Missing end of string");
            }

            if (token.Type is TokenType.Variable)
            {
                NextToken();
                return new VarNode((string)token.Value!);
            }

            throw new Exception("Invalid syntax");

        }
        private Node CreateStage1()
        {
            Node left = CreateStage0();
            while (currentToken != null && ((Token)currentToken).Type == TokenType.Variable)
            {
                left = new ConcatingNode(left, CreateStage0());
            }
            return left;
        }
        private Node CreateStage2() => CreateDoubleOpNode(CreateStage1, new TokenType[] { TokenType.Multiply, TokenType.Divide, TokenType.Modulo });
        private Node CreateStage3() => CreateDoubleOpNode(CreateStage2, new TokenType[] { TokenType.Add, TokenType.Subtract });
        private Node CreateStage4() => CreateDoubleOpNode(CreateStage3, new TokenType[] { TokenType.Equal, TokenType.NotEqual, TokenType.Greater, TokenType.Less });
        private Node CreateStage5() => CreateDoubleOpNode(CreateStage4, new TokenType[] { TokenType.Or });
        private Node CreateStage6() => CreateDoubleOpNode(CreateStage5, new TokenType[] { TokenType.And });
        private Node CreateStage7() => CreateDoubleOpNode(CreateStage6, new TokenType[] { TokenType.Xor });
        private Node CreateDoubleOpNode(Func<Node> function, TokenType[] tokenTypes)
        {
            Node left = function();
            while (currentToken != null && tokenTypes.Contains(((Token)currentToken).Type))
            {
                Token @operator = (Token)currentToken;
                NextToken();
                left = new DoubleOpNode(left, @operator.Type, function());
            }
            return left;
        }
    }

}
