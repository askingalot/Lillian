using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Lillian.Lib;
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
        public Parser()
        {
            Scope = ScopeWithBuiltins();
        }

        public LambdaExpression Parse(IEnumerable<Token> tokenList)
        {
            return Expression.Lambda(ExprBlock(new TokenEnumerator(tokenList)));
        }

        public BlockExpression ExprBlock(TokenEnumerator tokens)
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
            return Call(tokens)
                   ?? Binding(tokens)
                   ?? BinaryOperation(tokens);
        }

        public Expression Call(TokenEnumerator tokens)
        {
            return Util.Transaction(tokens, toks =>
            {
                var id = Identifier(tokens);
                if (id == null) return null;

                if (!(toks.Peek() is OpenParen)) return null;

                var args = new List<Expression>();
                toks.MoveNext();
                while (!(toks.Current is CloseParen))
                {
                    var arg = NonEmptyExpr(toks);
                    args.Add(arg);

                    toks.MoveNext();
                    if (!(toks.Current is Comma || toks.Current is CloseParen))
                        throw new ParseException("Expected ',' or ')'");
                }

                var printArgs = Expression.NewArrayInit(
                    typeof (object), args.Select(a => Expression.Convert(a, typeof(object))));

                return Expression.Invoke(id, printArgs);
            });
        }


        public Expression Binding(TokenEnumerator tokens)
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

            var comp = Util.Transaction(tokens, toks =>
            {
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

            var sum = Util.Transaction(tokens, toks => {
                tokens.MoveNext();
                var sumOp = tokens.Current;
                if (!(sumOp is Op)) return null;

                var rhs = Sum(toks);
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

            var product = Util.Transaction(tokens, toks => {
                toks.MoveNext();
                var prodOp = toks.Current;
                if (!(prodOp is Op)) return null;

                var rhs = Product(toks);
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

        public Expression Number(TokenEnumerator tokens)
        {
            return Util.Transaction(tokens, toks => {
                toks.MoveNext();
                var intLiteral = toks.Current as IntLiteral;
                return intLiteral == null 
                    ? null 
                    : Expression.Constant(intLiteral.Value);
            });
        }

        public Expression Identifier(TokenEnumerator tokens)
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

        public Expression StringLiteral(TokenEnumerator tokens)
        {
            return Util.Transaction(tokens, toks => {
                tokens.MoveNext();
                var strLiteral = tokens.Current as StringLiteral;
                return strLiteral == null
                    ? null
                    : Expression.Constant(strLiteral.Value);
            });
        }

        public Expression BooleanLiteral(TokenEnumerator tokens)
        {
            return Util.Transaction(tokens, toks => {
                tokens.MoveNext();
                var booleanLiteral = tokens.Current as BooleanLiteral;
                return booleanLiteral == null
                    ? null
                    : Expression.Constant(booleanLiteral.Value);
            });
        }



        private Scope ScopeWithBuiltins()
        {
            var builtins = typeof (Builtin).GetMethods(BindingFlags.Public | BindingFlags.Static);
            var initialScope = builtins.ToDictionary<MethodInfo, string, Expression>(
                builtin => builtin.Name.ToCamelCase(), 
                builtin => (Expression<ParamsFunc>) 
                    (vals => builtin.Invoke(null, BindingFlags.Public | BindingFlags.Static, null, new object[] {vals}, null)));

            return new Scope(initialScope);
        } 
    }
}