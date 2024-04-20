namespace ZMap.Style;

public class RasterStyle : Style
{
    /// <summary>
    /// 绘制图片的不透明度。默认为 1.
    /// </summary>
    public CSharpExpression<float?> Opacity { get; set; }

    /// <summary>
    /// 在色轮上旋转色相的角度。默认为 0.
    /// </summary>
    public CSharpExpression<float?> HueRotate { get; set; }

    /// <summary>
    /// 增大或减少图片的亮度。此值是最小亮度。默认为 0.
    /// </summary>
    public CSharpExpression<float?> BrightnessMin { get; set; }

    /// <summary>
    /// 增大或减少图片的亮度。此值是最大亮度。默认为 1.
    /// </summary>
    public CSharpExpression<float?> BrightnessMax { get; set; }

    /// <summary>
    /// 增加或者减少图片的饱和度。默认为 0.
    /// </summary>
    public CSharpExpression<float?> Saturation { get; set; }

    /// <summary>
    /// 增加或者减少图片的对比度。默认为 0.
    /// </summary>
    public CSharpExpression<float?> Contrast { get; set; }

    public override void Accept(IZMapStyleVisitor visitor, Feature feature)
    {
    }

    public override Style Clone()
    {
        return new RasterStyle
        {
            MaxZoom = MaxZoom,
            MinZoom = MinZoom,
            ZoomUnit = ZoomUnit,
            Filter = Filter?.Clone(),
            Opacity = Opacity?.Clone(),
            HueRotate = HueRotate?.Clone(),
            BrightnessMin = BrightnessMin?.Clone(),
            BrightnessMax = BrightnessMax?.Clone(),
            Saturation = Saturation?.Clone(),
            Contrast = Contrast?.Clone()
        };
    }
}