using System.Xml.Serialization;
using ZMap.SLD.Filter.Expression;

namespace ZMap.SLD.Filter
{
    /// <remarks/>
    [System.SerializableAttribute]
    [XmlType]
    [XmlRoot("PropertyIsBetween")]
    public class PropertyIsBetweenType : ComparisonOpsType
    {
        [XmlElement("PropertyName", typeof(PropertyNameType))]
        public PropertyNameType Item { get; set; }

        /// <remarks/>
        public LowerBoundaryType LowerBoundary { get; set; }

        /// <remarks/>
        public UpperBoundaryType UpperBoundary { get; set; }

        public override object Accept(IFilterVisitor visitor, object extraData)
        {
            visitor.VisitObject(Item, extraData);
            var propertyExpression = (ZMap.Style.CSharpExpression)visitor.Pop();

            visitor.VisitObject(LowerBoundary, extraData);
            var lowerBoundaryExpression = (ZMap.Style.CSharpExpression)visitor.Pop();

            visitor.VisitObject(UpperBoundary, extraData);
            var upperBoundaryExpression = (ZMap.Style.CSharpExpression)visitor.Pop();

            visitor.Push(ZMap.Style.CSharpExpression.New(
                $"ZMap.SLD.Filter.Methods.GreaterThanOrEqualTo({propertyExpression.Expression}, {lowerBoundaryExpression.Expression}, {upperBoundaryExpression.Expression})"));

            return null;
        }

        [System.SerializableAttribute]
        public enum PropertyIsBetweenChoiceType
        {
            /// <remarks/>
            Add,

            /// <remarks/>
            Div,

            /// <remarks/>
            Function,

            /// <remarks/>
            Literal,

            /// <remarks/>
            Mul,

            /// <remarks/>
            PropertyName,

            /// <remarks/>
            Sub,
        }
    }
}