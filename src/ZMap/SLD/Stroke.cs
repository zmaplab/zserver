using System.Collections.Generic;
using System.Xml.Serialization;
using ZMap.Style;

namespace ZMap.SLD
{
    public class Stroke
        : ParameterCollection
    {
        private static readonly Dictionary<string, dynamic> DefaultValues = new();

        static Stroke()
        {
            DefaultValues.Add("stroke", "#000000");
            DefaultValues.Add("stroke-width", 1);
            DefaultValues.Add("stroke-opacity", 1);
            DefaultValues.Add("stroke-linejoin", null);
            DefaultValues.Add("stroke-linecap", null);
            DefaultValues.Add("stroke-dasharray", null);
            DefaultValues.Add("stroke-dashoffset", 0);
        }

        protected override T GetDefault<T>(string name)
        {
            return DefaultValues.ContainsKey(name) ? DefaultValues[name] : default;
        }

        /// <summary>
        /// 描边的颜色（可选， 默认和 fill-color 一致。如果设置了 fill-pattern/Graphic， 则 fill-outline-color 将无效。为了使用此属性， 还需要设置 fill-antialias 为 true）
        /// </summary>
        [XmlElement("Color")]
        public ParameterValue Color => GetParameterValue<string>("stroke");

        public ParameterValue Width => GetParameterValue<int>("stroke-width");

        /// <summary>
        /// 填充的不透明度（可选，取值范围为 0 ~ 1，默认值为 1）
        /// </summary>
        public ParameterValue Opacity => GetParameterValue<float>("stroke-opacity");

        /// <summary>
        /// Indicates how the various segments of a (thick) line string should be joined.
        /// </summary>
        public ParameterValue LineJoin => GetParameterValue<string>("stroke-linejoin");

        /// <summary>
        /// "butt", "round", and "square"
        /// </summary>
        public ParameterValue LineCap => GetParameterValue<string>("stroke-linecap");

        /// <summary>
        /// 
        /// </summary>
        public ParameterValue DashArray => GetParameterValue<string>("stroke-dasharray");

        public ParameterValue DashOffset => GetParameterValue<float>("stroke-dashoffset");

        public GraphicFill GraphicFill { get; set; }
        public GraphicStroke GraphicStroke { get; set; }

        public void Accept(IStyleVisitor visitor, object extraData)
        {
            if (visitor.Pop() is not IStrokeStyle parent)
            {
                return;
            }

            parent.Color = Accept<string>(Color, visitor, extraData);
            parent.Width = Accept<int>(Width, visitor, extraData);
            parent.Opacity = Accept<float>(Opacity, visitor, extraData);
            parent.LineJoin = Accept<string>(LineJoin, visitor, extraData);
            parent.LineCap = Accept<string>(LineCap, visitor, extraData);
            parent.DashArray = GetArrayExpression<float>(DashArray, visitor, extraData);
            parent.DashOffset = Accept<float>(DashOffset, visitor, extraData);

            visitor.Push(parent);
        }
    }
}