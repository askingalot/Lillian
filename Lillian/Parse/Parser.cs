﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Lillian.Lib;
using Lillian.Tokenize;

namespace Lillian.Parse
{
    public static class Parser
    {
        /*
            ExprBlock     := Expr Expr*
            Expr          := NonEmptyExpr Semi
                           | Semi
            NonEmptyExpr  := Call
                           | Binding
                           | String
                           | Boolean
                           | Sum
                           | Comparison
            Call          := Id ( Args )
                           | Id ( ) 
            Args          := NonEmptyExpr, Args
                           | NonEmptyExpr
            Binding       := let Id AssignOp NonEmptyExpr
            Comparison    := NonEmptyExpr EqualOp NonEmptyExpr
            Sum           := Product
                           | Product SumOp Sum 
            Product       := Factor 
                           | Factor ProdOp Product 
            Factor        := ( Sum )
                           | Number
                           | Id
                           | NonEmptyExpr
            Boolean       := true | false
            Number        := Digit Number 
                           | Digit
            Digit         := 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9
            String        := abcd... | ''
            Id            := a-z (a-z | _ | A-Z)*
            SumOp         := + | -
            ProdOp        := * | / | %
            AssignOp      := =
            Semi          := ;

         */

        public static LambdaExpression Parse(IEnumerable<Token> tokenList)
        {
            return Expression.Lambda(ExprBlock(new TokenEnumerator(tokenList)));
        }

        public static BlockExpression ExprBlock(TokenEnumerator tokens)
        {
            var expressions = new List<Expression>();
            while (tokens.HasNext)
            {
                expressions.Add(Expr(tokens));
            }

            var variables = expressions
                .Where(b => b.NodeType == ExpressionType.Assign)
                .Select(b => ((BinaryExpression) b).Left)
                .Cast<ParameterExpression>();

            return Expression.Block(variables, expressions);
        }

        public static Expression Expr(TokenEnumerator tokens)
        {
            if (!tokens.HasNext) 
                throw new ParseException("Unexpected end of input.");

            var expr = NonEmptyExpr(tokens) ?? Util.Noop();

            tokens.MoveNext();
            if (! (tokens.Current is SemiColon))
                throw new ParseException("Expected ';'");

            return expr;
        }

        public static Expression NonEmptyExpr(TokenEnumerator tokens)
        {
            return Call(tokens)
                   ?? Binding(tokens)
                   ?? StringLiteral(tokens)
                   ?? BooleanLiteral(tokens)
                   //?? Comparison(tokens)
                   ?? Sum(tokens);
        }

        public static Expression Call(TokenEnumerator tokens)
        {
            return Util.Transaction(tokens, toks =>
            {
                var id = Identifier(tokens);
                if (id == null) return null;

                toks.MoveNext();
                var startToken = toks.Current;
                if (!(startToken is OpenParen)) return null;

                var args = new List<Expression>();
                var arg = NonEmptyExpr(toks);
                while (arg != null)
                {
                    args.Add(arg);

                    if (toks.Peek() is CloseParen) break;

                    toks.MoveNext();
                    if (!(toks.Current is Comma))
                        throw new ParseException("Expected ','");

                    arg = NonEmptyExpr(toks);
                }

                if (toks.MoveNext() && !(toks.Current is CloseParen))
                    throw new ParseException("Expected ')'");

                var printArgs = Expression.NewArrayInit(
                    typeof (object), args.Select(a => Expression.Convert(a, typeof(object))));

                return Expression.Invoke(id, printArgs);
            });
        }


        public static Expression Binding(TokenEnumerator tokens)
        {
            return Util.Transaction(tokens, toks => {
                tokens.MoveNext();
                var let = tokens.Current as Let;
                if (let == null)
                    return null;

                tokens.MoveNext();
                var id = tokens.Current as Identifier;
                if (id == null) throw new ParseException("Expected valid Identifier in 'let' binding.");

                tokens.MoveNext();
                var assign = tokens.Current as AssignOp;
                if (assign == null) throw new ParseException("Expected '=' in 'let' binding");

                var val = NonEmptyExpr(tokens);

                var variable = Expression.Variable(val.Type, id.Name);
                if (Scope.ContainsKey(id.Name))
                    throw new ParseException($"Identifier '{id.Name}' already bound.");
                Scope.Add(id.Name, variable);

                return Expression.Assign(variable, val);
            });
        }

