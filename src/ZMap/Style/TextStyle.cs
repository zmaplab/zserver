namespace ZMap.Style
{
    public class TextStyle : VectorStyle, IFillStyle
    {
        public Expression<string> Label { get; set; }
        public Expression<string> Color { get; set; }
        public Expression<double[]> Translate { get; set; }
        public Expression<float> Opacity { get; set; }
        public Expression<string> BackgroundColor { get; set; }
        public Expression<float> BackgroundOpacity { get; set; }
        public Expression<float> Radius { get; set; }
        public Expression<string> RadiusColor { get; set; }
        public Expression<float> RadiusOpacity { get; set; }
        public Expression<string[]> Font { get; set; }
        public Expression<int> Size { get; set; }
        public Expression<string> Weight { get; set; }

        /// <summary>
        /// 斜体
        /// </summary>
        public Expression<string> Style { get; set; }

        public Expression<string> Align { get; set; }
        public Expression<float> Rotate { get; set; }
        public Expression<TextTransform> Transform { get; set; }
        public Expression<float[]> Offset { get; set; }
        public Expression<int> OutlineSize { get; set; }

        public override void Accept(IZMapStyleVisitor visitor, Feature feature)
        {
            base.Accept(visitor, feature);

            // 文本若是没有 Label 则无意义
            if (Label == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(Label.Body))
            {
                Label.Invoke(feature);
            }
            else
            {
                var result = feature[Label.Value];
                Label.Value = result;
            }

            Color?.Invoke(feature, "#000000");
            Opacity?.Invoke(feature, 1);
            BackgroundColor?.Invoke(feature);
            BackgroundOpacity?.Invoke(feature, 1);
            Radius?.Invoke(feature);
            RadiusColor?.Invoke(feature, "#000000");
            RadiusOpacity?.Invoke(feature, 1);
            Font?.Invoke(feature);
            Weight?.Invoke(feature);
            Size?.Invoke(feature, 24);
            Style?.Invoke(feature);
            Align?.Invoke(feature);
            Rotate?.Invoke(feature);
            Transform?.Invoke(feature);
            Offset?.Invoke(feature);
            OutlineSize?.Invoke(feature);
        }
    }
}