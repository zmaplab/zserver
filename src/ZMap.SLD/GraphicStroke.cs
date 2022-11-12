namespace ZMap.SLD
{
    public class GraphicStroke
    {
        public Graphic Graphic { get; set; }
        public float InitialGap { get; set; }
        public float Gap { get; set; }
        // public void ReadXml(XmlReader reader)
        // {
        //     while (reader.Read())
        //     {
        //         if (reader.LocalName == "RasterSymbolizer" && reader.NodeType == XmlNodeType.EndElement)
        //         {
        //             break;
        //         }
        //         else
        //             switch (reader.LocalName)
        //             {
        //                 case "Graphic" when reader.NodeType == XmlNodeType.Element:
        //                     {
        //                         var graphic = new Graphic();
        //                         graphic.ReadXml(reader);
        //                         Graphic = graphic;
        //                         break;
        //                     }
        //                 case "InitialGap" when reader.NodeType == XmlNodeType.Element:
        //                     InitialGap = reader.ReadElementContentAsFloat();
        //                     break;
        //                 case "Gap" when reader.NodeType == XmlNodeType.Element:
        //                     Gap = reader.ReadElementContentAsFloat();
        //                     break;
        //             }
        //     }
        // }
    }
}