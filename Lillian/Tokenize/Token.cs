﻿using System;

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
    public class ModOp : Op
    {
        public ModOp() : base("%") { }
    }
    public class AssignOp : Op
    {
        public AssignOp() : base("=") { }
    }

    public class EqualOp : Op
    {
        public EqualOp() : base("==") { }
    }
    public class NotEqualOp : Op
    {
        public NotEqualOp() : base("!=") { }
    }
    public class GreaterThanOrEqualOp : Op
    {
        public GreaterThanOrEqualOp() : base(">=") { }
    }
    public class LesserThanOrEqualOp : Op
    {
        public LesserThanOrEqualOp() : base("<=") { }
    }
    public class Greater : Op
    {
        public Greater() : base(">") { }
    }
    public class Lesser : Op
    {
        public Lesser() : base("<") { }
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
    public class Comma : Symbol
    {
        public Comma() : base (",") { }
    }
    public class OpenCurly : Symbol
    {
        public OpenCurly() : base("{") { }
    }
    public class CloseCurly : Symbol
    {
        public CloseCurly() : base ("}") { }
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
    public class Fun : Keyword
    {
        public Fun() : base ("fun") { }
    }


    /*************************************************************
     *  Identifiers
     ************************************************************/
    public class Identifier : Token
    {
        public Identifier(string lexeme) : base(lexeme) { }
        public string Name => Lexeme;
    }


    /*************************************************************
     *  Literals
     ************************************************************/

    public abstract class Literal<T> : Token
    {
        public Literal(string lexeme) : base(lexeme) { }
        public T Value { get; protected set; }
    }

    public class IntLiteral : Literal<int>
    {
        public IntLiteral(string lexeme) : base(lexeme)
        {
            Value = Int32.Parse(lexeme);
        }
    }
    public class StringLiteral : Literal<string>
    {
        public StringLiteral(string lexeme) : base(lexeme)
        {
            Value = lexeme;
        }
    }

    public class BooleanLiteral : Literal<bool>
    {
        public BooleanLiteral(string lexeme) : base(lexeme)
        {
            Value = bool.Parse(lexeme);
        }
    }
}