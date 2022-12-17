using System.Collections.Generic;
using System.Xml.Serialization;
using ZMap.SLD.Filter;

namespace ZMap.SLD
{
    /// <summary>
    ///   A Rule is used to attach property/scale conditions to and group
    ///   the individual symbols used for rendering.
    /// </summary>
    public class Rule
    {
        /// <summary>
        /// 名称
        /// </summary>
        [XmlElement("Name")]
        public string Name { get; set; }

        /// <summary>
        /// 描述信息
        /// </summary>
        [XmlElement("Description")]
        public Description Description { get; set; }

        /// <summary>
        /// 过滤条件
        /// </summary>
        [XmlElement("Filter")]
        public FilterType FilterType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("ElseFilter")]
        public bool ElseFilter { get; set; }

        /// <summary>
        /// 特征
        /// </summary>
        [XmlElement(ElementName = nameof(TextSymbolizer), Type = typeof(TextSymbolizer))]
        [XmlElement(ElementName = nameof(LineSymbolizer), Type = typeof(LineSymbolizer))]
        [XmlElement(ElementName = nameof(PolygonSymbolizer), Type = typeof(PolygonSymbolizer))]
        [XmlElement(ElementName = nameof(PointSymbolizer), Type = typeof(PointSymbolizer))]
        [XmlElement(ElementName = nameof(RasterSymbolizer), Type = typeof(RasterSymbolizer))]
        public List<Symbolizer> Symbolizers { get; set; }

        /// <summary>
        /// 缩放单位
        /// </summary>
        [XmlElement("MinScaleDenominator")]
        public double MinScaleDenominator { get; set; }

        /// <summary>
        /// 缩放单位
        /// </summary>
        [XmlElement("MaxScaleDenominator")]
        public double MaxScaleDenominator { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("LegendGraphic")]
        public LegendGraphic LegendGraphic { get; set; }

        public virtual void Accept(IStyleVisitor visitor, object data)
        {
            visitor.Visit(this, data);
        }
    }
}