namespace ZMap.SLD.Filter;

/// <remarks/>
[Serializable]
[XmlType]
[XmlRoot("PropertyIsEmpty")]
public class PropertyIsEmptyType : ComparisonOpsType
{
    /// <remarks/>
    public PropertyNameType PropertyName { get; set; }

    public override object Accept(IFilterVisitor visitor, object extraData)
    {
        visitor.VisitObject(PropertyName, extraData);

        var expression = (CSharpExpressionV2)visitor.Pop();

        visitor.Push(CSharpExpressionV2.Create<bool>($"string.IsNullOrWhiteSpace({expression.FuncName}(feature)?.ToString())"));

        return null;
    }
}