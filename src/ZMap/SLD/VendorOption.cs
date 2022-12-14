using System.Xml.Serialization;

namespace ZMap.SLD
{
    public class VendorOption
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlText]
        public string Value { get; set; }
    }
}