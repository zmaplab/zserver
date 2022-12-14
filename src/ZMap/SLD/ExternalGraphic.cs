using System.Xml.Serialization;

namespace ZMap.SLD
{
    public class ExternalGraphic : GraphicalSymbol
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlElement("OnlineResource")]
        public OnlineResource OnlineResource { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Format")]
        public string Format { get; set; }
    }
}