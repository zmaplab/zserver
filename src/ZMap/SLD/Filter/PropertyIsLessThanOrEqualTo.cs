using System.Xml.Serialization;

namespace ZMap.SLD.Filter;

/// <remarks/>
[System.SerializableAttribute]
[XmlType]
[XmlRoot("PropertyIsLessThanOrEqualTo")]
public class PropertyIsLessThanOrEqualTo
    : BinaryComparisonOpType
{
    public override object Accept(IFilterVisitor visitor, object extraData)
    {
        var left = Items[0];
        var right = Items[1];
        visitor.VisitObject(left, extraData);
        var leftExpression = (ZMap.Style.Expression)visitor.Pop();
        visitor.VisitObject(right, extraData);
        var rightExpression = (ZMap.Style.Expression)visitor.Pop();

        visitor.Push(ZMap.Style.Expression.New(
            $"{(MatchCase ? string.Empty : "!")}ZMap.SLD.Filter.Methods.LessThanOrEqualTo({leftExpression.Body}, {rightExpression.Body})"));

        return null;
    }
}