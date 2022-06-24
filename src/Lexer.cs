using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
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


        private char? NextChar()
        {
            index++;
            currentChar = condition.Length <= index ? null : (char?)condition[index];
            return currentChar;
        }

        public Lexer(string condition)
        {
            this.condition = condition;
            NextChar();
        }

        public Token[] GetTokens()
        {
            List<Token> tokens = new List<Token>();
            while (currentChar != null)
            {
                if (currentChar is '\\')
                {
                    if (!inString)
                        throw new Exception("Invalid usage of char '\\'");
                    backsplash = true;
                    NextChar();
                    if (currentChar is null)
                        throw new Exception("Invalid usage of char '\\'");
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
                            tokens.Add(new Token(TokenType.Literal, currentChar));
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
                    continue;

                }
                if (currentChar is '$')
                {
                    if (backsplash)
                    {
                        tokens.Add(new Token(TokenType.Literal, currentChar));
                        NextChar();
                        continue;
                    }

                    NextChar();

                    if (!(currentChar is '('))
                        throw new Exception("Invalid usage of '$'");
                    StringBuilder varBilder = new StringBuilder();

                    NextChar();

                    while (char.IsLetter((char)currentChar!))
                    {
                        varBilder.Append(currentChar);
                        NextChar();


                    }
                    if (varBilder.Length is 0)
                        throw new Exception("Invalid variable; Var lenght must be longer than 0");

                    if (!(currentChar is ')'))
                        throw new Exception("Missing ')' in variable declaration");

                    tokens.Add(new Token(TokenType.Variable, varBilder.ToString()));
                    NextChar();
                    continue;
                }

                if (inString)
                {
                    tokens.Add(new Token(TokenType.Literal, currentChar));
                    NextChar();
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
                        throw new Exception("Invalid usgae of symblol '='");
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
                        throw new Exception("Invalid usgae of symblol '&'");
                    tokens.Add(new Token(TokenType.And));
                    NextChar();
                    continue;
                }
                if (currentChar is '|')
                {
                    NextChar();
                    if (!(currentChar is '|'))
                        throw new Exception("Invalid usgae of symblol '|'");
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
                if (char.IsDigit((char)currentChar))
                {
                    StringBuilder numBilder = new StringBuilder();
                    bool isFloating = false;
                    while (currentChar != null && (char.IsDigit((char)currentChar) || currentChar is '.'))
                    {
                        numBilder.Append(currentChar);

                        if (currentChar is '.')
                        {
                            isFloating = true;
                        }


                        NextChar();
                    }
                    if (isFloating)
                    {
                        double num = double.Parse(numBilder.ToString().Replace(".", ","));
                        tokens.Add(new Token(TokenType.Float, num));
                    }
                    else
                    {
                        BigInteger num = BigInteger.Parse(numBilder.ToString());
                        tokens.Add(new Token(TokenType.Int, num));
                    }


                    continue;


                }
                if (string.Equals(condition.AsSpan(index, 4).ToString(), "true"))
                {
                    tokens.Add(new Token(TokenType.Bool, true));
                    index += 3;
                    NextChar();
                    continue;

                }
                if (string.Equals(condition.AsSpan(index, 5).ToString(), "false"))
                {
                    tokens.Add(new Token(TokenType.Bool, false));
                    index += 4;
                    NextChar();
                    continue;

                }

                throw new Exception($"Invalid token: '{currentChar}'");

            }
            return tokens.ToArray();
        }
    }
}
