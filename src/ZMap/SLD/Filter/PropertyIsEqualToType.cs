namespace ZMap.SLD.Filter;

/// <remarks/>
[Serializable]
[XmlType]
[XmlRoot("PropertyIsEqualTo")]
public class PropertyIsEqualToType : BinaryComparisonOpType
{
    public override object Accept(IFilterVisitor visitor, object extraData)
    {
        var index = Array.FindIndex(Items, type => type is PropertyNameType);
        if (index < 0)
        {
            throw new ArgumentException("PropertyIsEqualToType 必须包含一个 PropertyName 表达式");
        }

        var left = Items[0];
        var right = Items[1];
        visitor.VisitObject(left, extraData);
        var leftExpression = (CSharpExpressionV2)visitor.Pop();
        visitor.VisitObject(right, extraData);
        var rightExpression = (CSharpExpressionV2)visitor.Pop();

        visitor.Push(CSharpExpressionV2.Create<bool>(
            $"{(MatchCase ? string.Empty : "!")}({leftExpression.FuncName}(feature) == {rightExpression.FuncName}(feature))"));
        return null;
    }
}