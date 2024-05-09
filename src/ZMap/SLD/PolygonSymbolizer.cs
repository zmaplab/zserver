namespace ZMap.SLD;

public class PolygonSymbolizer : Symbolizer
{
    public OnlineResource OnlineResource { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("Stroke")]
    public Stroke Stroke { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("Fill")]
    public Fill Fill { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("Displacement")]
    public Displacement Displacement { get; set; }

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

        var fillStyle = new FillStyle
        {
            MinZoom = 0,
            MaxZoom = Defaults.MaxZoomValue,
            Filter = CSharpExpressionV2.Create<bool?>(null),
            Opacity = CSharpExpressionV2.Create<float?>("1"),
            Translate = CSharpExpressionV2.Create<double[]>("[]"),
            TranslateAnchor = CSharpExpressionV2.Create<TranslateAnchor?>("ZMap.Style.TranslateAnchor.Map")
        };
        visitor.Push(fillStyle);
        visitor.Visit(Fill, extraData);
        return null;
    }
}