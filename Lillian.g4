grammar Lillian;

program : block EOF ;

block : (expr ';')* ;

expr : functionDefinition
     | functionCall
     | binding
     | '-' expr
     | expr op=('*'|'/') expr
     | expr op=('+'|'-') expr 
     | INT_LITERAL 
     | STRING_LITERAL
     | IDENTIFIER
     ;

functionDefinition : 'fun' '(' ( parameterList | ) ')' ':' TYPE '{' block '}' ;

functionCall : IDENTIFIER '(' argumentList ')' ;

argumentList : ( expr ( ',' expr )* )? ;

binding : 'let' idType '=' expr ;

idType : IDENTIFIER ':' TYPE ;

parameterList : ( idType (',' idType )* ) ;

TYPE : [A-Z][a-zA-Z0-9_]* ;
IDENTIFIER : [_a-zA-Z][_?a-zA-Z0-9]* ;

INT_LITERAL : [0-9]+ ;

STRING_LITERAL : '\'' ~'\''* '\'' 
               |  '"' ~'"'* '"' 
               ;

COMMENT : '//' ~('\r' | '\n')* -> skip ;
ML_COMMENT : '/*' .*? '*/' -> skip ;

WS : [ \t\r\n]+ -> skip;

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
