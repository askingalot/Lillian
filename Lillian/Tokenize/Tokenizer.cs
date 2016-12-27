using System.Collections.Generic;
using System.IO;

namespace Lillian.Tokenize
{
    public static class Tokenizer
    {
        public static IEnumerable<Token> Tokenize(string expression)
        {
            var reader = new StringReader(expression);
            var tokens = new List<Token>();

            int next;
            while ((next = reader.Peek()) != -1)
            {
                var nextChar = (char) next;
                if (char.IsWhiteSpace(nextChar))
                {
                    reader.Read(); // advance
                    // throw space away
                }
                else if (char.IsNumber(nextChar))
                {
                    tokens.Add(GetIntConstant(reader));
                }
                else if (nextChar == '(')
                {
                    tokens.Add(new LeftParen());
                    reader.Read();
                }
                else if (nextChar == ')')
                {
                    tokens.Add(new RightParen());
                    reader.Read();
                }
                else if (nextChar == ';')
                {
                    tokens.Add(new SemiColon());
                    reader.Read();
                }
                else
                {
                    tokens.Add(GetOp(reader));
                }
            }
            return tokens;
        }

        public static IntConstant GetIntConstant(StringReader reader)
        {
            try
            {
                var num = 0;
                var next = reader.Peek();
                while (next != -1 && char.IsNumber((char) next))
                {
                    num = (num*10) + (next - '0');
                    reader.Read();

                    next = reader.Peek();
                }
                return new IntConstant(num);
            }
            catch
            {
                throw new TokenizerException("Unable to tokenize number.");
            }
        }

        public static Op GetOp(StringReader reader)
        {
            var nextChar = (char) reader.Peek();
            switch (nextChar)
            {
                case '+':
                    reader.Read(); // advance
                    return new PlusOp();
                case '-':
                    reader.Read(); // advance
                    return new MinusOp();
                case '*':
                    reader.Read(); // advance
                    return new TimesOp();
                case '/':
                    reader.Read(); // advance
                    return new DivideOp();
            }

            throw new TokenizerException($"Unknown Operator: {nextChar}");
        }
    }
}
