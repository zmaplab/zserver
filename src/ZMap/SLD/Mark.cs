using System.Xml.Serialization;

namespace ZMap.SLD
{
    public class Mark : GraphicalSymbol
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlElement("WellKnownName")]
        public string WellKnownName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Fill Fill { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Stroke")]
        public Stroke Stroke { get; set; }
    }
}