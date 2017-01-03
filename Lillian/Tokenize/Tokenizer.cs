using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Lillian.Tokenize
{
    public static class Tokenizer
    {
        public static readonly Regex Whitespace = new Regex(@"^\s+");
        public static readonly Regex Comment = new Regex(@"^#.*$");
        public static readonly Regex Integer = new Regex(@"^[+\-]?\d+");
        public static readonly Regex Operator = new Regex(@"^[+\-*/%=]");
        public static readonly Regex Symbol = new Regex(@"^[;()]");
        public static readonly Regex Keyword = 
            new Regex($@"^{string.Join("|", "let")}");
        public static readonly Regex Identifer = new Regex(@"^[_a-z]([_a-zA-Z0-9])*");

        public static IEnumerable<Token> Tokenize(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                while (! string.IsNullOrWhiteSpace(line))
                {
                    Match match;
                    if ((match = Whitespace.Match(line)).Success)
                    {
                        // Skip white space
                    }
                    else if ((match = Comment.Match(line)).Success)
                    {
                        // Skip Comments too
                    }
                    else if ((match = Integer.Match(line)).Success)
                    {
                        yield return new IntConstant(match.Value);
                    }
                    else if ((match = Operator.Match(line)).Success)
                    {
                        yield return GetOp(match.Value);
                    }
                    else if ((match = Symbol.Match(line)).Success)
                    {
                        yield return GetSymbol(match.Value);
                    }
                    else if ((match = Keyword.Match(line)).Success)
                    {
                        yield return GetKeyword(match.Value);
                    }
                    else if ((match = Identifer.Match(line)).Success)
                    {
                        yield return new Identifier(match.Value);
                    }
                    else
                    {
                        throw new TokenizerException($"Unknown token: {line}");
                    }

                    line = line.Substring(match.Length);
                }
            }
        }

        public static Keyword GetKeyword(string lexeme)
        {
            switch (lexeme)
            {
                case "let":
                    return new Let();
                default:
                    throw new TokenizerException($"Unknown Keyword {lexeme}");
            }
        }

        public static Op GetOp(string op)
        {
            switch (op)
            {
                case "+":
                    return new PlusOp();
                case "-":
                    return new MinusOp();
                case "*":
                    return new TimesOp();
                case "/":
                    return new DivideOp();
                case "%":
                    return new ModOp();
                case "=":
                    return new AssignOp();
                default:
                    throw new TokenizerException($"Unknown Operator: {op}");
            }
        }

        public static Symbol GetSymbol(string op)
        {
            switch (op)
            {
                case ";":
                    return new SemiColon();
                case "(":
                    return new OpenParen();
                case ")":
                    return new CloseParen();
                default:
                    throw new TokenizerException($"Unknown Symbol: {op}");
            }
        }
    }
}
