namespace ZMap.SLD.Expression;

public class PropertyName : Expression
{
    public string Name { get; set; }

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