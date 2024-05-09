namespace ZMap.SLD.Filter;

/// <remarks/>
[Serializable]
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
        var propertyExpression = (CSharpExpressionV2)visitor.Pop();

        visitor.VisitObject(Literal, extraData);
        var literalExpression = (CSharpExpressionV2)visitor.Pop();

        throw new NotImplementedException();
        // visitor.Push(ZMap.Style.CSharpExpressionV2.Create<bool>(
        //     $"ZMap.SLD.Filter.Methods.Like({propertyExpression.Expression}, {literalExpression.Expression}, \"{WildCard}\", \"{SingleChar}\", \"{EscapeChar}\")"));

        return null;
    }
}