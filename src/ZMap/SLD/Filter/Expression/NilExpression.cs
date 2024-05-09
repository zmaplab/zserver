namespace ZMap.SLD.Filter.Expression;

[XmlRoot("Nil")]
public class NilExpression : ExpressionType
{
    public override object Accept(IExpressionVisitor visitor, object extraData)
    {
        visitor.Push(CSharpExpressionV2.Create<object>("null"));
        return null;
    }
}