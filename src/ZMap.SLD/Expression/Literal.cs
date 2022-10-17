namespace ZMap.SLD.Expression;

public class Literal : Expression
{
    public object Value { get; set; }

    public override object Accept(ExpressionVisitor visitor, object extraData)
    {
        throw new System.NotImplementedException();
    }

    public override object Evaluate(object @object)
    {
        throw new System.NotImplementedException();
    }

    public override T Evaluate<T>(object @object)
    {
        throw new System.NotImplementedException();
    }
}