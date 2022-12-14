using System.Linq;

namespace ZMap.Style
{
    public class LineStyle : VectorStyle, IStrokeStyle
    {
        public Expression<float> Opacity { get; set; }
        public Expression<int> Width { get; set; }
        public Expression<string> Color { get; set; }
        public Expression<float[]> DashArray { get; set; }
        public Expression<float> DashOffset { get; set; }
        public Expression<string> LineJoin { get; set; }
        public Expression<string> LineCap { get; set; }
        public Expression<double[]> Translate { get; set; }
        public Expression<TranslateAnchor> TranslateAnchor { get; set; }
        public Expression<int> GapWidth { get; set; }
        public Expression<int> Offset { get; set; }
        public Expression<int> Blur { get; set; }

        /// <summary>
        /// Disabled by dasharray. Disabled by pattern
        /// </summary>
        public Expression<int> Gradient { get; set; }

        public override void Accept(IZMapStyleVisitor visitor, Feature feature)
        {
            base.Accept(visitor, feature);

            Opacity?.Invoke(feature, 1);
            Width?.Invoke(feature, 1);
            Color?.Invoke(feature, "#000000");
            DashArray?.Invoke(feature);
            LineJoin?.Invoke(feature);
            LineCap?.Invoke(feature);
            Translate?.Invoke(feature);
            TranslateAnchor?.Invoke(feature);
            GapWidth?.Invoke(feature);
            Offset?.Invoke(feature);
            Blur?.Invoke(feature);
            Gradient?.Invoke(feature);
        }
    }
}