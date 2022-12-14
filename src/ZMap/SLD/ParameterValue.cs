using System;
using System.Xml.Serialization;
using ZMap.SLD.Expression;

namespace ZMap.SLD;

[Serializable]
[XmlRoot("InitialGap")]
public class ParameterValue
{
    /// <summary>
    /// 说明： 暂时只支持单个表达式， GeoServer 亦是如此， 后续看为什么对 SLD 的支持不完整
    /// </summary>
    [XmlElement("Add", typeof(BinaryOperator))]
    [XmlElement("Div", typeof(BinaryOperator))]
    [XmlElement("Function", typeof(Function))]
    [XmlElement("Literal", typeof(Literal))]
    [XmlElement("Mul", typeof(BinaryOperator))]
    [XmlElement("PropertyName", typeof(PropertyName))]
    [XmlElement("Sub", typeof(BinaryOperator))]
    [XmlChoiceIdentifier("ExpressionElementName")]
    public Expression.Expression Expression { get; set; }

    /// <remarks/>
    [XmlElement("ExpressionElementName")]
    [XmlIgnore]
    public ExpressionChoiceType ExpressionElementName { get; set; }

    /// <remarks/>
    [XmlText]
    public string Text { get; set; }

    public enum ExpressionChoiceType
    {
        /// <remarks/>
        [XmlEnum] Add,

        /// <remarks/>
        [XmlEnum] Div,

        /// <remarks/>
        [XmlEnum] Function,

        /// <remarks/>
        [XmlEnum] Literal,

        /// <remarks/>
        [XmlEnum] Mul,

        /// <remarks/>
        [XmlEnum] PropertyName,

        /// <remarks/>
        [XmlEnum] Sub
    }

    public object Accept(IStyleVisitor visitor, object extraData)
    {
        if (Text != null)
        {
            visitor.Push(Text);
        }
        else
        {
            visitor.Visit(Expression, extraData);
        }

        return null;
    }
}

[XmlInclude(typeof(SvgParameter))]
[XmlInclude(typeof(CssParameter))]
public class NamedParameter : ParameterValue
{
    /// <summary>
    /// 
    /// </summary>
    [XmlAttribute("name")]
    public string Name { get; set; }
}

[XmlRoot("SvgParameter")]
public class SvgParameter : NamedParameter
{
}

[XmlRoot("CssParameter")]
public class CssParameter : NamedParameter
{
}