using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Aadev.ConditionsInterpreter
{
    internal class Parser
    {
        private readonly Token[] tokens;
        private int index = -1;
        private Token currentToken;
        public Parser(Token[] tokens)
        {
            this.tokens = tokens;
            NextToken();
        }
        public Token NextToken()
        {
            index++;
            return currentToken = tokens.Length <= index ? null : tokens[index];
        }

        public Node Parse() => CreateStage7();


        private Node CreateStage0()
        {
            Token token = currentToken;
            if (token is null)
                throw new Exception("Unknown error");

            if (token.Type is TokenType.Subtract)
            {
                NextToken();
                return new SingleOpNode(CreateStage0(), token);
            }
            if (token.Type is TokenType.Keyword)
            {
                NextToken();
                switch ((Keywords)token.Value)
                {
                    case Keywords.True:
                        return new BoolNode(token);
                    case Keywords.False:
                        return new BoolNode(token);
                    default:
                        break;
                }
            }
            if (token.Type is TokenType.Number)
            {
                NextToken();
                return new NumberNode(token);
            }
            if (token.Type is TokenType.String)
            {
                NextToken();
                return new StringNode(token);
            }
            if (token.Type is TokenType.Not)
            {
                NextToken();
                return new SingleOpNode(CreateStage0(), token);
            }
            if (token.Type is TokenType.LParentheses)
            {
                NextToken();
                Node expression = CreateStage7();
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

                if(currentToken.Type == TokenType.StringEnding)
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
                return new VarNode(token);
            }

            throw new Exception("Invalid syntax");

        }
        private Node CreateStage1()
        {
            Node left = CreateStage0();
            while (currentToken != null && currentToken.Type == TokenType.Variable)
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
            while (currentToken != null && tokenTypes.Contains(currentToken.Type))
            {
                Token @operator = currentToken;
                NextToken();
                left = new DoubleOpNode(left, @operator, function());
            }
            return left;
        }
    }



    internal abstract class Node
    {
        public abstract object GetValue(ConditionsInterpreter interpreter);
    }

    internal sealed class VarNode : Node
    {
        public Token Value { get; }

        public VarNode(Token value)
        {
            Value = value;
        }

        public override object GetValue(ConditionsInterpreter interpreter)
        {
            string varName = Value.Value.ToString();

            return interpreter.GetVariableValue(varName);
        }
    }
    internal sealed class StringNode : Node
    {
        internal static readonly Node Empty = new StringNode(new Token(TokenType.String, string.Empty));

        public Token Token { get; }

        public StringNode(Token value)
        {
            Token = value;
        }

        public override object GetValue(ConditionsInterpreter interpreter) => Token.Value.ToString();
    }
    internal sealed class ConcatingNode : Node
    {
        public ConcatingNode(Node lNode, Node rNode)
        {
            LNode = lNode;
            RNode = rNode;
        }

        public Node LNode { get; }
        public Node RNode { get; }

        public override object GetValue(ConditionsInterpreter interpreter) => $"{LNode.GetValue(interpreter)}{RNode.GetValue(interpreter)}";
    }
    internal sealed class BoolNode : Node
    {
        public Token Value { get; }

        public BoolNode(Token value)
        {
            Value = value;
        }

        public override object GetValue(ConditionsInterpreter interpreter) => (Keywords)Value.Value == Keywords.True;
    }
    internal sealed class NumberNode : Node
    {
        public Token Value { get; }

        public NumberNode(Token value)
        {
            Value = value;
        }

        public override object GetValue(ConditionsInterpreter interpreter) => (double)Value.Value;
    }

    internal sealed class SingleOpNode : Node
    {
        public SingleOpNode(Node node, Token @operator)
        {
            Node = node;
            Operator = @operator;
        }

        public Node Node { get; }

        public Token Operator { get; }

        public override object GetValue(ConditionsInterpreter interpreter)
        {
            object rVal = Node.GetValue(interpreter);



            if (Operator.Type is TokenType.Not)
            {
                if (!(rVal is bool bVal))
                {
                    throw new Exception("Invalid syntax");
                }
                return !bVal;
            }
            if (Operator.Type is TokenType.Subtract)
            {
                if (!(rVal is double dVal))
                {
                    throw new Exception("Invalid syntax");
                }
                return -dVal;
            }


            throw new Exception("Invalid syntax");

        }
    }
    internal sealed class DoubleOpNode : Node
    {
        public DoubleOpNode(Node lNode, Token @operator, Node rNode)
        {
            LNode = lNode;
            Operator = @operator;
            RNode = rNode;
        }

        public Node LNode { get; }
        public Token Operator { get; }
        public Node RNode { get; }

        public override object GetValue(ConditionsInterpreter interpreter)
        {
            switch (Operator.Type)
            {
                case TokenType.And:
                {
                    object rlVal = LNode.GetValue(interpreter);

                    if (!(rlVal is bool blVal))
                        throw new Exception("Invalid syntax");
                    if (blVal is false)
                        return false;



                    object rrVal = RNode.GetValue(interpreter);

                    if (!(rrVal is bool brVal))
                        throw new Exception("Invalid syntax");
                    if (brVal is false)
                        return false;


                    return true;

                }
                case TokenType.Or:
                {
                    object rlVal = LNode.GetValue(interpreter);

                    if (!(rlVal is bool blVal))
                        throw new Exception("Invalid syntax");
                    if (blVal is true)
                        return true;


                    object rrVal = RNode.GetValue(interpreter);

                    if (!(rrVal is bool brVal))
                        throw new Exception("Invalid syntax");
                    if (brVal is true)
                        return true;

                    return false;
                }
                case TokenType.Xor:
                {
                    object rlVal = LNode.GetValue(interpreter);
                    object rrVal = RNode.GetValue(interpreter);

                    if (!(rlVal is bool blVal))
                        throw new Exception("Invalid syntax");
                    if (!(rrVal is bool brVal))
                        throw new Exception("Invalid syntax");

                    return blVal ^ brVal;
                }
                case TokenType.Equal:
                {
                    object rlVal = LNode.GetValue(interpreter);
                    object rrVal = RNode.GetValue(interpreter);

                    return rlVal?.Equals(rrVal) ?? false;
                }
                case TokenType.NotEqual:
                {
                    object rlVal = LNode.GetValue(interpreter);
                    object rrVal = RNode.GetValue(interpreter);

                    return !rlVal?.Equals(rrVal) ?? true;
                }
                case TokenType.Less:
                {
                    object rlVal = LNode.GetValue(interpreter);
                    object rrVal = RNode.GetValue(interpreter);


                    double dlVal = 0;
                    double drVal = 0;

                    if (rlVal is double dl)
                        dlVal = dl;
                    if (rrVal is double dr)
                        drVal = dr;
                    if (rlVal is BigInteger bl)
                        dlVal = (double)bl;
                    if (rrVal is BigInteger br)
                        drVal = (double)br;


                    return dlVal < drVal;

                }
                case TokenType.Greater:
                {
                    object rlVal = LNode.GetValue(interpreter);
                    object rrVal = RNode.GetValue(interpreter);


                    double dlVal = 0;
                    double drVal = 0;

                    if (rlVal is double dl)
                        dlVal = dl;
                    if (rrVal is double dr)
                        drVal = dr;
                    if (rlVal is BigInteger bl)
                        dlVal = (double)bl;
                    if (rrVal is BigInteger br)
                        drVal = (double)br;


                    return dlVal > drVal;

                }
                case TokenType.Add:
                {
                    object rlVal = LNode.GetValue(interpreter);
                    object rrVal = RNode.GetValue(interpreter);


                    if (rlVal is string || rrVal is string)
                    {
                        string lval = rlVal.ToString();
                        string rval = rrVal.ToString();


                        return lval + rval;


                    }



                    double dlVal = 0;
                    double drVal = 0;

                    if (rlVal is double dl)
                        dlVal = dl;
                    if (rrVal is double dr)
                        drVal = dr;
                    if (rlVal is BigInteger bl)
                        dlVal = (double)bl;
                    if (rrVal is BigInteger br)
                        drVal = (double)br;


                    return dlVal + drVal;

                }
                case TokenType.Subtract:
                {
                    object rlVal = LNode.GetValue(interpreter);
                    object rrVal = RNode.GetValue(interpreter);


                    double dlVal = 0;
                    double drVal = 0;

                    if (rlVal is double dl)
                        dlVal = dl;
                    if (rrVal is double dr)
                        drVal = dr;
                    if (rlVal is BigInteger bl)
                        dlVal = (double)bl;
                    if (rrVal is BigInteger br)
                        drVal = (double)br;


                    return dlVal - drVal;

                }
                case TokenType.Multiply:
                {
                    object rlVal = LNode.GetValue(interpreter);
                    object rrVal = RNode.GetValue(interpreter);


                    double dlVal = 0;
                    double drVal = 0;

                    if (rlVal is double dl)
                        dlVal = dl;
                    if (rrVal is double dr)
                        drVal = dr;
                    if (rlVal is BigInteger bl)
                        dlVal = (double)bl;
                    if (rrVal is BigInteger br)
                        drVal = (double)br;


                    return dlVal * drVal;

                }
                case TokenType.Divide:
                {
                    object rlVal = LNode.GetValue(interpreter);
                    object rrVal = RNode.GetValue(interpreter);


                    double dlVal = 0;
                    double drVal = 0;

                    if (rlVal is double dl)
                        dlVal = dl;
                    if (rrVal is double dr)
                        drVal = dr;
                    if (rlVal is BigInteger bl)
                        dlVal = (double)bl;
                    if (rrVal is BigInteger br)
                        drVal = (double)br;


                    return dlVal / drVal;

                }
                case TokenType.Modulo:
                {
                    object rlVal = LNode.GetValue(interpreter);
                    object rrVal = RNode.GetValue(interpreter);


                    double dlVal = 0;
                    double drVal = 0;

                    if (rlVal is double dl)
                        dlVal = dl;
                    if (rrVal is double dr)
                        drVal = dr;
                    if (rlVal is BigInteger bl)
                        dlVal = (double)bl;
                    if (rrVal is BigInteger br)
                        drVal = (double)br;


                    return dlVal % drVal;

                }
                default:
                    throw new Exception("Invalid syntax");
            }
        }
    }






}
