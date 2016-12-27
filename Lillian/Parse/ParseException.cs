using System;

namespace Lillian.Parse
{
    public class ParseException : Exception
    {
        public ParseException(string message) : base(message)
        {
        }
    }
}