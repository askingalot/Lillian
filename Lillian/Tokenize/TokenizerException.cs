using System;

namespace Lillian.Tokenize
{
    public class TokenizerException : Exception
    {
        public TokenizerException(string message) : base(message)
        {
        }
    }
}