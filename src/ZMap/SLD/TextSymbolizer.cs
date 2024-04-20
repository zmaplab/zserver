using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using ZMap.Style;

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
            Filter = CSharpExpression<bool?>.New(null),
            Label = CSharpExpression<string>.New(null, $"feature[\"{Label.PropertyName}\"]"),
            Offset = CSharpExpression<float[]>.New(Array.Empty<float>()),
            Color = CSharpExpression<string>.New("#000000"),
            Opacity = CSharpExpression<float?>.New(1),
            BackgroundColor = CSharpExpression<string>.New(null),
            BackgroundOpacity = CSharpExpression<float?>.New(1),
            Radius = CSharpExpression<float?>.New(0),
            RadiusColor = CSharpExpression<string>.New(null),
            RadiusOpacity = CSharpExpression<float?>.New(0),
            Weight = CSharpExpression<string>.New(null),
            Align = CSharpExpression<string>.New(null),
            Rotate = CSharpExpression<float?>.New(0),
            Transform = CSharpExpression<TextTransform>.New(TextTransform.Lowercase),
            OutlineSize = CSharpExpression<int?>.New(0),
            Font = CSharpExpression<List<string>>.New(new List<string>())
        };

        var size = Font.GetOrDefault("font-size");
        textStyle.Size =
            size.BuildExpression(visitor, extraData, (int?)FontType.Defaults["font-size"]);

        var families = Font.Where(x => x.Name == "font-family").ToList();
        foreach (var family in families)
        {
            family.Accept(visitor, extraData);
            var value = visitor.Pop();
            if (value != null)
            {
                textStyle.Font.Value.Add(value);
            }
        }

        Fill?.Accept(visitor, extraData);
        return null;
    }
}