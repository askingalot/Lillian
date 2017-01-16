using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Lillian.Tokenize;

namespace Lillian.Parse
{
    public class Parser
    {
        /*
            ExprBlock     := Expr Expr*
            Expr          := NonEmptyExpr Semi
                           | Semi
            NonEmptyExpr  := Call
                           | Binding
                           | BinOp
            BinOp         := Comparison
            Comparison    := Boolean
                           | Boolean CompOp Boolean
                           | String
                           | String CompOp String
                           : Sum
                           : Sum CompOp Sum
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
            CompOp        := ==
            AssignOp      := =
            Semi          := ;

         */

        public Scope Scope { get; }

        public Parser() : this(Util.ScopeWithBuiltins()) { }
        public Parser(Scope parentScope)
        {
            Scope = new Scope(parentScope);
        }

        public LambdaExpression Parse(TokenEnumerator tokens)
        {
            return Expression.Lambda(ParseBlock(tokens));
        }

        public BlockExpression ParseBlock(TokenEnumerator tokens)
        {
            if (tokens.Peek() is OpenCurly)
            {
                tokens.MoveNext();
            }

            var expressions = new List<Expression>();
            while (tokens.HasNext && !(tokens.Peek() is CloseCurly))
            {
                expressions.Add(Expr(tokens));
            }

            if (tokens.HasNext && tokens.Peek() is CloseCurly)
            {
                tokens.MoveNext();
            }

            var variables = expressions
                .Where(e => e.NodeType == ExpressionType.Assign)
                .Select(e => ((BinaryExpression) e).Left)
                .Cast<ParameterExpression>();

            return Expression.Block(variables, expressions);
        }

        public Expression Expr(TokenEnumerator tokens)
        {
            if (!tokens.HasNext) 
                throw new ParseException("Unexpected end of input.");

            var expr = NonEmptyExpr(tokens) ?? Util.Noop();

            tokens.MoveNext();
            if (! (tokens.Current is SemiColon))
                throw new ParseException("Expected ';'");

            return expr;
        }

        public Expression NonEmptyExpr(TokenEnumerator tokens)
        {
            return Function(tokens)
                   ?? Call(tokens)
                   ?? Binding(tokens)
                   ?? BinaryOperation(tokens);
        }

        private Expression Function(TokenEnumerator tokens)
        {
            return Util.Transaction(tokens, () => {
                tokens.MoveNext();
                if (!(tokens.Current is Fun)) return null;

                tokens.MoveNext();
                if (!(tokens.Current is OpenParen))
                    throw new ParseException("Expected '(' in 'fun' declaration.");

                tokens.MoveNext();
                if (!(tokens.Current is CloseParen))
                    throw new ParseException("Expected ')' in 'fun' declaration.");

                var funParser = new Parser(parentScope: Scope);
                var fun = funParser.Parse(tokens);
                return fun;
            });
        }

        public Expression Call(TokenEnumerator tokens)
        {
            return Util.Transaction(tokens, () => {
                var id = Identifier(tokens);
                if (id == null) return null;

                tokens.MoveNext();
                if (!(tokens.Current is OpenParen)) return null;

                var fun = id as LambdaExpression;
                if (fun == null)
                    throw new ParseException("Identifier is not invokable.");

                var args = new List<Expression>();
                if (tokens.Peek() is CloseParen)
                {
                    tokens.MoveNext();
                }
                else
                {
                    do
                    {
                        var arg = NonEmptyExpr(tokens);
                        args.Add(arg);

                        tokens.MoveNext();
                        if (!(tokens.Current is Comma || tokens.Current is CloseParen))
                            throw new ParseException("Expected ',' or ')'");
                    } while (!(tokens.Current is CloseParen));
                }

                return fun.Parameters.Count > 0
                    ? Expression.Invoke(id, 
                        Expression.NewArrayInit(typeof (object), args.Select(a => Expression.Convert(a, typeof (object)))))
                    : Expression.Invoke(id);
            });
        }


