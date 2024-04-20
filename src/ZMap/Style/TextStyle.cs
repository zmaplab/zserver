using System.Collections.Generic;

namespace ZMap.Style;

public class TextStyle : VectorStyle
{
    public CSharpExpression<string> Label { get; set; }
    public CSharpExpression<string> Color { get; set; }
    public CSharpExpression<double[]> Translate { get; set; }
    public CSharpExpression<float?> Opacity { get; set; }
    public CSharpExpression<string> BackgroundColor { get; set; }
    public CSharpExpression<float?> BackgroundOpacity { get; set; }
    public CSharpExpression<float?> Radius { get; set; }
    public CSharpExpression<string> RadiusColor { get; set; }
    public CSharpExpression<float?> RadiusOpacity { get; set; }
    public CSharpExpression<List<string>> Font { get; set; }
    public CSharpExpression<int?> Size { get; set; }
    public CSharpExpression<string> Weight { get; set; }

    /// <summary>
    /// 斜体
    /// </summary>
    public CSharpExpression<string> Style { get; set; }

    public CSharpExpression<string> Align { get; set; }
    public CSharpExpression<float?> Rotate { get; set; }
    public CSharpExpression<TextTransform> Transform { get; set; }
    public CSharpExpression<float[]> Offset { get; set; }
    public CSharpExpression<int?> OutlineSize { get; set; }

    public override void Accept(IZMapStyleVisitor visitor, Feature feature)
    {
        base.Accept(visitor, feature);

        // 文本若是没有 Label 则无意义
        if (Label == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(Label.Expression))
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
        OutlineSize?.Invoke(feature, 2);
    }

    public override Style Clone()
    {
        return new TextStyle
        {
            MaxZoom = MaxZoom,
            MinZoom = MinZoom,
            ZoomUnit = ZoomUnit,
            Filter = Filter?.Clone(),
            Label = Label?.Clone(),
            Color = Color?.Clone(),
            Translate = Translate?.Clone(),
            Opacity = Opacity?.Clone(),
            BackgroundColor = BackgroundColor?.Clone(),
            BackgroundOpacity = BackgroundOpacity?.Clone(),
            Radius = Radius?.Clone(),
            RadiusColor = RadiusColor?.Clone(),
            RadiusOpacity = RadiusOpacity?.Clone(),
            Font = Font?.Clone(),
            Size = Size?.Clone(),
            Weight = Weight?.Clone(),
            Style = Style?.Clone(),
            Align = Align?.Clone(),
            Rotate = Rotate?.Clone(),
            Transform = Transform?.Clone(),
            Offset = Offset?.Clone(),
            OutlineSize = OutlineSize?.Clone()
        };
    }
}