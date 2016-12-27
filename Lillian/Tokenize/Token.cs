namespace Lillian.Tokenize
{
    public abstract class Token { }
    public abstract class Op : Token { } 
    public class PlusOp : Op
    {
        public override string ToString() => " + ";
    }

    public class MinusOp : Op
    {
        public override string ToString() => " - ";
    }

    public class TimesOp : Op
    {
        public override string ToString() => " * ";
    }

    public class DivideOp : Op
    {
        public override string ToString() => " / ";
    }

    public class IntConstant : Token
    {
        public IntConstant(int value)
        {
            Value = value;
        }

        public int Value { get; }
        public override string ToString() => Value.ToString();
    }

    public class LeftParen : Token
    {
        public override string ToString() => "(";
    }

    public class RightParen : Token
    {
        public override string ToString() => ")";
    }

    public class SemiColon : Token
    {
        public override string ToString() => ";";
    }
}