        public static Expression BooleanLiteral(TokenEnumerator tokens)
        {
            return Util.Transaction(tokens, toks => {
                tokens.MoveNext();
                var booleanLiteral = tokens.Current as BooleanLiteral;
                return booleanLiteral == null
                    ? null
                    : Expression.Constant(booleanLiteral.Value);
            });
        }

        public static Expression Comparison(TokenEnumerator tokens)
        {
            return Util.Transaction(tokens, toks => {
                var leftExpr = NonEmptyExpr(toks);
                if (leftExpr == null) return null;

                toks.MoveNext();
                if (!(toks.Current is EqualOp)) return null;

                var rightExpr = NonEmptyExpr(toks);
                if (rightExpr == null)
                    throw new ParseException("Expected an Expression on right side of comparison");

                return Expression.Equal(leftExpr, rightExpr);
            });
        }

        public static Expression StringLiteral(TokenEnumerator tokens)
        {
            return Util.Transaction(tokens, toks => {
                tokens.MoveNext();
                var strLiteral = tokens.Current as StringLiteral;
                return strLiteral == null
                    ? null
                    : Expression.Constant(strLiteral.Value);
            });
        }

        public static Expression Sum(TokenEnumerator tokens)
        {
            var product = Product(tokens);
            if (product == null)
                return null;

            var sum = Util.Transaction(tokens, toks => {
                tokens.MoveNext();
                var sumOp = tokens.Current;
                if (!(sumOp is Op)) return null;

                var sumVal = Sum(toks);
                if (sumVal == null)
                    throw new ParseException("Expected a numeric expression.");

                if (sumOp is PlusOp)
                    return Expression.Add(product, sumVal);
                if (sumOp is MinusOp)
                    return Expression.Subtract(product, sumVal);

                return null;
                ;
            });

            return sum ?? product;
        }

        public static Expression Product(TokenEnumerator tokens)
        {
            var factor = Factor(tokens);
            if (factor == null)
                return null;

            var product = Util.Transaction(tokens, toks => {
                toks.MoveNext();
                var prodOp = toks.Current;
                if (!(prodOp is Op)) return null;

                var prodVal = Product(toks);
                if (prodVal == null)
                    throw new ParseException("Expected a numeric expression.");

                if (prodOp is TimesOp)
                    return Expression.Multiply(factor, prodVal);
                if (prodOp is DivideOp)
                    return Expression.Divide(factor, prodVal);
                if (prodOp is ModOp)
                    return Expression.Modulo(factor, prodVal);

                return null;
            });

            return product ?? factor;
        }

        public static Expression Factor(TokenEnumerator tokens)
        {
            return MathParenthetical(tokens)
                   ?? Number(tokens)
                   ?? Identifier(tokens)
                   ?? NonEmptyExpr(tokens);
        }

        public static Expression MathParenthetical(TokenEnumerator tokens)
        {
            return Util.Transaction(tokens, toks => {
                tokens.MoveNext();
                var startToken = tokens.Current;
                if (!(startToken is OpenParen))
                    return null;

                var sum = Sum(tokens);
                if (tokens.MoveNext() && !(tokens.Current is CloseParen))
                    throw new ParseException("Expected ')'");

                return sum;
            });
        }

        public static Expression Number(TokenEnumerator tokens)
        {
            return Util.Transaction(tokens, toks => {
                toks.MoveNext();
                var intLiteral = toks.Current as IntLiteral;
                return intLiteral == null 
                    ? null 
                    : Expression.Constant(intLiteral.Value);
            });
        }

        public static Expression Identifier(TokenEnumerator tokens)
        {
            return Util.Transaction(tokens, toks => {
                toks.MoveNext();
                var id = toks.Current as Identifier;
                if (id == null) return null;

                if (! Scope.ContainsKey(id.Name))
                    throw new ParseException($"Identifier, '{toks.Current}', has not been declared");

                return Scope[id.Name];
            });
        }

        public static readonly IDictionary<string, Expression> Scope =
            new Dictionary<string, Expression> {
                { "print", (Expression<ParamsFunc>) (vals => Builtin.Print(vals)) },
                { "println", (Expression<ParamsFunc>) (vals => Builtin.PrintLn(vals)) },
                { "concat", (Expression<ParamsFunc>) (vals => Builtin.Concat(vals)) }
            };

    }
}