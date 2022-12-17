using System;
using System.Xml.Serialization;
using ZMap.SLD.Filter.Expression;

namespace ZMap.SLD.Filter
{
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
            var leftExpression = (ZMap.Style.Expression)visitor.Pop();
            visitor.VisitObject(right, extraData);
            var rightExpression = (ZMap.Style.Expression)visitor.Pop();

            visitor.Push(ZMap.Style.Expression.New(
                $"{(MatchCase ? string.Empty : "!")}ZMap.SLD.Filter.Methods.EqualTo({leftExpression.Body}, {rightExpression.Body})"));
            return null;
        }
    }
}