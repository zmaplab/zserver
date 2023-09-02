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
        var propertyExpression = (ZMap.Style.CSharpExpression)visitor.Pop();

        visitor.VisitObject(Literal, extraData);
        var literalExpression = (ZMap.Style.CSharpExpression)visitor.Pop();

        visitor.Push(ZMap.Style.CSharpExpression.New(
            $"ZMap.SLD.Filter.Methods.Like({propertyExpression.Expression}, {literalExpression.Expression}, \"{WildCard}\", \"{SingleChar}\", \"{EscapeChar}\")"));
        return null;
    }
}