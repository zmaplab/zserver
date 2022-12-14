using System.Xml.Serialization;

namespace ZMap.SLD
{
    public class TextSymbolizer : Symbolizer
    {
        public OnlineResource OnlineResource { get; set; }

        // /// <summary>
        // /// TODO: 可以支持表达式， 转换图形
        // /// </summary>
        // public Geometry Geometry { get; set; }

        /// <summary>
        /// 标签的文本内容。
        /// </summary>
        [XmlElement("Label")]
        public Label Label { get; set; }

        /// <summary>
        /// 标签文本的填充样式。
        /// </summary>
        public Fill Fill { get; set; }

        /// <summary>
        /// 标签的字体信息。
        /// </summary>
        [XmlElement("Font")]
        public Font Font { get; set; }

        /// <summary>
        /// 设置标签相对于其关联几何图形的位置。
        /// </summary>
        [XmlElement("LabelPlacement")]
        public LabelPlacement LabelPlacement { get; set; }

        /// <summary>
        /// 在标签文本周围创建彩色背景，以提高可读性。
        /// </summary>
        [XmlElement("Halo")]
        public Halo Halo { get; set; }

        /// <summary>
        /// 要在标签文本后面显示的图形。
        /// </summary>
        public Graphic Graphic { get; set; }
        
        public override object Accept(IStyleVisitor visitor, object extraData)
        {
            // visitor.Visit(Halo, extraData);
            // visitor.Visit(Graphic, extraData);
            return null;
        }
    }
}