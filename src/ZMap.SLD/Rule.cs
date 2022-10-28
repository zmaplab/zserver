using System.Collections.Generic;
using System.Linq;
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
        // [XmlElement(ElementName = "NamedLayer", Type = typeof(NamedLayer))]
        // [XmlElement(ElementName = "UserLayer", Type = typeof(UserLayer))]
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

        public Rule()
        {
        }

        public virtual void Accept(IStyleVisitor visitor, object data)
        {
            visitor.Visit(this, data);
        }

        // public void ReadXml(XmlReader reader)
        // {
        //     while (reader.Read())
        //     {
        //         if (reader.LocalName == "Rule" && reader.NodeType == XmlNodeType.EndElement)
        //         {
        //             break;
        //         }
        //         else
        //             switch (reader.LocalName)
        //             {
        //                 case "Unit" when reader.NodeType == XmlNodeType.Element:
        //                     {
        //                         var unit = reader.ReadString();
        //                         if (Enum.TryParse<ZoomUnits>(unit, false, out var zoomUnit))
        //                         {
        //                             ZoomUnits = zoomUnit;
        //                         }
        //                         break;
        //                     }
        //                 case "MinScaleDenominator" when reader.NodeType == XmlNodeType.Element:
        //                     Min = (double)reader.ReadElementContentAs(typeof(double), null);
        //                     break;
        //                 case "MaxScaleDenominator" when reader.NodeType == XmlNodeType.Element:
        //                     Max = (double)reader.ReadElementContentAs(typeof(double), null);
        //                     break;
        //                 case "Name" when reader.NodeType == XmlNodeType.Element:
        //                     Name = reader.ReadString();
        //                     break;
        //                 case "Description" when reader.NodeType == XmlNodeType.Element:
        //                     {
        //                         var description = new Description();
        //                         description.ReadXml(reader);
        //                         Description = description;
        //                         break;
        //                     }
        //                 case "LineSymbolizer" when reader.NodeType == XmlNodeType.Element:
        //                     {
        //                         var lineSymbolizer = new LineSymbolizer();
        //                         lineSymbolizer.ReadXml(reader);
        //                         Symbolizers.Add(lineSymbolizer);
        //                         break;
        //                     }
        //                 case "PolygonSymbolizer" when reader.NodeType == XmlNodeType.Element:
        //                     {
        //                         var lineSymbolizer = new PolygonSymbolizer();
        //                         lineSymbolizer.ReadXml(reader);
        //                         Symbolizers.Add(lineSymbolizer);
        //                         break;
        //                     }
        //                 case "TextSymbolizer" when reader.NodeType == XmlNodeType.Element:
        //                     {
        //                         var textSymbolizer = new TextSymbolizer();
        //                         textSymbolizer.ReadXml(reader);
        //                         Symbolizers.Add(textSymbolizer);
        //                         break;
        //                     }
        //                 case "PointSymbolizer" when reader.NodeType == XmlNodeType.Element:
        //                     {
        //                         var pointSymbolizer = new PointSymbolizer();
        //                         pointSymbolizer.ReadXml(reader);
        //                         Symbolizers.Add(pointSymbolizer);
        //                         break;
        //                     }
        //                 case "RasterSymbolizer" when reader.NodeType == XmlNodeType.Element:
        //                     {
        //                         var rasterSymbolizer = new RasterSymbolizer();
        //                         rasterSymbolizer.ReadXml(reader);
        //                         Symbolizers.Add(rasterSymbolizer);
        //                         break;
        //                     }
        //                 case "Filter" when reader.NodeType == XmlNodeType.Element:
        //                     {
        //                         var filterBuilder = new Filter();
        //                         Filter = filterBuilder.ReadXml(reader);
        //                         break;
        //                     }
        //                 case "LegendGraphic" when reader.NodeType == XmlNodeType.Element:
        //                     {
        //                         var legendGraphic = new LegendGraphic();
        //                         legendGraphic.ReadXml(reader);
        //                         LegendGraphic = legendGraphic;
        //                         break;
        //                     }
        //             }
        //     }
        // }
    }
}