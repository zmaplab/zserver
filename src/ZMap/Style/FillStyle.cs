namespace ZMap.Style
{
    public class FillStyle : VectorStyle
    {
        public bool Antialias { get; set; }
        public Expression<float> Opacity { get; set; }
        public Expression<string> Color { get; set; }
        public Expression<double[]> Translate { get; set; }
        public Expression<TranslateAnchor> TranslateAnchor { get; set; }

        public override void Accept(IZMapStyleVisitor visitor, Feature feature)
        {
            base.Accept(visitor, feature);

            Opacity?.Invoke(feature, 1);
            Color?.Invoke(feature, "#000000");
            Translate?.Invoke(feature);
            TranslateAnchor?.Invoke(feature);
        }
    }
}