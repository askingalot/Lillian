using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Lillian.Tokenize;

namespace Lillian.Parse
{
    public static class Parser
    {
        /*
            Expr    := Expr Expr*
                     | Sum Semi
                     | Semi
            Sum     := Product
                     | Product SumOp Sum 
            Product := Factor 
                     | Factor ProdOp Product 
            Factor  := ( Sum )
                     | Number
                     | Expr
            Number  := Digit Number 
                     | Digit
            Digit   := "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9"
            SumOp   := + | -
            ProdOp  := * | /
            Semi    := ;

         */

        public static LambdaExpression Parse(IEnumerable<Token> tokenList)
        {
            var tokens = new TokenEnumerator(tokenList);
            return Expression.Lambda(ExprBlock(tokens));
        }

        public static Expression ExprBlock(TokenEnumerator tokens)
        {
            var block = new List<Expression>();
            while (tokens.HasNext)
            {
                block.Add(Expr(tokens));
            }
            return Expression.Block(block);
        }

        public static Expression Expr(TokenEnumerator tokens)
        {
            var savePos = tokens.CreateSavePoint();
            if (!tokens.MoveNext() || tokens.Current is SemiColon)
                return Noop();
            tokens.RevertToSavePoint(savePos);

            var sum = Sum(tokens);
            if (!tokens.MoveNext() || !(tokens.Current is SemiColon))
                throw new ParseException("Expected ';'.");

            return sum;
        }

        public static Expression Sum(TokenEnumerator tokens)
        {
            var product = Product(tokens);

            var savePoint = tokens.CreateSavePoint();
            if (tokens.MoveNext())
            {
                var sumOp = tokens.Current;

                if (sumOp is PlusOp)
                    return Expression.Add(product, Sum(tokens));
                if (sumOp is MinusOp)
                    return Expression.Subtract(product, Sum(tokens));
            }

            tokens.RevertToSavePoint(savePoint);
            return product;
        }

        public static Expression Product(TokenEnumerator tokens)
        {
            var factor = Factor(tokens);
           
            var savePoint = tokens.CreateSavePoint();
            if (tokens.MoveNext())
            {
                var prodOp = tokens.Current;

                if (prodOp is TimesOp)
                    return Expression.Multiply(factor, Product(tokens));
                if (prodOp is DivideOp)
                    return Expression.Divide(factor, Product(tokens));
            }

            tokens.RevertToSavePoint(savePoint);
            return factor;
        }

        public static Expression Factor(TokenEnumerator tokens)
        {
            var savePoint = tokens.CreateSavePoint();
            if (tokens.MoveNext())
            {
                var startToken = tokens.Current;
                if (startToken is OpenParen)
                {
                    var sum = Sum(tokens);
                    if (tokens.MoveNext() && !(tokens.Current is CloseParen))
                        throw new ParseException("Expected ')'");

                    return sum;
                }
            }
            tokens.RevertToSavePoint(savePoint);

            try
            {
                savePoint = tokens.CreateSavePoint();
                return Number(tokens);
            }
            catch
            {
                tokens.RevertToSavePoint(savePoint);
            }

            return Expr(tokens);
        }

        public static Expression Number(IEnumerator<Token> tokens)
        {
            try
            {
                tokens.MoveNext();
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

        public static Expression Noop()
        {
            return (Expression<Action>) (() => NoopFunction());
        }
        private static void NoopFunction() { }
    }
}