using System;

namespace Lillian.Tokenize
{
    public class TokenizerException : Exception
    {
        public TokenizerException(string message) : base(message)
        {
        }
    }

    public class OutOfTokensException : TokenizerException
    {
        public OutOfTokensException() : base("The TokenEnumerator has no more tokens") { }
    }
}