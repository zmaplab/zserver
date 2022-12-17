using System;
using System.Xml.Serialization;

namespace ZMap.SLD.Filter.Expression;

[XmlRoot("Div")]
[XmlType]
[Serializable]
public class Div : BinaryOperatorType
{
    public override object Accept(IExpressionVisitor visitor, object extraData)
    {
        base.Accept(visitor, extraData);

        var left = Items[0];
        visitor.Visit(left, extraData);
        var leftExpression = (ZMap.Style.Expression)visitor.Pop();

        var right = Items[1];
        visitor.Visit(right, extraData);
        var rightExpression = (ZMap.Style.Expression)visitor.Pop();

        visitor.Push(ZMap.Style.Expression.New($"{leftExpression.Body} / {rightExpression.Body}"));
        return null;
    }
}