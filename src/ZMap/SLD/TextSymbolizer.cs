namespace ZMap.SLD;

public class TextSymbolizer : Symbolizer
{
    public OnlineResource OnlineResource { get; set; }

    // /// <summary>
    // /// TODO: 可以支持表达式， 转换图形
    // /// </summary>
    // public Geometry Geometry { get; set; }

    /// <summary>
    /// 标签的文本内容。
    /// </summary>
    [XmlElement("Label")]
    public Label Label { get; set; }

    /// <summary>
    /// 标签的字体信息。
    /// </summary>
    [XmlArrayItem("SvgParameter", typeof(SvgParameter))]
    [XmlArrayItem("CssParameter", typeof(CssParameter))]
    public NamedParameter[] Font { get; set; } = Array.Empty<NamedParameter>();

    /// <summary>
    /// 设置标签相对于其关联几何图形的位置。
    /// </summary>
    [XmlElement("LabelPlacement")]
    public LabelPlacement LabelPlacement { get; set; }

    /// <summary>
    /// 在标签文本周围创建彩色背景，以提高可读性。
    /// </summary>
    [XmlElement("Halo")]
    public Halo Halo { get; set; }

    /// <summary>
    /// 标签文本的填充样式。
    /// </summary>
    [XmlElement("Fill")]
    public Fill Fill { get; set; }

    // /// <summary>
    // /// 要在标签文本后面显示的图形。
    // /// </summary>
    // public Graphic Graphic { get; set; }

    public override object Accept(IStyleVisitor visitor, object extraData)
    {
        var textStyle = new TextStyle
        {
            MinZoom = 0,
            MaxZoom = Defaults.MaxZoomValue,
            Filter = CSharpExpressionV2.Create<bool?>(null),
            Label = CSharpExpressionV2.Create<string>($"{{{{ feature[\"{Label.PropertyName}\"] }}}}"),
            Offset = CSharpExpressionV2.Create<float[]>("{{ default }}"),
            Color = CSharpExpressionV2.Create<string>("#000000"),
            Opacity = CSharpExpressionV2.Create<float?>("1"),
            BackgroundColor = CSharpExpressionV2.Create<string>(null),
            BackgroundOpacity = CSharpExpressionV2.Create<float?>("1"),
            Radius = CSharpExpressionV2.Create<float?>("0"),
            RadiusColor = CSharpExpressionV2.Create<string>(null),
            RadiusOpacity = CSharpExpressionV2.Create<float?>("0"),
            Weight = CSharpExpressionV2.Create<string>(null),
            Align = CSharpExpressionV2.Create<string>(null),
            Rotate = CSharpExpressionV2.Create<float?>("0"),
            Transform = CSharpExpressionV2.Create<TextTransform>("Lowercase"),
            OutlineSize = CSharpExpressionV2.Create<int?>("0")
        };

        var size = Font.GetOrDefault("font-size");
        textStyle.Size =
            size.BuildExpressionV2<int?>(visitor, extraData);

        var families = Font.Where(x => x.Name == "font-family").ToList();
        var fontList = new StringBuilder();
        fontList.Append("new List<string>() {");
        for (var i = 0; i < families.Count; ++i)
        {
            var family = families[i];
            family.Accept(visitor, extraData);

            if (visitor.Pop() is string value)
            {
                fontList.Append('"').Append(value).Append('"');
            }

            if (i < families.Count - 1)
            {
                fontList.Append(',');
            }
        }

        fontList.Append('}');
        textStyle.Font = CSharpExpressionV2.Create<List<string>>(fontList.ToString());
        Fill?.Accept(visitor, extraData);
        return null;
    }
}