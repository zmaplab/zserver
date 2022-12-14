namespace ZMap.Style;

public class ZMapStyleVisitor : IZMapStyleVisitor
{
    public void Visit(StyleGroup styleGroup, Feature feature)
    {
        styleGroup?.Accept(this, feature);
    }

    public void Visit(Style style, Feature feature)
    {
        switch (style)
        {
            case TextStyle textStyle:
                textStyle.Accept(this, feature);
                break;
            case RasterStyle rasterStyle:
                rasterStyle.Accept(this, feature);
                break;
            case SpriteFillStyle spriteFillStyle:
                spriteFillStyle.Accept(this, feature);
                break;
            case ResourceFillStyle resourceFillStyle:
                resourceFillStyle.Accept(this, feature);
                break;
            case FillStyle fillStyle:
                fillStyle.Accept(this, feature);
                break;
            case SpriteLineStyle spriteLineStyle:
                spriteLineStyle.Accept(this, feature);
                break;
            case LineStyle lineStyle:
                lineStyle.Accept(this, feature);
                break;
            case SymbolStyle symbolStyle:
                symbolStyle.Accept(this, feature);
                break;
        }
    }

    public void Visit(FillStyle style, Feature feature)
    {
        style.Accept(this, feature);
    }

    public void Visit(LineStyle style, Feature feature)
    {
        style.Accept(this, feature);
    }

    public void Visit(TextStyle style, Feature feature)
    {
        style.Accept(this, feature);
    }

    public void Visit(RasterStyle style, Feature feature)
    {
        style.Accept(this, feature);
    }

    public void Visit(ResourceFillStyle style, Feature feature)
    {
        style.Accept(this, feature);
    }

    public void Visit(SpriteFillStyle style, Feature feature)
    {
        style.Accept(this, feature);
    }

    public void Visit(SpriteLineStyle style, Feature feature)
    {
        style.Accept(this, feature);
    }

    public void Visit(SymbolStyle style, Feature feature)
    {
        style.Accept(this, feature);
    }
}