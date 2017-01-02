using System;

namespace Lillian.Tokenize
{
    public abstract class Token
    {
        public Token(string lexeme)
        {
            Lexeme = lexeme;
        }
        public string Lexeme { get; }
        public override string ToString() => $" {Lexeme} ";
    }

    public abstract class Op : Token
    {
        public Op(string lexeme) : base(lexeme) { }
    } 

    public class PlusOp : Op
    {
        public PlusOp() : base("+") { }
    }

    public class MinusOp : Op
    {
        public MinusOp() : base("-") { }
    }

    public class TimesOp : Op
    {
        public TimesOp() : base("*") { }
    }

    public class DivideOp : Op
    {
        public DivideOp() : base("/") { }
    }

    public class IntConstant : Token
    {
        public IntConstant(string lexeme) : base(lexeme)
        {
            Value = Int32.Parse(lexeme);
        }

        public int Value { get; }
    }

    public class OpenParen : Token
    {
        public OpenParen() : base("(") { }
    }

    public class CloseParen : Token
    {
        public CloseParen() : base (")") { }
    }

    public class SemiColon : Token
    {
        public SemiColon() : base (";") { }
    }
}