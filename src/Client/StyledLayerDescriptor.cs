using System.Collections.Generic;
using System.Xml.Serialization;
using SharpMap.Symbology.Serialization;

namespace Client;

public class StyledLayerDescriptor
{
    [XmlElement("NamedLayer")] public List<NamedLayer> NamedLayers { get; set; }
}

public class NamedLayer
{
    [XmlElement("UserStyle")] public UserStyle UserStyle { get; set; }
}

public class UserStyle
{
    [XmlElement("FeatureTypeStyle")] public FeatureTypeStyleType FeatureTypeStyle { get; set; }
}