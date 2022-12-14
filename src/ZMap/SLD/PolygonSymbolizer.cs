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
        public Fill Fill { get; set; }

        [XmlElement("Displacement")] public Displacement Displacement { get; set; }

        public override object Accept(IStyleVisitor visitor, object extraData)
        {
            var lineStyle = new LineStyle
            {
                MinZoom = 0,
                MaxZoom = Defaults.MaxZoomValue
            };
            visitor.Push(lineStyle);
            
            visitor.Visit(Stroke, extraData);

            var fillStyle = new FillStyle()
            {
                MinZoom = 0,
                MaxZoom = Defaults.MaxZoomValue
            };
            visitor.Push(fillStyle);
            visitor.Visit(Fill, extraData);
            return null;
        }
    }
}