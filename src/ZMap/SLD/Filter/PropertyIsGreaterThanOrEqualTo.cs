using System;
using System.Xml.Serialization;
using ZMap.SLD.Filter.Expression;

namespace ZMap.SLD.Filter;

/// <remarks/>
[Serializable]
[XmlType]
[XmlRoot("PropertyIsGreaterThanOrEqualTo")]
public class PropertyIsGreaterThanOrEqualTo
    : BinaryComparisonOpType
{
    public override object Accept(IFilterVisitor visitor, object extraData)
    {
        base.Accept(visitor, extraData);

        var index = Array.FindIndex(Items, type => type is PropertyNameType);
        if (index < 0)
        {
            throw new ArgumentException("PropertyIsEqualToType 必须包含一个 PropertyName 表达式");
        }

        var propertyExpression = Items[index];
        var compareExpression = Items[index == 0 ? 1 : 0];

        visitor.VisitObject(propertyExpression, extraData);
        var leftExpression = (ZMap.Style.CSharpExpression)visitor.Pop();
        visitor.VisitObject(compareExpression, extraData);
        var rightExpression = (ZMap.Style.CSharpExpression)visitor.Pop();

        visitor.Push(ZMap.Style.CSharpExpression.New(
            $"{(MatchCase ? string.Empty : "!")}ZMap.SLD.Filter.Methods.GreaterThanOrEqualTo({leftExpression.Expression}, {rightExpression.Expression})"));

        return null;
    }
}