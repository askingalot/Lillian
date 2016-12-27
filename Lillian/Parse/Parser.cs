using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Lillian.Tokenize;

namespace Lillian.Parse
{
    public static class Parser
    {
        /*
            Expression := Expression Operator Expression | Number
            Operator   := "+" | "-"
            Number     := Digit Number | Digit
            Digit      := "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9"
         */

        public static LambdaExpression Parse(IEnumerable<Token> tokenList)
        {
            var tokens = new TokenEnumerator(tokenList);
            tokens.MoveNext();
            return Expression.Lambda(Expr(tokens));
        }

        public static Expression Expr(IEnumerator<Token> tokens)
        {
            Expression num1;
            if (tokens.Current is LeftParen)
            {
                tokens.MoveNext();
                num1 = Expr(tokens);
            }
            else
            {
                num1 = Number(tokens);
            }

            if (!tokens.MoveNext())
                return num1;

            while (tokens.Current is Op)
            {
                var op = tokens.Current;
                tokens.MoveNext();

                Expression num2;
                if (tokens.Current is LeftParen)
                {
                    tokens.MoveNext(); // skip (
                    num2 = Expr(tokens);
                }
                else
                {
                    num2 = Number(tokens);
                }

                if (op is PlusOp)
                {
                    num1 = Expression.Add(num1, num2);
                }
                else if (op is MinusOp)
                {
                    num1 = Expression.Subtract(num1, num2);
                }
                else if (op is TimesOp)
                {
                    num1 = Expression.Multiply(num1, num2);
                }
                else if (op is DivideOp)
                {
                    num1 = Expression.Divide(num1, num2);
                }
                else
                {
                    throw new ParseException($"Unknown Operator: {op}");
                }

                if (!tokens.MoveNext())
                {
                    return num1;
                }
            }

            return num1;
        }

        public static Expression Number(IEnumerator<Token> tokens)
        {
            try
            {
                return Expression.Constant(((IntConstant) tokens.Current).Value);
            }
            catch (InvalidCastException)
            {
                throw new ParseException($"Expected Number, but got: {tokens.Current}");
            }
            catch
            {
                throw new ParseException("Expected Number, but got an unexpected error");
            }
        }
    }
}