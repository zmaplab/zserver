namespace ZMap.Style;

public class ResourceFillStyle : FillStyle
{
    public CSharpExpressionV2<string> Uri { get; set; }

    public override void Accept(IZMapStyleVisitor visitor, Feature feature)
    {
        base.Accept(visitor, feature);

        Uri?.Accept(feature);
    }

    public override Style Clone()
    {
        return new ResourceFillStyle
        {
            MaxZoom = MaxZoom,
            MinZoom = MinZoom,
            ZoomUnit = ZoomUnit,
            Filter = Filter?.Clone(),
            Antialias = Antialias,
            Opacity = Opacity?.Clone(),
            Pattern = Pattern?.Clone(),
            Color = Color?.Clone(),
            Translate = Translate?.Clone(),
            TranslateAnchor = TranslateAnchor?.Clone(),
            Uri = Uri?.Clone()
        };
    }
}