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
# Let's do some math
let x = 2;
let y = x * 2;
let z = 3;
x + y;
";
            try
            {

                var tokens = Tokenizer.Tokenize(new StringReader(expression));

                var expr = Parser.Parse(tokens.Select(t =>
                {
                    //Console.Write(t);
                    return t;
                }));
                //Console.WriteLine();

                var executer = expr.Compile();
                Console.WriteLine(executer.DynamicInvoke());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.GetType()}\n  {ex.Message}");
            }
            Console.WriteLine();
        }
    }
}
