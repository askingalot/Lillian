﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Lillian.Tokenize;

namespace Lillian.Parse
{
    public static class Parser
    {
        /*
            ExprBlock     := Expr Expr*
            Expr          := NonEmptyExpr
                           | Semi
            NonEmptyExpr  := Binding Semi
                           | Sum Semi
            Binding       := "let" Id AssignOp Expr
            Sum           := Product
                           | Product SumOp Sum 
            Product       := Factor 
                           | Factor ProdOp Product 
            Factor        := ( Sum )
                           | Number
                           | Expr
            Number        := Digit Number 
                           | Digit
            Digit         := "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9"
            Id            := "a"-"z" ("a"-"z" | _ | "A-Z")*
            SumOp         := + | -
            ProdOp        := * | /
            AssignOp      := =
            Semi          := ;

         */

        public static LambdaExpression Parse(IEnumerable<Token> tokenList)
        {
            var tokens = new TokenEnumerator(tokenList);
            var block = ExprBlock(tokens);

            return Expression.Lambda(block, block.Variables);
        }

        public static BlockExpression ExprBlock(TokenEnumerator tokens)
        {
            var block = new List<Expression>();
            while (tokens.HasNext)
            {
                block.Add(Expr(tokens));
            }

            var variables =
                block.Where(b => b.NodeType == ExpressionType.Assign)
                    .Select(b => ((BinaryExpression) b).Left)
                    .Cast<ParameterExpression>();
            return Expression.Block(variables, block);
        }

        public static Expression Expr(TokenEnumerator tokens)
        {
            if (!tokens.HasNext) 
                throw new ParseException("Unexpected end of input.");

            var savePoint = tokens.CreateSavePoint();
            tokens.MoveNext();
            if (tokens.Current is SemiColon)
                return Noop();
            tokens.RevertToSavePoint(savePoint);

            return NonEmptyExpr(tokens);
        }

        public static Expression NonEmptyExpr(TokenEnumerator tokens)
        {
            if (tokens.Peek() is Let)
            {
                return Binding(tokens);
            }

            var sum = Sum(tokens);
            tokens.MoveNext();
            if (!(tokens.Current is SemiColon))
                throw new ParseException("Expected ';'.");

            return sum;
        }

        public static Expression Binding(TokenEnumerator tokens)
        {
            tokens.MoveNext();
            var let = tokens.Current as Let;
            tokens.MoveNext();
            var id = tokens.Current as Identifier;
            tokens.MoveNext();
            var assign = tokens.Current as AssignOp;
            var val = NonEmptyExpr(tokens);

            return Expression.Assign(
                Expression.Variable(typeof (int), id.Name), val);
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

            if (tokens.Peek() is IntConstant)
                return Number(tokens);
            if (tokens.Peek() is Identifier)
            {
                tokens.MoveNext();
                var id = (Identifier) tokens.Current;
                return Expression.Variable(typeof (int), id.Name);
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