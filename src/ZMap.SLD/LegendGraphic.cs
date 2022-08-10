using System.Xml;

namespace ZMap.SLD
{
    public class LegendGraphic
    {
        public Graphic Graphic { get; set; }
        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.LocalName == "LegendGraphic" && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else if (reader.LocalName == "Graphic" && reader.NodeType == XmlNodeType.Element)
                {
                    var graphic = new Graphic();
                    graphic.ReadXml(reader);
                    Graphic = graphic;
                }
            }
        }
    }
}