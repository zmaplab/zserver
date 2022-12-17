using System;
using System.Xml.Serialization;
using ZMap.Style;

namespace ZMap.SLD
{
    public class LineSymbolizer : Symbolizer
    {
        public OnlineResource OnlineResource { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Stroke")]
        public Stroke Stroke { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public PerpendicularOffset PerpendicularOffset { get; set; }

        public override object Accept(IStyleVisitor visitor, object extraData)
        {
            var lineStyle = new LineStyle
            {
                MinZoom = 0,
                MaxZoom = Defaults.MaxZoomValue,
                Filter = Expression<bool?>.New(null),
                Translate = Expression<double[]>.New(Array.Empty<double>()),
                TranslateAnchor = Expression<TranslateAnchor>.New(TranslateAnchor.Map),
                GapWidth = Expression<int>.New(0),
                Gradient = Expression<int>.New(0),
                Offset = Expression<int>.New(0),
                LineCap = Expression<string>.New("round"),
                LineJoin = Expression<string>.New("round"),
                Opacity = Expression<float>.New(1),
                Blur = Expression<int>.New(0),
            };
            visitor.Push(lineStyle);
            visitor.Visit(Stroke, extraData);
            return null;
        }
    }
}