using System.Xml;

namespace ZMap.SLD
{
    public class ImageOutline
    {
        public LineSymbolizer LineSymbolizer { get; set; }
        public PolygonSymbolizer PolygonSymbolizer { get; set; }
        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.LocalName == "ImageOutline" && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                    switch (reader.LocalName)
                    {
                        case "LineSymbolizer" when reader.NodeType == XmlNodeType.Element:
                            var symbolizer = new LineSymbolizer();
                            symbolizer.ReadXml(reader);
                            LineSymbolizer = symbolizer;
                            break;
                        case "PolygonSymbolizer" when reader.NodeType == XmlNodeType.Element:
                            var polygonSymbolizer = new PolygonSymbolizer();
                            polygonSymbolizer.ReadXml(reader);
                            PolygonSymbolizer = polygonSymbolizer;
                            break;
                    }
            }
        }
    }
}