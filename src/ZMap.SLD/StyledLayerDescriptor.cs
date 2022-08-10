using System.Collections.Generic;
using System.Xml;

namespace ZMap.SLD
{
    public class StyledLayerDescriptor
    {
        /// <summary>
        /// 对服务器目录中命名层的引用
        /// </summary>
        public List<NamedLayer> NamedLayers { get; set; }

        public StyledLayerDescriptor()
        {
            NamedLayers = new List<NamedLayer>();
        }

        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.EOF || (reader.NodeType == XmlNodeType.EndElement && reader.Name == "StyledLayerDescriptor"))
                {
                    break;
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "NamedLayer")
                {
                    var namedLayer = new NamedLayer();
                    namedLayer.ReadXml(reader);
                    NamedLayers.Add(namedLayer);
                }
                else if (reader.LocalName == "StyledLayerDescriptor" && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
            }
        }
    }
}