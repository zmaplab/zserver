namespace ZMap.SLD.Expression;

public abstract class BinaryOperator : Expression
{
    public Expression LeftExpression { get; set; }
    public Expression RightExpression { get; set; }

 
}