namespace ZMap.SLD.Expression;

public abstract class Expression
{
    public abstract object Accept(ExpressionVisitor visitor, object extraData);

    public abstract object Evaluate(object @object);

    public abstract T Evaluate<T>(object @object);
}