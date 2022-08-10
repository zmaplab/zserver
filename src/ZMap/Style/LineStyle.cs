namespace ZMap.Style
{
    public class LineStyle : VectorStyle
    {
        public Expression<float> Opacity { get; set; }
        public Expression<int> Width { get; set; }
        public Expression<string> Color { get; set; }
        public Expression<float[]> DashArray { get; set; }
        public Expression<float> DashOffset { get; set; }
        public Expression<string> LineJoin { get; set; }
        public Expression<string> Cap { get; set; }
        public Expression<double[]> Translate { get; set; }
        public Expression<TranslateAnchor> TranslateAnchor { get; set; }
        public Expression<int> GapWidth { get; set; }
        public Expression<int> Offset { get; set; }
        public Expression<int> Blur { get; set; }

        /// <summary>
        /// Disabled by dasharray. Disabled by pattern
        /// </summary>
        public Expression<int> Gradient { get; set; }
    }
}