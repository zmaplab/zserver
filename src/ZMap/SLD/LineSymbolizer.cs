namespace ZMap.SLD;

public class LineSymbolizer : Symbolizer
{
    /// <summary>
    /// 
    /// </summary>
    [XmlElement("OnlineResource")]
    public OnlineResource OnlineResource { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Stroke Stroke { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public PerpendicularOffset PerpendicularOffset { get; set; }

    public override object Accept(IStyleVisitor visitor, object extraData)
    {
        var lineStyle = new LineStyle
        {
            MinZoom = 0,
            MaxZoom = Defaults.MaxZoomValue,
            Filter = CSharpExpressionV2.Create<bool?>(null),
            Translate = CSharpExpressionV2.Create<double[]>("{{ default }}"),
            TranslateAnchor = CSharpExpressionV2.Create<TranslateAnchor>("Map"),
            GapWidth = CSharpExpressionV2.Create<int?>("0"),
            Gradient = CSharpExpressionV2.Create<int?>("0"),
            Offset = CSharpExpressionV2.Create<int?>("0"),
            LineCap = CSharpExpressionV2.Create<string>("round"),
            LineJoin = CSharpExpressionV2.Create<string>("round"),
            Opacity = CSharpExpressionV2.Create<float?>("1"),
            Blur = CSharpExpressionV2.Create<int?>("0"),
        };
        visitor.Push(lineStyle);
        visitor.Visit(Stroke, extraData);
        return null;
    }
}