using System.Xml;

namespace ZMap.SLD
{
    public class AnchorPoint
    {
        /// <summary>
        /// 标注X轴均偏移倍数
        /// </summary>
        public float AnchorPointX { get; set; }
        /// <summary>
        /// 标注Y轴均偏移倍数
        /// </summary>
        public float AnchorPointY { get; set; }
        public AnchorPoint() { }
        public AnchorPoint(float anchorPointX, float anchorPointY)
        {
            AnchorPointX = anchorPointX;
            AnchorPointY = anchorPointY;
        }
        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.LocalName == "AnchorPoint" && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                    switch (reader.LocalName)
                    {
                        case "AnchorPointX" when reader.NodeType == XmlNodeType.Element:
                            AnchorPointX = reader.ReadContentAsFloat();
                            break;
                        case "AnchorPointY" when reader.NodeType == XmlNodeType.Element:
                            AnchorPointY = reader.ReadContentAsFloat();
                            break;
                    }
            }
        }
    }
}