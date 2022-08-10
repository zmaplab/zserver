using System.Xml;

namespace ZMap.SLD
{
    public class Font
    {
        /// <summary>
        /// 字体宽
        /// </summary>
        public int FontWeight { get; set; }
        /// <summary>
        /// 字体大小
        /// </summary>
        public int FontSize { get; set; } = 12;
        /// <summary>
        /// 字体
        /// </summary>
        public string FontFamily { get; set; }
        /// <summary>
        /// 斜体等
        /// </summary>
        public string FontStyle { get; set; }
        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.LocalName == "Font" && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else if (reader.LocalName == "SvgParameter" && reader.NodeType == XmlNodeType.Element)
                {
                    var attribute = reader.GetAttribute("name").ToLower();
                    switch (attribute)
                    {
                        case "font-weight":
                            {
                                FontWeight = reader.ReadElementContentAsInt();
                                break;
                            }
                        case "font-size":
                            {
                                FontSize = reader.ReadElementContentAsInt();
                                break;
                            }
                        case "font-family":
                            {
                                FontFamily = reader.ReadElementContentAsString();
                                break;
                            }
                        case "font-style":
                            {
                                FontStyle = reader.ReadElementContentAsString();
                                break;
                            }
                    }
                }
            }
        }
    }
}