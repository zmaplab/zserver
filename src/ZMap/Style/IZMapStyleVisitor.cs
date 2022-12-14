namespace ZMap.Style;

public interface IZMapStyleVisitor
{
    void Visit(StyleGroup fill, Feature feature);
    void Visit(Style style, Feature feature);
    void Visit(FillStyle style, Feature feature);
    void Visit(LineStyle style, Feature feature);
    void Visit(TextStyle style, Feature feature);
    void Visit(RasterStyle style, Feature feature);
    void Visit(ResourceFillStyle style, Feature feature);
    void Visit(SpriteFillStyle style, Feature feature);
    void Visit(SpriteLineStyle style, Feature feature);
    void Visit(SymbolStyle style, Feature feature);
}