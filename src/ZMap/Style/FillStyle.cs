using System;

namespace ZMap.Style
{
    public class FillStyle : VectorStyle, IFillStyle
    {
        /// <summary>
        /// 填充时是否反锯齿（可选，默认值为 true）
        /// </summary>
        public bool Antialias { get; set; } = true;

        /// <summary>
        /// 填充的不透明度（可选，取值范围为 0 ~ 1，默认值为 1）
        /// </summary>
        public Expression<float> Opacity { get; set; }

        /// <summary>
        /// 填充用的图案（可选，这里填写在 sprite 雪碧图中图标名称。为了图案能无缝填充，图标的高宽需要是 2 的倍数）
        /// </summary>
        public Expression<string> Pattern { get; set; }

        /// <summary>
        /// 填充的颜色（可选，默认值为 #000000。如果设置了 pattern，则 color 将无效）
        /// </summary>
        public Expression<string> Color { get; set; }

        /// <summary>
        /// 描边的颜色（可选，默认和 color 一致。如果设置了 pattern，则 outline-color 将无效。为了使用此属性，还需要设置 antialias 为 true）
        /// </summary>
        [Obsolete("没有描边粗细的设计， 建议使用 LineStyle 绘制")]
        public Expression<string> OutlineColor { get; set; }

        /// <summary>
        /// 填充的平移（可选，通过平移 [x, y] 达到一定的偏移量。默认值为 [0, 0]，单位：像素。）
        /// </summary>
        public Expression<double[]> Translate { get; set; }

        /// <summary>
        /// 填充的平移的锚点，即相对的参考物（可选，可选值为 map、viewport，默认为 map）
        /// </summary>
        public Expression<TranslateAnchor> TranslateAnchor { get; set; }

        public override void Accept(IZMapStyleVisitor visitor, Feature feature)
        {
            base.Accept(visitor, feature);

            Opacity?.Invoke(feature, 1);
            Pattern?.Invoke(feature);

            if (Color != null)
            {
                Color.Invoke(feature, "#000000");
                OutlineColor?.Invoke(feature, Color.Value);
            }
            else
            {
                OutlineColor?.Invoke(feature, "#000000");
            }

            Translate?.Invoke(feature);
            TranslateAnchor?.Invoke(feature);
        }
    }
}