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
        var leftExpression = (CSharpExpressionV2)visitor.Pop();

        var right = Items[1];
        visitor.Visit(right, extraData);
        var rightExpression = (CSharpExpressionV2)visitor.Pop();

        visitor.Push(
            CSharpExpressionV2.Create<dynamic>(
                $"{leftExpression.FuncName}(feature) - {rightExpression.FuncName}(feature)"));
        return null;
    }
}