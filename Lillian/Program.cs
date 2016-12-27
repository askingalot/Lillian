using System;
using Lillian.Parse;
using Lillian.Tokenize;

namespace Lillian
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var expression = "2 + (2 / 2);";
            var tokens = Tokenizer.Tokenize(expression);
            foreach (var token in tokens)
            {
                Console.Write(token);
            }
            Console.WriteLine();

            var expr = Parser.Parse(tokens);
            var executer = expr.Compile();
            Console.WriteLine(executer.DynamicInvoke());
        }
    }
}
