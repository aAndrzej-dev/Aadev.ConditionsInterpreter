using System;

namespace Aadev.ConditionsInterpreter
{
    internal abstract class Node
    {
        public abstract object GetValue(IVariableProvider interpreter);
    }

    internal sealed class VarNode : Node
    {
        public string VarName { get; }

        public VarNode(string varName)
        {
            VarName = varName;
        }

        public override object GetValue(IVariableProvider interpreter) => interpreter.GetVariableValue(VarName);
    }
    internal sealed class StringNode : Node
    {
        internal static readonly Node Empty = new StringNode(string.Empty);

        public string Value { get; }

        public StringNode(string value)
        {
            Value = value;
        }

        public override object GetValue(IVariableProvider interpreter) => Value.ToString();
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

        public override object GetValue(IVariableProvider interpreter) => $"{LNode.GetValue(interpreter)}{RNode.GetValue(interpreter)}";
    }
    internal sealed class BoolNode : Node
    {
        public bool Value { get; }

        public BoolNode(bool value)
        {
            Value = value;
        }

        public override object GetValue(IVariableProvider interpreter) => Value;
    }
    internal sealed class NumberNode : Node
    {
        public double Value { get; }

        public NumberNode(double value)
        {
            Value = value;
        }

        public override object GetValue(IVariableProvider interpreter) => Value;
    }

    internal sealed class SingleOpNode : Node
    {
        public SingleOpNode(Node node, TokenType @operator)
        {
            Node = node;
            Operator = @operator;
        }

        public Node Node { get; }

        public TokenType Operator { get; }

        public override object GetValue(IVariableProvider interpreter)
        {
            object rVal = Node.GetValue(interpreter);



            if (Operator is TokenType.Not)
            {
                if (!(rVal is bool bVal))
                {
                    throw new Exception("Invalid syntax");
                }
                return !bVal;
            }
            if (Operator is TokenType.Subtract)
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
        public DoubleOpNode(Node lNode, TokenType @operator, Node rNode)
        {
            LNode = lNode;
            Operator = @operator;
            RNode = rNode;
        }

        public Node LNode { get; }
        public TokenType Operator { get; }
        public Node RNode { get; }

        public override object GetValue(IVariableProvider interpreter)
        {
            switch (Operator)
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


                    return dlVal % drVal;

                }
                default:
                    throw new Exception("Invalid syntax");
            }
        }
    }




}
