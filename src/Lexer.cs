using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Aadev.ConditionsInterpreter
{
    internal class Lexer
    {
        private readonly string condition;
        private int index = -1;
        private char? currentChar;

        private bool backsplash = false;
        private bool inString = false;
        private bool inStringBreak = false;


        private static readonly Regex wordRegEx = new Regex("[a-zA-Z0-9_]+", RegexOptions.Compiled);
        private static readonly Regex floatRegEx = new Regex("^[0-9]*([.][0-9]+)?", RegexOptions.Compiled);

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

        public Lexer(string condition)
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
        public Token[] GetTokens()
        {


            List<Token> tokens = new List<Token>();
            while (currentChar != null)
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
                            tokens.Add(new Token(TokenType.String, currentChar.ToString()));
                            NextChar();
                            continue;
                        }

                        tokens.Add(new Token(TokenType.StringEnding));

                        inString = false;
                        NextChar();
                        continue;
                    }
                    tokens.Add(new Token(TokenType.StringBeginning));
                    inString = true;
                    NextChar();

                    CreateStrignContent(tokens);

                    continue;

                }
                if (currentChar is '$' && inString)
                {

                    if (backsplash)
                    {
                        tokens.Add(new Token(TokenType.String, currentChar.ToString()));
                        NextChar();
                        continue;
                    }

                    NextChar();

                    if (!(currentChar is '('))
                        throw new Exception($"Invalid usage of '$' at {index}");



                    tokens.Add(new Token(TokenType.StringEnding));
                    tokens.Add(new Token(TokenType.Add));
                    tokens.Add(new Token(TokenType.LParentheses));

                    inString = false;
                    inStringBreak = true;

                    NextChar();
                    continue;

                }
                if (currentChar is ')' && inStringBreak)
                {
                    tokens.Add(new Token(TokenType.RParentheses));
                    tokens.Add(new Token(TokenType.Add));
                    tokens.Add(new Token(TokenType.StringBeginning));
                    inString = true;
                    inStringBreak = false;
                    NextChar();


                    CreateStrignContent(tokens);


                    continue;
                }


                if (currentChar is ' ' || currentChar is '\n' || currentChar is '\r' || currentChar is '\t')
                {
                    NextChar();
                    continue;
                }
                if (currentChar is '(')
                {
                    tokens.Add(new Token(TokenType.LParentheses));
                    NextChar();
                    continue;
                }
                if (currentChar is ')')
                {
                    tokens.Add(new Token(TokenType.RParentheses));
                    NextChar();
                    continue;
                }
                if (currentChar is '=')
                {
                    NextChar();
                    if (!(currentChar is '='))
                        throw new Exception($"Invalid usgae of symblol '=' at {index}");
                    tokens.Add(new Token(TokenType.Equal));
                    NextChar();
                    continue;
                }
                if (currentChar is '!')
                {
                    NextChar();
                    if (currentChar is '=')
                    {
                        tokens.Add(new Token(TokenType.NotEqual));
                        NextChar();
                        continue;
                    }
                    tokens.Add(new Token(TokenType.Not));
                    continue;
                }
                if (currentChar is '&')
                {
                    NextChar();
                    if (!(currentChar is '&'))
                        throw new Exception($"Invalid usgae of symblol '&' at {index}");
                    tokens.Add(new Token(TokenType.And));
                    NextChar();
                    continue;
                }
                if (currentChar is '|')
                {
                    NextChar();
                    if (!(currentChar is '|'))
                        throw new Exception($"Invalid usgae of symblol '|' at {index}");
                    tokens.Add(new Token(TokenType.Or));
                    NextChar();
                    continue;
                }
                if (currentChar is '^')
                {
                    tokens.Add(new Token(TokenType.Xor));
                    NextChar();
                    continue;
                }
                if (currentChar is '>')
                {
                    tokens.Add(new Token(TokenType.Greater));
                    NextChar();
                    continue;
                }
                if (currentChar is '<')
                {
                    tokens.Add(new Token(TokenType.Less));
                    NextChar();
                    continue;
                }
                if (currentChar is '+')
                {
                    tokens.Add(new Token(TokenType.Add));
                    NextChar();
                    continue;
                }
                if (currentChar is '-')
                {
                    tokens.Add(new Token(TokenType.Subtract));
                    NextChar();
                    continue;
                }
                if (currentChar is '*')
                {
                    tokens.Add(new Token(TokenType.Multiply));
                    NextChar();
                    continue;
                }
                if (currentChar is '/')
                {
                    tokens.Add(new Token(TokenType.Divide));
                    NextChar();
                    continue;
                }
                if (currentChar is '%')
                {
                    tokens.Add(new Token(TokenType.Modulo));
                    NextChar();
                    continue;
                }

                if (char.IsDigit((char)currentChar))
                {
                    StringBuilder numBilder = new StringBuilder();
                    bool isFloating = false;
                    while (currentChar != null && (char.IsDigit((char)currentChar) || currentChar is '.'))
                    {
                        numBilder.Append(currentChar);

                        if (currentChar is '.')
                        {
                            if (isFloating)
                                throw new Exception($"Only one '.' is allowed in numbers at {index}");
                            isFloating = true;
                        }


                        NextChar();
                    }

                    double num = double.Parse(numBilder.ToString().Replace(".", ","));
                    tokens.Add(new Token(TokenType.Number, num));



                    continue;


                }

                Match match = wordRegEx.Match(condition, index);

                if (match.Success)
                {
                    if (match.Value == "true")
                    {
                        tokens.Add(new Token(TokenType.Keyword, Keywords.True));
                        index += 3;
                        NextChar();
                        continue;
                    }
                    if (match.Value == "false")
                    {
                        tokens.Add(new Token(TokenType.Keyword, Keywords.False));
                        index += 4;
                        NextChar();
                        continue;
                    }

                    tokens.Add(new Token(TokenType.Variable, match.Value));
                    index += match.Value.Length - 1;
                    NextChar();
                    continue;

                }

                throw new Exception($"Invalid token: '{currentChar}' at {index}");

            }
            return tokens.ToArray();
        }
    }
}
