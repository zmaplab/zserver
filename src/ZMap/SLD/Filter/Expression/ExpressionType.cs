using System.Xml.Serialization;

namespace ZMap.SLD.Filter.Expression;

//[XmlIncludeAttribute(typeof(MapItemType))]
//[XmlIncludeAttribute(typeof(InterpolationPointType))]
[XmlInclude(typeof(LiteralType))]
[XmlInclude(typeof(FunctionType1))]
[XmlInclude(typeof(PropertyNameType))]
[XmlInclude(typeof(BinaryOperatorType))]
[XmlInclude(typeof(FunctionType))]
// [XmlIncludeAttribute(typeof(RecodeType))]
// [XmlIncludeAttribute(typeof(InterpolateType))]
// [XmlIncludeAttribute(typeof(CategorizeType))]
// [XmlIncludeAttribute(typeof(StringLengthType))]
// [XmlIncludeAttribute(typeof(StringPositionType))]
// [XmlIncludeAttribute(typeof(TrimType))]
// [XmlIncludeAttribute(typeof(ChangeCaseType))]
// [XmlIncludeAttribute(typeof(ConcatenateType))]
// [XmlIncludeAttribute(typeof(SubstringType))]
// [XmlIncludeAttribute(typeof(FormatDateType))]
// [XmlIncludeAttribute(typeof(FormatNumberType))]
public abstract class ExpressionType
{
    public abstract object Accept(IExpressionVisitor visitor, object extraData);
}