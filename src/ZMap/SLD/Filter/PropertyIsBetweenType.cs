namespace ZMap.SLD.Filter;

/// <remarks/>
[Serializable]
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
        var propertyExpression = (CSharpExpressionV2)visitor.Pop();

        visitor.VisitObject(LowerBoundary, extraData);
        var lowerBoundaryExpression = (CSharpExpressionV2)visitor.Pop();

        visitor.VisitObject(UpperBoundary, extraData);
        var upperBoundaryExpression = (CSharpExpressionV2)visitor.Pop();

        visitor.Push(CSharpExpressionV2.Create<bool>(
            $"{propertyExpression.FuncName}(feature) >= {lowerBoundaryExpression.FuncName}(feature) && {propertyExpression.FuncName}(feature) <= {upperBoundaryExpression.FuncName}(feature)"));

        return null;
    }

    [Serializable]
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