namespace ZMap.SLD;

public class Stroke
{
    private static readonly Dictionary<string, dynamic> DefaultValues = new();

    static Stroke()
    {
        DefaultValues.Add("stroke", "#000000");
        DefaultValues.Add("stroke-width", 1);
        DefaultValues.Add("stroke-opacity", 1F);
        DefaultValues.Add("stroke-linejoin", null);
        DefaultValues.Add("stroke-linecap", null);
        DefaultValues.Add("stroke-dasharray", null);
        DefaultValues.Add("stroke-dashoffset", 0);
    }

    /// <summary>
    /// 说明： 暂时只支持单个表达式， GeoServer 亦是如此， 后续看为什么对 SLD 的支持不完整
    /// </summary>
    [XmlElement("SvgParameter", typeof(SvgParameter))]
    [XmlElement("CssParameter", typeof(CssParameter))]
    [XmlChoiceIdentifier("ParametersElementName")]
    public NamedParameter[] Parameters { get; set; } = Array.Empty<NamedParameter>();

    /// <remarks/>
    [XmlElement("ParametersElementName")]
    [XmlIgnore]
    public NamedParameter.NamedParameterChoiceType[] ParametersElementName { get; set; }


    /// <summary>
    /// 描边的颜色（可选， 默认和 fill-color 一致。如果设置了 fill-pattern/Graphic， 则 fill-outline-color 将无效。为了使用此属性， 还需要设置 fill-antialias 为 true）
    /// </summary>
    public ParameterValue Color => Parameters.GetOrDefault("stroke");

    public ParameterValue Width => Parameters.GetOrDefault("stroke-width");

    /// <summary>
    /// 填充的不透明度（可选，取值范围为 0 ~ 1，默认值为 1）
    /// </summary>
    public ParameterValue Opacity => Parameters.GetOrDefault("stroke-opacity");

    /// <summary>
    /// Indicates how the various segments of a (thick) line string should be joined.
    /// </summary>
    public ParameterValue LineJoin => Parameters.GetOrDefault("stroke-linejoin");

    /// <summary>
    /// "butt", "round", and "square"
    /// </summary>
    public ParameterValue LineCap => Parameters.GetOrDefault("stroke-linecap");

    /// <summary>
    /// 
    /// </summary>
    public ParameterValue DashArray => Parameters.GetOrDefault("stroke-dasharray");

    public ParameterValue DashOffset => Parameters.GetOrDefault("stroke-dashoffset");

    public GraphicFill GraphicFill { get; set; }
    public GraphicStroke GraphicStroke { get; set; }

    public void Accept(IStyleVisitor visitor, object extraData)
    {
        if (visitor.Pop() is not LineStyle strokeStyle)
        {
            return;
        }

        strokeStyle.Color =
            Color.BuildExpressionV2<string>(visitor, extraData);
        strokeStyle.Width =
            Width.BuildExpressionV2<int?>(visitor, extraData);
        strokeStyle.Opacity =
            Opacity.BuildExpressionV2<float?>(visitor, extraData);
        strokeStyle.LineJoin =
            LineJoin.BuildExpressionV2<string>(visitor, extraData);
        strokeStyle.LineCap =
            LineCap.BuildExpressionV2<string>(visitor, extraData);
        strokeStyle.DashOffset =
            DashOffset.BuildExpressionV2<float?>(visitor, extraData);
        strokeStyle.DashArray =
            DashArray.BuildArrayExpressionV2<float[]>(visitor as IExpressionVisitor, extraData);

        visitor.Push(strokeStyle);
    }
}