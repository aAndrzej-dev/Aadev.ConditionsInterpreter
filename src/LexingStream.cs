using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Aadev.ConditionsInterpreter
{
    internal class LexingStream
    {
        private readonly string condition;
        private int index = -1;
        private char? currentChar;

        private bool backsplash = false;
        private bool inString = false;
        private bool inStringBreak = false;
        private readonly List<Token> buffer = new List<Token>();


        private static readonly Regex wordRegEx = new Regex("[a-zA-Z0-9_]+", RegexOptions.Compiled);
        private static readonly Regex floatRegEx = new Regex("[0-9]+([.][0-9]+)?", RegexOptions.Compiled);

        [DebuggerStepThrough]
        private char? NextChar()
        {
            index++;

            if (condition.Length <= index)
                currentChar = null;
            else
                currentChar = condition[index];



            return currentChar;
        }

        public LexingStream(string condition)
        {
            this.condition = condition;
            NextChar();
        }
        private void CreateStrignContent(List<Token> tokens)
        {
            StringBuilder sb = new StringBuilder();

            bool bs;
            bool end = false;
            bool cont = false;
            while (true)
            {
                if (currentChar is '\\')
                {
                    bs = true;
                    NextChar();
                    if (currentChar is null)
                        throw new Exception($"Invalid usage of char '\\' at {index}");
                }
                else
                {
                    bs = false;
                }

                if ((currentChar is '\'' || currentChar is '\"') && !bs)
                {
                    end = true;
                    NextChar();
                    break;

                }
                if (currentChar is '$' && !bs)
                {
                    cont = true;
                    break;
                }

                sb.Append(currentChar);
                NextChar();
            }
            if (sb.Length > 0)
                tokens.Add(new Token(TokenType.String, sb.ToString()));
            if (cont)
                return;
            if (end)
            {


                tokens.Add(new Token(TokenType.StringEnding));

                inString = false;
            }
        }

        public Token? NextToken()
        {
            if (currentChar is null && buffer.Count is 0)
                return null;

            while (buffer.Count == 0 && currentChar != null)
            {
                NextMove();
            }

            Token element = buffer[0];

            buffer.RemoveAt(0);

            return element;

        }




        private void NextMove()
        {
            if (currentChar is '\\')
            {
                if (!inString)
                    throw new Exception($"Invalid usage of char '\\' at {index}");
                backsplash = true;
                NextChar();
                if (currentChar is null)
                    throw new Exception($"Invalid usage of char '\\' at {index}");
            }
            else
            {
                backsplash = false;
            }

            if (currentChar is '\'' || currentChar is '"')
            {
                if (inString)
                {
                    if (backsplash)
                    {
                        buffer.Add(new Token(TokenType.String, currentChar.ToString()));
                        NextChar();
                        return;
                    }

                    buffer.Add(new Token(TokenType.StringEnding));

                    inString = false;
                    NextChar();
                    return;
                }
                buffer.Add(new Token(TokenType.StringBeginning));
                inString = true;
                NextChar();

                CreateStrignContent(buffer);

                return;

            }
            if (currentChar is '$' && inString)
            {

                if (backsplash)
                {
                    buffer.Add(new Token(TokenType.String, currentChar.ToString()));
                    NextChar();
                    return;
                }

                NextChar();

                if (!(currentChar is '('))
                    throw new Exception($"Invalid usage of '$' at {index}");



                buffer.Add(new Token(TokenType.StringEnding));
                buffer.Add(new Token(TokenType.Add));
                buffer.Add(new Token(TokenType.LParentheses));

                inString = false;
                inStringBreak = true;

                NextChar();
                return;

            }
            if (currentChar is ')' && inStringBreak)
            {
                buffer.Add(new Token(TokenType.RParentheses));
                buffer.Add(new Token(TokenType.Add));
                buffer.Add(new Token(TokenType.StringBeginning));
                inString = true;
                inStringBreak = false;
                NextChar();


                CreateStrignContent(buffer);


                return;
            }


            if (currentChar is ' ' || currentChar is '\n' || currentChar is '\r' || currentChar is '\t')
            {
                NextChar();
                return;
            }
            if (currentChar is '(')
            {
                buffer.Add(new Token(TokenType.LParentheses));
                NextChar();
                return;
            }
            if (currentChar is ')')
            {
                buffer.Add(new Token(TokenType.RParentheses));
                NextChar();
                return;
            }
            if (currentChar is '=')
            {
                NextChar();
                if (!(currentChar is '='))
                    throw new Exception($"Invalid usgae of symblol '=' at {index}");
                buffer.Add(new Token(TokenType.Equal));
                NextChar();
                return;
            }
            if (currentChar is '!')
            {
                NextChar();
                if (currentChar is '=')
                {
                    buffer.Add(new Token(TokenType.NotEqual));
                    NextChar();
                    return;
                }
                buffer.Add(new Token(TokenType.Not));
                return;
            }
            if (currentChar is '&')
            {
                NextChar();
                if (!(currentChar is '&'))
                    throw new Exception($"Invalid usgae of symblol '&' at {index}");
                buffer.Add(new Token(TokenType.And));
                NextChar();
                return;
            }
            if (currentChar is '|')
            {
                NextChar();
                if (!(currentChar is '|'))
                    throw new Exception($"Invalid usgae of symblol '|' at {index}");
                buffer.Add(new Token(TokenType.Or));
                NextChar();
                return;
            }
            if (currentChar is '^')
            {
                buffer.Add(new Token(TokenType.Xor));
                NextChar();
                return;
            }
            if (currentChar is '>')
            {
                buffer.Add(new Token(TokenType.Greater));
                NextChar();
                return;
            }
            if (currentChar is '<')
            {
                buffer.Add(new Token(TokenType.Less));
                NextChar();
                return;
            }
            if (currentChar is '+')
            {
                buffer.Add(new Token(TokenType.Add));
                NextChar();
                return;
            }
            if (currentChar is '-')
            {
                buffer.Add(new Token(TokenType.Subtract));
                NextChar();
                return;
            }
            if (currentChar is '*')
            {
                buffer.Add(new Token(TokenType.Multiply));
                NextChar();
                return;
            }
            if (currentChar is '/')
            {
                buffer.Add(new Token(TokenType.Divide));
                NextChar();
                return;
            }
            if (currentChar is '%')
            {
                buffer.Add(new Token(TokenType.Modulo));
                NextChar();
                return;
            }

            Match floatMatch = floatRegEx.Match(condition, index);

            if (floatMatch.Success)
            {
                buffer.Add(new Token(TokenType.Number, double.Parse(floatMatch.Value.Replace('.', ','))));

                index += floatMatch.Value.Length - 1;
                NextChar();
                return;
            }


            Match match = wordRegEx.Match(condition, index);

            if (match.Success)
            {
                if (match.Value == "true")
                {
                    buffer.Add(new Token(TokenType.Keyword, Keywords.True));
                    index += 3;
                    NextChar();
                    return;
                }
                if (match.Value == "false")
                {
                    buffer.Add(new Token(TokenType.Keyword, Keywords.False));
                    index += 4;
                    NextChar();
                    return;
                }

                buffer.Add(new Token(TokenType.Variable, match.Value));
                index += match.Value.Length - 1;
                NextChar();
                return;

            }

            throw new Exception($"Invalid token: '{currentChar}' at {index}");

        }
    }
}
