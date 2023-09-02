using System;
using System.Xml.Serialization;

namespace ZMap.SLD.Filter.Expression;

[XmlRoot("Sub")]
[XmlType]
[Serializable]
public class Sub : BinaryOperatorType
{
    public override object Accept(IExpressionVisitor visitor, object extraData)
    {
        base.Accept(visitor, extraData);

        var left = Items[0];
        visitor.Visit(left, extraData);
        var leftExpression = (ZMap.Style.CSharpExpression)visitor.Pop();

        var right = Items[1];
        visitor.Visit(right, extraData);
        var rightExpression = (ZMap.Style.CSharpExpression)visitor.Pop();

        visitor.Push(ZMap.Style.CSharpExpression.New($"{leftExpression.Expression} - {rightExpression.Expression}"));
        return null;
    }
}