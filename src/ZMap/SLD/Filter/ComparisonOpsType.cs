namespace ZMap.SLD.Filter;

/// <remarks/>
[XmlInclude(typeof(PropertyIsBetweenType))]
[XmlInclude(typeof(PropertyIsNullType))]
[XmlInclude(typeof(PropertyIsEmptyType))]
[XmlInclude(typeof(PropertyIsLikeType))]
[XmlInclude(typeof(BinaryComparisonOpType))]
[Serializable]
[XmlType]
public class ComparisonOpsType
{
    public virtual object Accept(IFilterVisitor visitor, object extraData)
    {
        throw new NotImplementedException();
    }
}