using System;
using System.Xml.Serialization;
using ZMap.Style;

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
        [XmlElement("Fill")]
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
            var textStyle = new TextStyle
            {
                MinZoom = 0,
                MaxZoom = Defaults.MaxZoomValue,
                Filter = Expression<bool?>.New(null),
                Label = Expression<string>.New(null, $"feature[\"{Label.PropertyName}\"]"),
                Offset = Expression<float[]>.New(Array.Empty<float>()),
                Color = Expression<string>.New("#000000"),
                Opacity = Expression<float>.New(1),
                BackgroundColor = Expression<string>.New(null),
                BackgroundOpacity = Expression<float>.New(1),
                Radius = Expression<float>.New(0),
                RadiusColor = Expression<string>.New(null),
                RadiusOpacity = Expression<float>.New(0),
                Weight = Expression<string>.New(null),
                Align = Expression<string>.New(null),
                Rotate = Expression<float>.New(0),
                Transform = Expression<TextTransform>.New(TextTransform.Lowercase),
                OutlineSize = Expression<int>.New(0)
            };
            visitor.Push(textStyle);
            Font?.Accept(visitor, extraData);
            Fill?.Accept(visitor, extraData);
            return null;
        }
    }
}