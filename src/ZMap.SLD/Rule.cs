using System;
using System.Collections.Generic;
using System.Xml;

namespace ZMap.SLD
{
    /// <summary>
    /// 规则
    /// </summary>
    public class Rule
    {
        /// <summary>
        /// 过滤条件
        /// </summary>
        public string Filter { get; set; }
        /// <summary>
        /// 特征
        /// </summary>
        public List<ISymbolizer> Symbolizers { get; set; }
        /// <summary>
        /// 最小比例尺
        /// </summary>
        public double Min { get; set; }
        /// <summary>
        /// 最大比例尺
        /// </summary>
        public double Max { get; set; }
        /// <summary>
        /// 缩放单位
        /// </summary>
        public ZoomUnits ZoomUnits { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 描述信息
        /// </summary>
        public Description Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public LegendGraphic LegendGraphic { get; set; }

        public Rule()
        {
            Symbolizers = new List<ISymbolizer>();
        }

        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.LocalName == "Rule" && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                    switch (reader.LocalName)
                    {
                        case "Unit" when reader.NodeType == XmlNodeType.Element:
                            {
                                var unit = reader.ReadString();
                                if (Enum.TryParse<ZoomUnits>(unit, false, out var zoomUnit))
                                {
                                    ZoomUnits = zoomUnit;
                                }
                                break;
                            }
                        case "MinScaleDenominator" when reader.NodeType == XmlNodeType.Element:
                            Min = (double)reader.ReadElementContentAs(typeof(double), null);
                            break;
                        case "MaxScaleDenominator" when reader.NodeType == XmlNodeType.Element:
                            Max = (double)reader.ReadElementContentAs(typeof(double), null);
                            break;
                        case "Name" when reader.NodeType == XmlNodeType.Element:
                            Name = reader.ReadString();
                            break;
                        case "Description" when reader.NodeType == XmlNodeType.Element:
                            {
                                var description = new Description();
                                description.ReadXml(reader);
                                Description = description;
                                break;
                            }
                        case "LineSymbolizer" when reader.NodeType == XmlNodeType.Element:
                            {
                                var lineSymbolizer = new LineSymbolizer();
                                lineSymbolizer.ReadXml(reader);
                                Symbolizers.Add(lineSymbolizer);
                                break;
                            }
                        case "PolygonSymbolizer" when reader.NodeType == XmlNodeType.Element:
                            {
                                var lineSymbolizer = new PolygonSymbolizer();
                                lineSymbolizer.ReadXml(reader);
                                Symbolizers.Add(lineSymbolizer);
                                break;
                            }
                        case "TextSymbolizer" when reader.NodeType == XmlNodeType.Element:
                            {
                                var textSymbolizer = new TextSymbolizer();
                                textSymbolizer.ReadXml(reader);
                                Symbolizers.Add(textSymbolizer);
                                break;
                            }
                        case "PointSymbolizer" when reader.NodeType == XmlNodeType.Element:
                            {
                                var pointSymbolizer = new PointSymbolizer();
                                pointSymbolizer.ReadXml(reader);
                                Symbolizers.Add(pointSymbolizer);
                                break;
                            }
                        case "RasterSymbolizer" when reader.NodeType == XmlNodeType.Element:
                            {
                                var rasterSymbolizer = new RasterSymbolizer();
                                rasterSymbolizer.ReadXml(reader);
                                Symbolizers.Add(rasterSymbolizer);
                                break;
                            }
                        case "Filter" when reader.NodeType == XmlNodeType.Element:
                            {
                                var filterBuilder = new Filter();
                                Filter = filterBuilder.ReadXml(reader);
                                break;
                            }
                        case "LegendGraphic" when reader.NodeType == XmlNodeType.Element:
                            {
                                var legendGraphic = new LegendGraphic();
                                legendGraphic.ReadXml(reader);
                                LegendGraphic = legendGraphic;
                                break;
                            }
                    }
            }
        }
    }
}