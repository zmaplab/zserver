using System.Xml.Serialization;
using ZMap.SLD.Filter;

namespace ZMap.SLD;

public class FilterType
{
    [XmlElement(typeof(PropertyIsEqualTo))]
    [XmlElement(typeof(PropertyIsBetween))]
    public Filter.Filter Filter { get; set; }
}