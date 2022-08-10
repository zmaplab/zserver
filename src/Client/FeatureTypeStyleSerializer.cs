using System;
using System.IO;
using System.Xml.Serialization;

namespace SharpMap.Symbology.Serialization
{
    public class FeatureTypeStyleSerializer
    {
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(FeatureTypeStyleType));

        public static FeatureTypeStyleType Deserialize(String xml)
        {
            return _serializer.Deserialize(new StringReader(xml)) as FeatureTypeStyleType;
        }

        public static String Serialize(FeatureTypeStyleType featureTypeStyle)
        {
            var writer = new StringWriter();
            _serializer.Serialize(writer, featureTypeStyle);
            return writer.ToString();
        }
    }
}
