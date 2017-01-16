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
            var expression = File.ReadAllText(@"code.LLL");
            try
            {
                var tokens = Tokenizer.Tokenize(new StringReader(expression));

                var expr = new Parser().Parse(new TokenEnumerator(
                        tokens.Select(t => {
/*                          Console.Write(t);
                            if (t is SemiColon)
                                Console.WriteLine();
*/
                            return t;
                        })));
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
