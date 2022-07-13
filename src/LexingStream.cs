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
        private readonly Token[] buffer = new Token[5];
        private char? currentChar;
        private int bufferLenght = 0;
        private int index = -1;
        private int outIndex = 0;
        private bool backsplash = false;
        private bool inString = false;
        private bool inStringBreak = false;


        

        private static readonly Regex wordRegEx = new Regex("[a-zA-Z_]+[a-zA-Z0-9_]*", RegexOptions.Compiled);
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
        private void CreateStrignContent(Token[] tokens, ref int index)
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
                tokens[index++] = new Token(TokenType.String, sb.ToString());
            if (cont)
                return;
            if (end)
            {


                tokens[index++] = new Token(TokenType.StringEnding);

                inString = false;
            }
        }

        public Token? NextToken()
        {
            if (currentChar is null && bufferLenght <= outIndex)
                return null;

            while (bufferLenght <= outIndex && currentChar != null)
            {
                bufferLenght = NextMove();
                outIndex = 0;
            }

            Token element = buffer[outIndex++];

            return element;

        }




        private int NextMove()
        {
            int inIndex = 0;
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
                        buffer[inIndex++] = new Token(TokenType.String, currentChar.ToString());
                        NextChar();
                        return inIndex;
                    }

                    buffer[inIndex++] = new Token(TokenType.StringEnding);

                    inString = false;
                    NextChar();
                    return inIndex;
                }
                buffer[inIndex++] = new Token(TokenType.StringBeginning);
                inString = true;
                NextChar();

                CreateStrignContent(buffer, ref inIndex);

                return inIndex;

            }
            if (currentChar is '$' && inString)
            {

                if (backsplash)
                {
                    buffer[inIndex++] = new Token(TokenType.String, currentChar.ToString());
                    NextChar();
                    return inIndex;
                }

                NextChar();

                if (!(currentChar is '('))
                    throw new Exception($"Invalid usage of '$' at {index}");



                buffer[inIndex++] = new Token(TokenType.StringEnding);
                buffer[inIndex++] = new Token(TokenType.Add);
                buffer[inIndex++] = new Token(TokenType.LParentheses);

                inString = false;
                inStringBreak = true;

                NextChar();
                return inIndex;

            }
            if (currentChar is ')' && inStringBreak)
            {
                buffer[inIndex++] = new Token(TokenType.RParentheses);
                buffer[inIndex++] = new Token(TokenType.Add);
                buffer[inIndex++] = new Token(TokenType.StringBeginning);
                inString = true;
                inStringBreak = false;
                NextChar();


                CreateStrignContent(buffer, ref inIndex);


                return inIndex;
            }


            if (currentChar is ' ' || currentChar is '\n' || currentChar is '\r' || currentChar is '\t')
            {
                NextChar();
                return inIndex;
            }
            if (currentChar is '(')
            {
                buffer[inIndex++] = new Token(TokenType.LParentheses);
                NextChar();
                return inIndex;
            }
            if (currentChar is ')')
            {
                buffer[inIndex++] = new Token(TokenType.RParentheses);
                NextChar();
                return inIndex;
            }
            if (currentChar is '=')
            {
                NextChar();
                if (!(currentChar is '='))
                    throw new Exception($"Invalid usgae of symblol '=' at {index}");
                buffer[inIndex++] = new Token(TokenType.Equal);
                NextChar();
                return inIndex;
            }
            if (currentChar is '!')
            {
                NextChar();
                if (currentChar is '=')
                {
                    buffer[inIndex++] = new Token(TokenType.NotEqual);
                    NextChar();
                    return inIndex;
                }
                buffer[inIndex++] = new Token(TokenType.Not);
                return inIndex;
            }
            if (currentChar is '&')
            {
                NextChar();
                if (!(currentChar is '&'))
                    throw new Exception($"Invalid usgae of symblol '&' at {index}");
                buffer[inIndex++] = new Token(TokenType.And);
                NextChar();
                return inIndex;
            }
            if (currentChar is '|')
            {
                NextChar();
                if (!(currentChar is '|'))
                    throw new Exception($"Invalid usgae of symblol '|' at {index}");
                buffer[inIndex++] = new Token(TokenType.Or);
                NextChar();
                return inIndex;
            }
            if (currentChar is '^')
            {
                buffer[inIndex++] = new Token(TokenType.Xor);
                NextChar();
                return inIndex;
            }
            if (currentChar is '>')
            {
                buffer[inIndex++] = new Token(TokenType.Greater);
                NextChar();
                return inIndex;
            }
            if (currentChar is '<')
            {
                buffer[inIndex++] = new Token(TokenType.Less);
                NextChar();
                return inIndex;
            }
            if (currentChar is '+')
            {
                buffer[inIndex++] = new Token(TokenType.Add);
                NextChar();
                return inIndex;
            }
            if (currentChar is '-')
            {
                buffer[inIndex++] = new Token(TokenType.Subtract);
                NextChar();
                return inIndex;
            }
            if (currentChar is '*')
            {
                buffer[inIndex++] = new Token(TokenType.Multiply);
                NextChar();
                return inIndex;
            }
            if (currentChar is '/')
            {
                buffer[inIndex++] = new Token(TokenType.Divide);
                NextChar();
                return inIndex;
            }
            if (currentChar is '%')
            {
                buffer[inIndex++] = new Token(TokenType.Modulo);
                NextChar();
                return inIndex;
            }



            Match floatMatch = floatRegEx.Match(condition, index);

            if (floatMatch.Success && floatMatch.Index == index)
            {
                buffer[inIndex++] = new Token(TokenType.Number, double.Parse(floatMatch.Value.Replace('.', ',')));

                index += floatMatch.Value.Length - 1;
                NextChar();
                return inIndex;
            }


            Match wordMatch = wordRegEx.Match(condition, index);

            if (wordMatch.Success && wordMatch.Index == index)
            {
                
                if (wordMatch.Value == "true")
                {
                    buffer[inIndex++] = new Token(TokenType.Keyword, Keywords.True);
                    index += 3;
                    NextChar();
                    return inIndex;
                }
                if (wordMatch.Value == "false")
                {
                    buffer[inIndex++] = new Token(TokenType.Keyword, Keywords.False);
                    index += 4;
                    NextChar();
                    return inIndex;
                }

                buffer[inIndex++] = new Token(TokenType.Variable, wordMatch.Value);
                index += wordMatch.Value.Length - 1;
                NextChar();
                return inIndex;

            }



          

            throw new Exception($"Invalid token: '{currentChar}' at {index}");

        }
    }
}
