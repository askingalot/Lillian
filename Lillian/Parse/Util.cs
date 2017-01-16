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
            Func<Expression> expressionProducer)
        {
            Expression resultExpression = null;
            var savePoint = tokens.CreateSavePoint();
            try
            {
                resultExpression = expressionProducer();
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

            //TODO: Look into using the parameters array to detrmine which 
            // Func<T1, T2,...> to use.
            //builtins.First().GetParameters().First().

            var initialScope = builtins.ToDictionary<MethodInfo, string, Expression>(
                builtin => builtin.Name.ToCamelCase(), 
                builtin => (Expression<ParamsFunc>) 
                    (vals => builtin.Invoke(null, new object[] {vals})));

            return new Scope(initialScope);
        } 
    }
}