using System.Xml;

namespace ZMap.SLD
{
    public class Halo
    {
        public float Radius { get; set; }
        public Fill Fill { get; set; }
        
        // public void ReadXml(XmlReader reader)
        // {
        //     while (reader.Read())
        //     {
        //         if (reader.LocalName == "Halo" && reader.NodeType == XmlNodeType.EndElement)
        //         {
        //             break;
        //         }
        //         else
        //             switch (reader.LocalName)
        //             {
        //                 case "Radius" when reader.NodeType == XmlNodeType.Element:
        //                     Radius = reader.ReadElementContentAsInt();
        //                     break;
        //                 case "Fill" when reader.NodeType == XmlNodeType.Element:
        //                     var fill = new Fill();
        //                     fill.ReadXml(reader);
        //                     Fill = fill;
        //                     break;
        //             }
        //     }
        // }
    }
}