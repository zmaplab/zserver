namespace ZMap.Style;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class SymbolStyle : VectorStyle
{
    public CSharpExpressionV2<int?> Size { get; set; }
    public CSharpExpressionV2<string> Uri { get; set; }
    public CSharpExpressionV2<float?> Opacity { get; set; }
    public CSharpExpressionV2<float?> Rotation { get; set; }

    public override void Accept(IZMapStyleVisitor visitor, Feature feature)
    {
        base.Accept(visitor, feature);

        Size?.Accept(feature, 24);
        Uri?.Accept(feature);
        Opacity?.Accept(feature, 1);
        Rotation?.Accept(feature);
    }

    public override Style Clone()
    {
        return new SymbolStyle
        {
            MaxZoom = MaxZoom,
            MinZoom = MinZoom,
            ZoomUnit = ZoomUnit,
            Filter = Filter?.Clone(),
            Size = Size?.Clone(),
            Uri = Uri?.Clone(),
            Opacity = Opacity?.Clone(),
            Rotation = Rotation?.Clone()
        };
    }
}