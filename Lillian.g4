grammar Lillian;

program : block EOF ;

block : (expr ';')+ ;

expr : functionDefinition
     | functionCall
     | binding
     | '-' expr
     | expr ('*'|'/') expr
     | expr ('+'|'-') expr

/*
    ExprBlock     := Expr Expr*
    Expr          := NonEmptyExpr Semi
                   | Semi
    NonEmptyExpr  := Call
                   | Binding
                   | BinOp
    BinOp         := Comparison
    Comparison    := Boolean
                   | Boolean CompOp Boolean
                   | String
                   | String CompOp String
                   : Sum
                   : Sum CompOp Sum
    Call          := Id ( Args )
                   | Id ( ) 
    Args          := NonEmptyExpr, Args
                   | NonEmptyExpr
    Binding       := let Id AssignOp NonEmptyExpr
    Comparison    := NonEmptyExpr EqualOp NonEmptyExpr
    Sum           := Product
                   | Product SumOp Sum 
    Product       := Factor 
                   | Factor ProdOp Product 
    Factor        := ( Sum )
                   | Number
                   | Id
                   | NonEmptyExpr
    Boolean       := true | false
    Number        := Digit Number 
                   | Digit
    Digit         := 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9
    String        := abcd... | ''
    Id            := a-z (a-z | _ | A-Z)*
    SumOp         := + | -
    ProdOp        := * | / | %
    CompOp        := ==
    AssignOp      := =
    Semi          := ;

 */
