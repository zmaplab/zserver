using System.Collections.Generic;

namespace ZMap.Style;

public class StyleGroup : IVisibleLimit
{
    public CSharpExpression<bool?> Filter { get; set; }

    /// <summary>
    /// 样式组名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 最小显示范围
    /// </summary>
    public double MinZoom { get; set; }

    /// <summary>
    /// 最大显示范围
    /// </summary>
    public double MaxZoom { get; set; }

    /// <summary>
    /// 显示范围单位: 缩放级别、比例尺
    /// </summary>
    public ZoomUnits ZoomUnit { get; set; }

    /// <summary>
    /// 样式列表
    /// </summary>
    public List<Style> Styles { get; set; } = new();

    public StyleGroup Clone()
    {
        var styleGroup = new StyleGroup
        {
            Filter = Filter?.Clone(),
            Name = Name,
            Description = Description,
            MinZoom = MinZoom,
            MaxZoom = MaxZoom,
            ZoomUnit = ZoomUnit
        };

        foreach (var style in Styles)
        {
            styleGroup.Styles.Add(style.Clone());
        }

        return styleGroup;
    }

    public void Accept(IZMapStyleVisitor visitor, Feature feature)
    {
        if (Styles == null)
        {
            return;
        }

        Filter?.Invoke(feature);

        foreach (var style in Styles)
        {
            style.Accept(visitor, feature);
        }
    }
}