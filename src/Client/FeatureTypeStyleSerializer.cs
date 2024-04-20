using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using SharpMap.Symbology.Serialization;

namespace Client;

public class NamespaceIgnorantXmlTextReader(Stream stream) : XmlTextReader(stream)
{
    public override string NamespaceURI => "";
}
    
public class FeatureTypeStyleSerializer
{
    private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(FeatureTypeStyleType));

    public static FeatureTypeStyleType Deserialize(String xml)
    {
        return Serializer.Deserialize(new StringReader(xml)) as FeatureTypeStyleType;
    }

    public static String Serialize(FeatureTypeStyleType featureTypeStyle)
    {
        var writer = new StringWriter();
        Serializer.Serialize(writer, featureTypeStyle);
        return writer.ToString();
    }
}