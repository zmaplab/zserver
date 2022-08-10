namespace ZMap.Style
{
    public class FillStyle : VectorStyle
    {
        public bool Antialias { get; set; }
        public Expression<float> Opacity { get; set; }
        public Expression<string> Color { get; set; }
        public Expression<double[]> Translate { get; set; }
        public Expression<TranslateAnchor> TranslateAnchor { get; set; }
    }
}