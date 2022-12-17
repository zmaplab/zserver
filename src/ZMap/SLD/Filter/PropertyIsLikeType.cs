using System.Xml.Serialization;
using ZMap.SLD.Filter.Expression;

namespace ZMap.SLD.Filter;

/// <remarks/>
[System.SerializableAttribute]
[XmlType]
[XmlRoot("PropertyIsLike")]
public class PropertyIsLikeType : ComparisonOpsType
{
    /// <remarks/>
    [XmlElement("PropertyName")]
    public PropertyNameType PropertyName { get; set; }

    /// <remarks/>
    [XmlElement("Literal")]
    public LiteralType Literal { get; set; }

    /// <remarks/>
    [XmlAttribute("wildCard")]
    public string WildCard { get; set; }

    /// <remarks/>
    [XmlAttribute("singleChar")]
    public string SingleChar { get; set; }

    /// <remarks/>
    [XmlAttribute("escapeChar")]
    public string EscapeChar { get; set; }

    public override object Accept(IFilterVisitor visitor, object extraData)
    {
        visitor.VisitObject(PropertyName, extraData);
        var propertyExpression = (ZMap.Style.Expression)visitor.Pop();

        visitor.VisitObject(Literal, extraData);
        var literalExpression = (ZMap.Style.Expression)visitor.Pop();

        visitor.Push(ZMap.Style.Expression.New(
            $"ZMap.SLD.Filter.Methods.Like({propertyExpression.Body}, {literalExpression.Body}, \"{WildCard}\", \"{SingleChar}\", \"{EscapeChar}\")"));
        return null;
    }
}