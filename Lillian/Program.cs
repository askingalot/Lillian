using System;
using System.IO;
using System.Linq;
using Lillian.Parse;
using Lillian.Tokenize;

namespace Lillian
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var expression = @"
let a = 2;
2 + a;
";
            var tokens = Tokenizer.Tokenize(new StringReader(expression));
            /*
            foreach (var token in tokens)
            {
                Console.Write(token);
            }
            Console.WriteLine();
            */

            var expr = Parser.Parse(tokens.Select(t => {
                Console.Write(t);
                return t;
            }));
            Console.WriteLine();

            var executer = expr.Compile();
            Console.WriteLine(executer.DynamicInvoke());
        }
    }
}
