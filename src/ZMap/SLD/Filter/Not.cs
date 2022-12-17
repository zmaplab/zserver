using System.Xml.Serialization;

namespace ZMap.SLD.Filter;

/// <remarks/>
[System.SerializableAttribute]
[XmlType]
[XmlRoot("Not")]
public class Not : UnaryLogicOpType
{
    public override object Accept(IFilterVisitor visitor, object extraData)
    {
        throw new System.NotImplementedException();
    }
}