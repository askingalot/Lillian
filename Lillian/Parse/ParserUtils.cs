using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Lillian.Lib;
using Lillian.Tokenize;

namespace Lillian.Parse
{
    public static class Util
    {
        private static void NoopFunction() { }
        /// <summary>
        ///  Create a "no-op" Expression
        /// </summary>
        /// <returns>An Expression(OF Action) that does nothing</returns>
        public static Expression Noop()
        {
            return (Expression<Action>) (() => NoopFunction());
        }

        /// <summary>
        ///  Wraps a call to expressionProducer in a 
        ///   tokens.CreateSavePoint() / tokens.RevertToSavePoint block.
        ///  Reverts to Save Point if expressionProducer() 
        ///   returns null or throws an exception.
        /// </summary>
        /// <param name="tokens">The token enumerator usine by expressionProducer()</param>
        /// <param name="expressionProducer">
        ///  Function that produces an expression or null
        /// </param>
        /// <returns>The return value of the expressionProducer()</returns>
        public static Expression Transaction(TokenEnumerator tokens, 
            Func<TokenEnumerator, Expression> expressionProducer)
        {
            Expression resultExpression = null;
            var savePoint = tokens.CreateSavePoint();
            try
            {
                resultExpression = expressionProducer(tokens);
            }
            finally 
            {
                if (resultExpression == null)
                {
                    tokens.RevertToSavePoint(savePoint);
                }
            }
            return resultExpression;
        }

        public static Scope ScopeWithBuiltins()
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