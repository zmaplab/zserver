using System.Xml.Serialization;

namespace ZMap.SLD
{
    public class Symbolizer
    {
        public string Geometry { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [XmlElement("Name")]
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [XmlElement("Description")]
        public Description Description { get; set; }

        // abstract object Accept(IStyleVisitor visitor, object extraData);
    }
}