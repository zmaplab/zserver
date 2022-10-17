namespace ZMap.SLD.Expression;

public class NilExpression : Expression
{
    public override object Accept(ExpressionVisitor visitor, object extraData)
    {
        return visitor.Visit(this, extraData);
    }

    public override object Evaluate(object @object)
    {
        return null;
    }

    public override T Evaluate<T>(object @object)
    {
        return default;
    }
}