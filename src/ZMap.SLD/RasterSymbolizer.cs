using System.Xml;

namespace ZMap.SLD
{
    public class RasterSymbolizer : ISymbolizer
    {
        public OnlineResource OnlineResource { get; set; }
        public Geometry Geometry { get; set; }
        public float Opacity { get; set; }
        public ChannelSelection ChannelSelection { get; set; }
        public int OverlapBehavior { get; set; }
        public ColorMap ColorMap { get; set; }
        public ContrastEnhancement ContrastEnhancement { get; set; }
        public ShadedRelief ShadedRelief { get; set; }
        public ImageOutline ImageOutline { get; set; }

        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.LocalName == "RasterSymbolizer" && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                    switch (reader.LocalName)
                    {
                        case "OnlineResource" when reader.NodeType == XmlNodeType.Element:
                            {
                                var onlineResource = new OnlineResource();
                                onlineResource.ReadXml(reader);
                                OnlineResource = onlineResource;
                                break;
                            }
                        case "Geometry" when reader.NodeType == XmlNodeType.Element:
                            var geometry = new Geometry();
                            geometry.ReadXml(reader);
                            Geometry = geometry;
                            break;
                        case "Opacity" when reader.NodeType == XmlNodeType.Element:
                            Opacity = reader.ReadElementContentAsFloat();
                            break;
                        case "ChannelSelection" when reader.NodeType == XmlNodeType.Element:
                            var channelSelection = new ChannelSelection();
                            channelSelection.ReadXml(reader);
                            ChannelSelection = channelSelection;
                            break;
                        case "OverlapBehavior" when reader.NodeType == XmlNodeType.Element:
                            OverlapBehavior = reader.ReadElementContentAsInt();
                            break;
                        case "ColorMap" when reader.NodeType == XmlNodeType.Element:
                            var colorMap = new ColorMap();
                            colorMap.ReadXml(reader);
                            ColorMap = colorMap;
                            break;
                        case "ContrastEnhancement" when reader.NodeType == XmlNodeType.Element:
                            var contrastEnhancement = new ContrastEnhancement();
                            contrastEnhancement.ReadXml(reader);
                            ContrastEnhancement = contrastEnhancement;
                            break;
                        case "ShadedRelief" when reader.NodeType == XmlNodeType.Element:
                            var shadedRelief = new ShadedRelief();
                            shadedRelief.ReadXml(reader);
                            ShadedRelief = shadedRelief;
                            break;
                        case "ImageOutline" when reader.NodeType == XmlNodeType.Element:
                            var imageOutline = new ImageOutline();
                            imageOutline.ReadXml(reader);
                            ImageOutline = imageOutline;
                            break;
                    }
            }
        }
    }
}