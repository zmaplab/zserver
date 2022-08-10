using System.Linq;
using System.Xml;

namespace ZMap.SLD
{
    public class Stroke
    {
        /// <summary>
        /// 描边的颜色（可选， 默认和 fill-color 一致。如果设置了 fill-pattern/Graphic， 则 fill-outline-color 将无效。为了使用此属性， 还需要设置 fill-antialias 为 true）
        /// </summary>
        public string Color { get; set; } = "#000000";

        public int Width { get; set; }

        /// <summary>
        /// 填充的不透明度（可选，取值范围为 0 ~ 1，默认值为 1）
        /// </summary>
        public float Opacity { get; set; } = 1;

        /// <summary>
        /// Indicates how the various segments of a (thick) line string should be joined.
        /// </summary>
        public string LineJoin { get; set; }

        /// <summary>
        /// "butt", "round", and "square"
        /// </summary>
        public string Linecap { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float[] DashArray { get; set; }

        public float DashOffset { get; set; } = 1;
        public GraphicFill GraphicFill { get; set; }
        public GraphicStroke GraphicStroke { get; set; }

        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.LocalName == "Stroke" && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else if (reader.LocalName == "SvgParameter" && reader.NodeType == XmlNodeType.Element)
                {
                    var attribute = reader.GetAttribute("name").ToLower();
                    switch (attribute)
                    {
                        case "stroke":
                        {
                            Color = reader.ReadElementContentAsString();
                            break;
                        }
                        case "stroke-width":
                        {
                            Width = reader.ReadElementContentAsInt();
                            break;
                        }
                        case "stroke-opacity":
                        {
                            Opacity = reader.ReadElementContentAsFloat();
                            break;
                        }
                        case "stroke-linejoin":
                        {
                            LineJoin = reader.ReadElementContentAsString();
                            break;
                        }
                        case "stroke-linecap":
                        {
                            Linecap = reader.ReadElementContentAsString();
                            break;
                        }
                        case "stroke-dasharray":
                        {
                            var val = reader.ReadElementContentAsString();
                            DashArray = val?.Split(' ').Where(s => float.TryParse(s, out var x))
                                ?.Select(s => float.Parse(s)).ToArray();
                            break;
                        }
                        case "stroke-dashoffset":
                        {
                            DashOffset = reader.ReadElementContentAsFloat();
                            break;
                        }
                        case "GraphicFill" when reader.NodeType == XmlNodeType.Element:
                            var graphicFill = new GraphicFill();
                            graphicFill.ReadXml(reader);
                            GraphicFill = graphicFill;
                            break;
                        case "GraphicStroke" when reader.NodeType == XmlNodeType.Element:
                            var graphicStroke = new GraphicStroke();
                            graphicStroke.ReadXml(reader);
                            GraphicStroke = graphicStroke;
                            break;
                    }
                }
            }
        }
    }
}