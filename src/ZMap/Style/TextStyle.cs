using System.Collections.Generic;

namespace ZMap.Style
{
    public class TextStyle : VectorStyle, IFillStyle
    {
        public CSharpExpression<string> Label { get; set; }
        public CSharpExpression<string> Color { get; set; }
        public CSharpExpression<double[]> Translate { get; set; }
        public CSharpExpression<float> Opacity { get; set; }
        public CSharpExpression<string> BackgroundColor { get; set; }
        public CSharpExpression<float> BackgroundOpacity { get; set; }
        public CSharpExpression<float> Radius { get; set; }
        public CSharpExpression<string> RadiusColor { get; set; }
        public CSharpExpression<float> RadiusOpacity { get; set; }
        public CSharpExpression<List<string>> Font { get; set; }
        public CSharpExpression<int> Size { get; set; }
        public CSharpExpression<string> Weight { get; set; }

        /// <summary>
        /// 斜体
        /// </summary>
        public CSharpExpression<string> Style { get; set; }

        public CSharpExpression<string> Align { get; set; }
        public CSharpExpression<float> Rotate { get; set; }
        public CSharpExpression<TextTransform> Transform { get; set; }
        public CSharpExpression<float[]> Offset { get; set; }
        public CSharpExpression<int> OutlineSize { get; set; }

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