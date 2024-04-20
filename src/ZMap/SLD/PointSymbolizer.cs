using ZMap.Style;

namespace ZMap.SLD;

public class PointSymbolizer : Symbolizer
{
    /// <summary>
    /// 图像文件的位置。
    /// </summary>
    public OnlineResource OnlineResource { get; set; }

    /// <summary>
    /// 指定点符号的样式
    /// </summary>
    public Graphic Graphic { get; set; }

    public override object Accept(IStyleVisitor visitor, object extraData)
    {
        var symbolStyle = new SymbolStyle
        {
            MinZoom = 0,
            MaxZoom = Defaults.MaxZoomValue,
            Filter = CSharpExpression<bool?>.New(null)
        };
        visitor.Push(symbolStyle);
        visitor.Visit(Graphic, extraData);
        return null;
    }
}