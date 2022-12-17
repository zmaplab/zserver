using System.Xml.Serialization;

namespace ZMap.SLD.Filter;

[XmlInclude(typeof(FeatureIdType))]
[System.SerializableAttribute]
[XmlType]
public abstract class AbstractIdType
{
}