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
let z = x + 1;

println('x = ', x);
println('z = ', z);

let y = x * 2;
#let mary = x + y * (x * x - y);
#mary;

#let a = 10 % 8;

let s = 'foo';

println(1);
println('bar', ' ', s, 'blah       ');
#print('bar', ' ', s, 'blah');

let hw = concat('hello,', ' ', 'world!', 1, 2, 3, x);
println(hw);
";
            try
            {
                var tokens = Tokenizer.Tokenize(new StringReader(expression));

                var expr = Parser.Parse(tokens.Select(t =>
                {
/*                    Console.Write(t);
                    if (t is SemiColon)
                        Console.WriteLine();
*/
                    return t;
                }));
                Console.WriteLine();

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
