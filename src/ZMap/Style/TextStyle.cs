namespace ZMap.Style;

public class TextStyle : VectorStyle
{
    public CSharpExpressionV2<string> Label { get; set; }
    public CSharpExpressionV2<string> Color { get; set; }
    public CSharpExpressionV2<double[]> Translate { get; set; }
    public CSharpExpressionV2<float?> Opacity { get; set; }
    public CSharpExpressionV2<string> BackgroundColor { get; set; }
    public CSharpExpressionV2<float?> BackgroundOpacity { get; set; }
    public CSharpExpressionV2<float?> Radius { get; set; }
    public CSharpExpressionV2<string> RadiusColor { get; set; }
    public CSharpExpressionV2<float?> RadiusOpacity { get; set; }
    public CSharpExpressionV2<List<string>> Font { get; set; }
    public CSharpExpressionV2<int?> Size { get; set; }
    public CSharpExpressionV2<string> Weight { get; set; }

    /// <summary>
    /// 斜体
    /// </summary>
    public CSharpExpressionV2<string> Style { get; set; }

    public CSharpExpressionV2<string> Align { get; set; }
    public CSharpExpressionV2<float?> Rotate { get; set; }
    public CSharpExpressionV2<TextTransform> Transform { get; set; }
    public CSharpExpressionV2<float[]> Offset { get; set; }
    public CSharpExpressionV2<int?> OutlineSize { get; set; }

    public override void Accept(IZMapStyleVisitor visitor, Feature feature)
    {
        base.Accept(visitor, feature);

        // 文本若是没有 Label 则无意义
        if (Label == null)
        {
            return;
        }

        // if (!string.IsNullOrWhiteSpace(Label.Expression))
        // {
        //     Label.Accept(feature);
        // }
        // else
        // {
        //     var result = feature[Label.Value];
        //     Label.Value = result;
        // }

        Label.Accept(feature);
        Color?.Accept(feature, "#000000");
        Opacity?.Accept(feature, 1);
        BackgroundColor?.Accept(feature);
        BackgroundOpacity?.Accept(feature, 1);
        Radius?.Accept(feature);
        RadiusColor?.Accept(feature, "#000000");
        RadiusOpacity?.Accept(feature, 1);
        Font?.Accept(feature);
        Weight?.Accept(feature);
        Size?.Accept(feature, 24);
        Style?.Accept(feature);
        Align?.Accept(feature);
        Rotate?.Accept(feature);
        Transform?.Accept(feature);
        Offset?.Accept(feature);
        OutlineSize?.Accept(feature, 2);
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