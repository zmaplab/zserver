using System.Xml;

namespace ZMap.SLD
{
    public class Labeling : ISymbolizer
    {
        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.LocalName == "Labeling" && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
            }
        }
    }
}