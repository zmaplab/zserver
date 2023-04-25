using System.Xml.Serialization;

namespace ZMap.SLD.Filter.Expression;

[XmlRoot("Nil")]
public class NilExpression : ExpressionType
{
    public override object Accept(IExpressionVisitor visitor, object extraData)
    {
        visitor.Push(ZMap.Style.CSharpExpression.New($"null"));
        return null;
    }
}