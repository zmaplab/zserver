using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using ZMap.SLD.Filter.Expression;
using ZMap.Style;

namespace ZMap.SLD
{
    [XmlRoot("Fill")]
    public class Fill : IStyle
    {
        private static readonly Dictionary<string, dynamic> DefaultValues = new();

        static Fill()
        {
            DefaultValues.Add("fill", "#000000");
            DefaultValues.Add("fill-antialias", true);
            DefaultValues.Add("fill-opacity", 1);
            DefaultValues.Add("fill-translate", new[] { 0d, 0d });
        }

        [XmlElement("SvgParameter", typeof(SvgParameter))]
        [XmlElement("CssParameter", typeof(CssParameter))]
        [XmlChoiceIdentifier("ParametersElementName")]
        public NamedParameter[] Parameters { get; set; } = Array.Empty<NamedParameter>();

        /// <remarks/>
        [XmlElement("ParametersElementName")]
        [XmlIgnore]
        public NamedParameter.NamedParameterChoiceType[] ParametersElementName { get; set; }

        /// <summary>
        /// 填充时是否反锯齿（可选，默认值为 true）
        /// </summary>
        public ParameterValue Antialias => Parameters.GetOrDefault("fill-antialias");

        /// <summary>
        /// 填充的不透明度（可选，取值范围为 0 ~ 1，默认值为 1）
        /// fill-opacity
        /// </summary>
        public ParameterValue Opacity => Parameters.GetOrDefault("fill-opacity");

        /// <summary>
        /// 填充的颜色（可选，默认值为 #000000。如果设置了 Graphic， 则 color 将无效）
        /// </summary>
        public ParameterValue Color => Parameters.GetOrDefault("fill");

        /// <summary>
        /// 填充的平移（可选，通过平移 [x, y] 达到一定的偏移量。默认值为 [0, 0]，单位：像素。）
        /// </summary>
        public ParameterValue Translate => Parameters.GetOrDefault("fill-translate");

        /// <summary>
        /// 平移的锚点，即相对的参考物（可选，可选值为 map、viewport，默认为 map）
        /// </summary>
        public TranslateAnchor TranslateAnchor { get; set; } = TranslateAnchor.Map;

        /// <summary>
        /// TODO: 用于填充图案, 可以是线上图片
        /// </summary>
        public GraphicFill GraphicFill { get; set; }

        public object Accept(IStyleVisitor visitor, object extraData)
        {
            if (visitor.Pop() is not FillStyle fillStyle)
            {
                // TODO: 是不是应该报错？
                return null;
            }

            fillStyle.Color =
                Color.BuildExpression(visitor, extraData, (string)DefaultValues["fill"]);
            fillStyle.Opacity = Opacity.BuildExpression(visitor, extraData, (float?)DefaultValues["fill-opacity"]);

            var translate = Parameters.FirstOrDefault(x => x.Name == "fill-translate");
            fillStyle.Translate =
                translate.BuildArrayExpression(visitor as IExpressionVisitor, extraData,
                    (double[])DefaultValues["fill-translate"]);

            visitor.Push(fillStyle);

            return null;
        }
    }
}