using System.Xml;

namespace ZMap.SLD
{
    public class Displacement
    {
        /// <summary>
        /// 标注在X轴位移的px值
        /// </summary>
        public float DisplacementX { get; set; }
        /// <summary>
        /// 标注在Y轴位移的px值
        /// </summary>
        public float DisplacementY { get; set; }
        public Displacement(){ }
        public Displacement(float displacementX, float displacementY)
        {
            DisplacementX = displacementX;
            DisplacementY = displacementY;
        }
        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.LocalName == "Displacement" && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                    switch (reader.LocalName)
                    {
                        case "DisplacementX" when reader.NodeType == XmlNodeType.Element:
                            DisplacementX = reader.ReadContentAsFloat();
                            break;
                        case "DisplacementY" when reader.NodeType == XmlNodeType.Element:
                            DisplacementY = reader.ReadContentAsFloat();
                            break;
                    }
            }
        }
    }
}