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
2 + 2 * 
+4; 
1 + -1; # 1 + 1
# foo
-200;
";
            var tokens = Tokenizer.Tokenize(new StringReader(expression));

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
