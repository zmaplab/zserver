namespace ZMap.SLD;

public class RasterSymbolizer : Symbolizer
{
    public OnlineResource OnlineResource { get; set; }
    public float Opacity { get; set; }
    public ChannelSelection ChannelSelection { get; set; }
    public int OverlapBehavior { get; set; }
    public ColorMap ColorMap { get; set; }
    public ContrastEnhancement ContrastEnhancement { get; set; }
    public ShadedRelief ShadedRelief { get; set; }

    // public ImageOutline ImageOutline { get; set; }

    public override object Accept(IStyleVisitor visitor, object extraData)
    {
        return null;
    }
}