namespace ZMap.SLD.Filter;

/// <remarks/>
[Serializable]
[XmlType]
[XmlRoot("Not")]
public class Not : UnaryLogicOpType
{
    public override object Accept(IFilterVisitor visitor, object extraData)
    {
        throw new NotImplementedException();
    }
}