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
                MaxZoom = Defaults.MaxZoomValue
            };
            visitor.Push(lineStyle);
            visitor.Visit(Stroke, extraData);
            return null;
        }
    }
}