        public Expression Binding(TokenEnumerator tokens)
        {
            return Util.Transaction(tokens, () => {
                tokens.MoveNext();
                var let = tokens.Current as Let;
                if (let == null)
                    return null;

                tokens.MoveNext();
                var id = tokens.Current as Identifier;
                if (id == null)
                    throw new ParseException("Expected valid Identifier in 'let' binding.");

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

        public Expression BinaryOperation(TokenEnumerator tokens)
        {
            return Comparison(tokens);
        }

        public Expression Comparison(TokenEnumerator tokens)
        {
            var lhs = BooleanLiteral(tokens)
                      ?? StringLiteral(tokens)
                      ?? Sum(tokens);
            if (lhs == null) return null;

            var comp = Util.Transaction(tokens, () => {
                tokens.MoveNext();
                var compOp = tokens.Current;
                if (!(compOp is Op)) return null;

                var rhs = Comparison(tokens);
                if (rhs == null)
                    throw new ParseException("Expected right hand side of comparison");

                if (compOp is EqualOp)
                    return Expression.Equal(lhs, rhs);
                if (compOp is NotEqualOp)
                    return Expression.NotEqual(lhs, rhs);
                if (compOp is GreaterThanOrEqualOp)
                    return Expression.GreaterThanOrEqual(lhs, rhs);
                if (compOp is LesserThanOrEqualOp)
                    return Expression.LessThanOrEqual(lhs, rhs);
                if (compOp is Greater)
                    return Expression.GreaterThan(lhs, rhs);
                if (compOp is Lesser)
                    return Expression.LessThan(lhs, rhs);

                return null;
            });

            return comp ?? lhs;
        }

        public Expression Sum(TokenEnumerator tokens)
        {
            var lhs = Product(tokens);
            if (lhs == null) return null;

            var sum = Util.Transaction(tokens, () => {
                tokens.MoveNext();
                var sumOp = tokens.Current;
                if (!(sumOp is Op)) return null;

                var rhs = Sum(tokens);
                if (rhs == null)
                    throw new ParseException("Expected a numeric expression.");

                if (sumOp is PlusOp)
                    return Expression.Add(lhs, rhs);
                if (sumOp is MinusOp)
                    return Expression.Subtract(lhs, rhs);

                return null;
            });

            return sum ?? lhs;
        }

        public Expression Product(TokenEnumerator tokens)
        {
            var lhs = Factor(tokens);
            if (lhs == null)
                return null;

            var product = Util.Transaction(tokens, () => {
                tokens.MoveNext();
                var prodOp = tokens.Current;
                if (!(prodOp is Op)) return null;

                var rhs = Product(tokens);
                if (rhs == null)
                    throw new ParseException("Expected a numeric expression.");

                if (prodOp is TimesOp)
                    return Expression.Multiply(lhs, rhs);
                if (prodOp is DivideOp)
                    return Expression.Divide(lhs, rhs);
                if (prodOp is ModOp)
                    return Expression.Modulo(lhs, rhs);

                return null;
            });

            return product ?? lhs;
        }

        public Expression Factor(TokenEnumerator tokens)
        {
            return MathParenthetical(tokens)
                   ?? Number(tokens)
                   ?? Identifier(tokens)
                   ?? NonEmptyExpr(tokens);
        }

        public Expression MathParenthetical(TokenEnumerator tokens)
        {
            return Util.Transaction(tokens, () => {
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

        public Expression Number(TokenEnumerator tokens)
        {
            return Util.Transaction(tokens, () => {
                tokens.MoveNext();
                var intLiteral = tokens.Current as IntLiteral;
                return intLiteral == null 
                    ? null 
                    : Expression.Constant(intLiteral.Value);
            });
        }

        public Expression Identifier(TokenEnumerator tokens)
        {
            return Util.Transaction(tokens, () => {
                tokens.MoveNext();
                var id = tokens.Current as Identifier;
                if (id == null) return null;

                if (! Scope.ContainsKey(id.Name))
                    throw new ParseException($"Identifier, '{tokens.Current}', has not been declared");

                return Scope[id.Name];
            });
        }

        public Expression StringLiteral(TokenEnumerator tokens)
        {
            return Util.Transaction(tokens, () => {
                tokens.MoveNext();
                var strLiteral = tokens.Current as StringLiteral;
                return strLiteral == null
                    ? null
                    : Expression.Constant(strLiteral.Value);
            });
        }

        public Expression BooleanLiteral(TokenEnumerator tokens)
        {
            return Util.Transaction(tokens, () => {
                tokens.MoveNext();
                var booleanLiteral = tokens.Current as BooleanLiteral;
                return booleanLiteral == null
                    ? null
                    : Expression.Constant(booleanLiteral.Value);
            });
        }
    }
}