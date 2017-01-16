using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Lillian.Tokenize
{
    public static class Tokenizer
    {
        public static readonly Regex Whitespace      = new Regex(@"^\s+");
        public static readonly Regex Comment         = new Regex(@"^#.*$");
        public static readonly Regex IntegerLiteral  = new Regex(@"^[+\-]?\d+");
        public static readonly Regex StringLiteral   = new Regex(@"^'([^']*)'");
        public static readonly Regex BooleanLiteral  = new Regex(@"^(true|false)");
        public static readonly Regex Operator        = 
            new Regex(@"^(==|!=|>=|<=|>|<|\+|-|\*|\/|%|=)");
        public static readonly Regex Symbol          = new Regex(@"^[,;(){}]");
        public static readonly Regex Keyword         = 
            new Regex($@"^({
                string.Join("|", "let", "fun")
            })");
        public static readonly Regex Identifer       = new Regex(@"^[_a-z]([_a-zA-Z0-9])*");

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
                    else if ((match = IntegerLiteral.Match(line)).Success)
                    {
                        yield return new IntLiteral(match.Value);
                    }
                    else if ((match = StringLiteral.Match(line)).Success)
                    {
                        yield return new StringLiteral(match.Groups[1].Value);
                    }
                    else if ((match = BooleanLiteral.Match(line)).Success)
                    {
                        yield return new BooleanLiteral(match.Value);
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
                case "let": return new Let();
                case "fun": return new Fun();
                default:
                    throw new TokenizerException($"Unknown Keyword {lexeme}");
            }
        }

        public static Op GetOp(string op)
        {
            switch (op)
            {
                case "+": return new PlusOp();
                case "-": return new MinusOp();
                case "*": return new TimesOp();
                case "/": return new DivideOp();
                case "%": return new ModOp();
                case "=": return new AssignOp();
                case "==": return new EqualOp();
                case "!=": return new NotEqualOp();
                case ">=": return new GreaterThanOrEqualOp();
                case "<=": return new LesserThanOrEqualOp();
                case ">": return new Greater();
                case "<": return new Lesser();
                default:
                    throw new TokenizerException($"Unknown Operator: {op}");
            }
        }

        public static Symbol GetSymbol(string op)
        {
            switch (op)
            {
                case "(": return new OpenParen();
                case ")": return new CloseParen();
                case ",": return new Comma();
                case ";": return new SemiColon();
                case "{": return new OpenCurly();
                case "}": return new CloseCurly();
                default:
                    throw new TokenizerException($"Unknown Symbol: {op}");
            }
        }
    }
}
