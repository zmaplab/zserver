namespace ZMap.Style;

public class SpriteFillStyle : ResourceFillStyle
{
    public override void Accept(IZMapStyleVisitor visitor, Feature feature)
    {
        base.Accept(visitor, feature);

        Pattern?.Accept(feature);
    }

    public override Style Clone()
    {
        return new SpriteFillStyle
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