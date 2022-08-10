using System.Xml;

namespace ZMap.SLD
{
    public class TextSymbolizer : ISymbolizer
    {
        public OnlineResource OnlineResource { get; set; }
        /// <summary>
        /// TODO: 可以支持表达式， 转换图形
        /// </summary>
        public Geometry Geometry { get; set; }
        /// <summary>
        /// 标签的文本内容。
        /// </summary>
        public Label Label { get; set; }
        /// <summary>
        /// 标签文本的填充样式。
        /// </summary>
        public Fill Fill { get; set; }
        /// <summary>
        /// 标签的字体信息。
        /// </summary>
        public Font Font { get; set; }
        /// <summary>
        /// 设置标签相对于其关联几何图形的位置。
        /// </summary>
        public LabelPlacement LabelPlacement { get; set; }
        /// <summary>
        /// 要在标签文本后面显示的图形。
        /// </summary>
        public Graphic Graphic { get; set; }
        /// <summary>
        /// 在标签文本周围创建彩色背景，以提高可读性。
        /// </summary>
        public Halo Halo { get; set; }
        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.LocalName == "TextSymbolizer" && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                    switch (reader.LocalName)
                    {
                        case "OnlineResource" when reader.NodeType == XmlNodeType.Element:
                            {
                                var onlineResource = new OnlineResource();
                                onlineResource.ReadXml(reader);
                                OnlineResource = onlineResource;
                                break;
                            }
                        case "Geometry" when reader.NodeType == XmlNodeType.Element:
                            {
                                var geometry = new Geometry();
                                geometry.ReadXml(reader);
                                Geometry = geometry;
                                break;
                            }

                        case "Label" when reader.NodeType == XmlNodeType.Element:
                            {
                                var label = new Label();
                                label.ReadXml(reader);
                                Label = label;
                                break;
                            }

                        case "Fill" when reader.NodeType == XmlNodeType.Element:
                            {
                                var fill = new Fill();
                                fill.ReadXml(reader);
                                Fill = fill;
                                break;
                            }

                        case "Font" when reader.NodeType == XmlNodeType.Element:
                            {
                                var font = new Font();
                                font.ReadXml(reader);
                                Font = font;
                                break;
                            }

                        case "LabelPlacement" when reader.NodeType == XmlNodeType.Element:
                            {
                                var labelPlacement = new LabelPlacement();
                                labelPlacement.ReadXml(reader);
                                LabelPlacement = labelPlacement;
                                break;
                            }

                        case "Graphic" when reader.NodeType == XmlNodeType.Element:
                            {
                                var graphic = new Graphic();
                                graphic.ReadXml(reader);
                                Graphic = graphic;
                                break;
                            }

                        case "Halo" when reader.NodeType == XmlNodeType.Element:
                            {
                                var halo = new Halo();
                                halo.ReadXml(reader);
                                Halo = halo;
                                break;
                            }
                    }
            }
        }
    }
}