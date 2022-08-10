using System.Xml;

namespace ZMap.SLD
{
    public class PolygonSymbolizer : ISymbolizer
    {
        public OnlineResource OnlineResource { get; set; }
        /// <summary>
        /// TODO: 可以支持表达式， 转换图形
        /// </summary>
        public Geometry Geometry { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Stroke Stroke { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Fill Fill { get; set; }
        public Displacement Displacement { get; set; }

        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.LocalName == "PolygonSymbolizer" && reader.NodeType == XmlNodeType.EndElement)
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
                            {
                                var geometry = new Geometry();
                                geometry.ReadXml(reader);
                                Geometry = geometry;
                                break;
                            }

                        case "Stroke" when reader.NodeType == XmlNodeType.Element:
                            {
                                var stroke = new Stroke();
                                stroke.ReadXml(reader);
                                Stroke = stroke;
                                break;
                            }

                        case "Fill" when reader.NodeType == XmlNodeType.Element:
                            {
                                var fill = new Fill();
                                fill.ReadXml(reader);
                                Fill = fill;
                                break;
                            }
                        case "Displacement" when reader.NodeType == XmlNodeType.Element:
                            {
                                var displacement = new Displacement();
                                displacement.ReadXml(reader);
                                Displacement = displacement;
                                break;
                            }
                    }
            }
        }
    }
}