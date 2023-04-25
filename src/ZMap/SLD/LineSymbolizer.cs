using System;
using System.Xml.Serialization;
using ZMap.Style;

namespace ZMap.SLD
{
    public class LineSymbolizer : Symbolizer
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlElement("OnlineResource")]
        public OnlineResource OnlineResource { get; set; }

        /// <summary>
        /// 
        /// </summary>
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
                Filter = CSharpExpression<bool?>.New(null),
                Translate = CSharpExpression<double[]>.New(Array.Empty<double>()),
                TranslateAnchor = CSharpExpression<TranslateAnchor>.New(TranslateAnchor.Map),
                GapWidth = CSharpExpression<int>.New(0),
                Gradient = CSharpExpression<int>.New(0),
                Offset = CSharpExpression<int>.New(0),
                LineCap = CSharpExpression<string>.New("round"),
                LineJoin = CSharpExpression<string>.New("round"),
                Opacity = CSharpExpression<float>.New(1),
                Blur = CSharpExpression<int>.New(0),
            };
            visitor.Push(lineStyle);
            visitor.Visit(Stroke, extraData);
            return null;
        }
    }
}