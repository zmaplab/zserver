using System.Collections.Generic;
using System.Xml.Serialization;
using ZMap.Style;

namespace ZMap.SLD
{
    [XmlRoot("Fill")]
    public class Fill : ParameterCollection, IStyle
    {
        private static readonly Dictionary<string, dynamic> Defaults = new();

        static Fill()
        {
            Defaults.Add("fill", "#000000");
            Defaults.Add("fill-antialias", true);
            Defaults.Add("fill-opacity", 1);
            Defaults.Add("fill-translate", "0 0");
        }

        protected override T GetDefault<T>(string name)
        {
            return Defaults.ContainsKey(name) ? Defaults[name] : default;
        }

        /// <summary>
        /// 填充时是否反锯齿（可选，默认值为 true）
        /// </summary>
        public ParameterValue Antialias => GetParameterValue<bool>("fill-antialias");

        /// <summary>
        /// 填充的不透明度（可选，取值范围为 0 ~ 1，默认值为 1）
        /// fill-opacity
        /// </summary>
        public ParameterValue Opacity => GetParameterValue<float>("fill-opacity");

        /// <summary>
        /// 填充的颜色（可选，默认值为 #000000。如果设置了 Graphic， 则 color 将无效）
        /// </summary>
        public ParameterValue Color => GetParameterValue<string>("fill");

        /// <summary>
        /// 填充的平移（可选，通过平移 [x, y] 达到一定的偏移量。默认值为 [0, 0]，单位：像素。）
        /// </summary>
        public ParameterValue Translate => GetParameterValue<string>("fill-translate");

        // /// <summary>
        // /// 平移的锚点，即相对的参考物（可选，可选值为 map、viewport，默认为 map）
        // /// </summary>
        // public TranslateAnchor TranslateAnchor { get; set; } = TranslateAnchor.Map;

        /// <summary>
        /// TODO: 用于填充图案, 可以是线上图片
        /// </summary>
        public GraphicFill GraphicFill { get; set; }

        public object Accept(IStyleVisitor visitor, object extraData)
        {
            if (visitor.Pop() is not IFillStyle parent)
            {
                return null;
            }

            parent.Color = Accept<string>(Color, visitor, extraData);
            parent.Opacity = Accept<float>(Opacity, visitor, extraData);
            parent.Translate = GetArrayExpression<double>(Translate, visitor, extraData);

            visitor.Push(parent);

            return null;
        }
    }
}