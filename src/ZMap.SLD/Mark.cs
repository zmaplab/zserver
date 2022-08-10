using System.Xml;

namespace ZMap.SLD
{
    public class Mark : GraphicalSymbol
    {
        public string WellKnownName { get; set; }
        public Fill Fill { get; set; }
        public Stroke Stroke { get; set; }
        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.LocalName == "Mark" && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                    switch (reader.LocalName)
                    {
                        case "WellKnownName" when reader.NodeType == XmlNodeType.Element:
                            WellKnownName = reader.ReadContentAsString();
                            break;
                        case "Fill" when reader.NodeType == XmlNodeType.Element:
                            {
                                var fill = new Fill();
                                fill.ReadXml(reader);
                                Fill = fill;
                                break;
                            }

                        case "Stroke" when reader.NodeType == XmlNodeType.Element:
                            {
                                var stroke = new Stroke();
                                stroke.ReadXml(reader);
                                Stroke = stroke;
                                break;
                            }
                    }
            }
        }
    }
}