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


    /*************************************************************
     *  Operators
     ************************************************************/
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



    /*************************************************************
     *  Symbols
     ************************************************************/
    public abstract class Symbol : Token
    {
        public Symbol(string lexeme) : base(lexeme) { }
    }

    public class OpenParen : Symbol
    {
        public OpenParen() : base("(") { }
    }

    public class CloseParen : Symbol
    {
        public CloseParen() : base (")") { }
    }

    public class SemiColon : Symbol
    {
        public SemiColon() : base (";") { }
    }


    /*************************************************************
     *  Keywords
     ************************************************************/
    public abstract class Keyword : Token
    {
        public Keyword(string lexeme) : base(lexeme) { }
    }

    public class Let : Keyword
    {
        public Let() : base ("let") { }
    }


    /*************************************************************
     *  Identifiers
     ************************************************************/
    public class Identifier : Token
    {
        public Identifier(string lexeme) : base(lexeme) { 
        }
    }


    /*************************************************************
     *  Literals
     ************************************************************/
    public class IntConstant : Token
    {
        public IntConstant(string lexeme) : base(lexeme)
        {
            Value = Int32.Parse(lexeme);
        }

        public int Value { get; }
    }
}