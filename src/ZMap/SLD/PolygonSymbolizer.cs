using System;
using System.Xml.Serialization;
using ZMap.Style;

namespace ZMap.SLD
{
    public class PolygonSymbolizer : Symbolizer
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
        [XmlElement("Fill")]
        public Fill Fill { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Displacement")]
        public Displacement Displacement { get; set; }

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

            var fillStyle = new FillStyle
            {
                MinZoom = 0,
                MaxZoom = Defaults.MaxZoomValue,
                Filter = Expression<bool?>.New(null),
                Opacity = Expression<float>.New(1),
                Translate = Expression<double[]>.New(Array.Empty<double>()),
                TranslateAnchor = Expression<TranslateAnchor>.New(TranslateAnchor.Map),
            };
            visitor.Push(fillStyle);
            visitor.Visit(Fill, extraData);
            return null;
        }
    }
}