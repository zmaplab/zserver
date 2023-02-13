namespace ZMap.Style
{
    public class LineStyle : VectorStyle, IStrokeStyle
    {
        /// <summary>
        /// 线的不透明度（可选，取值范围为 0 ~ 1，默认值为 1）
        /// </summary>
        public Expression<float> Opacity { get; set; }

        /// <summary>
        /// 线用的图案（可选，这里填写在 sprite 雪碧图中图标名称。为了图案能无缝填充，图标的高宽需要是 2 的倍数）
        /// </summary>
        public Expression<string> Pattern { get; set; }

        /// <summary>
        /// 线的宽度（可选，值 >= 0，默认值为 1，单位：像素）
        /// </summary>
        public Expression<int> Width { get; set; }

        /// <summary>
        /// 线的颜色（可选，默认值为 #000000。如果设置了 line-pattern，则 line-color 将无效）
        /// </summary>
        public Expression<string> Color { get; set; }

        /// <summary>
        /// 虚线的破折号部分和间隔的长度（可选，默认值为 [0, 0]。如果设置了 line-pattern，则 line-dasharray 将无效）
        /// </summary>
        public Expression<float[]> DashArray { get; set; }
        public Expression<float> DashOffset { get; set; }
        public Expression<string> LineJoin { get; set; }
        public Expression<string> LineCap { get; set; }
        /// <summary>
        /// 线的平移（可选，通过平移 [x, y] 达到一定的偏移量。默认值为 [0, 0]，单位：像素。）
        /// </summary>
        public Expression<double[]> Translate { get; set; }

        /// <summary>
        /// 线的平移锚点，即相对的参考物（可选，可选值为 map、viewport，默认为 map）
        /// </summary>
        public Expression<TranslateAnchor> TranslateAnchor { get; set; }
        public Expression<int> GapWidth { get; set; }
        public Expression<int> Offset { get; set; }
        
        /// <summary>
        /// 线的模糊度（可选，值 >= 0，默认值为 0，单位：像素）
        /// </summary>
        public Expression<int> Blur { get; set; }

        /// <summary>
        /// Disabled by dasharray. Disabled by pattern
        /// </summary>
        public Expression<int> Gradient { get; set; }

        public override void Accept(IZMapStyleVisitor visitor, Feature feature)
        {
            base.Accept(visitor, feature);

            Opacity?.Invoke(feature, 1);
            Pattern?.Invoke(feature);
            Width?.Invoke(feature, 1);
            Color?.Invoke(feature, "#000000");
            DashArray?.Invoke(feature);
            LineJoin?.Invoke(feature);
            LineCap?.Invoke(feature);
            Translate?.Invoke(feature);
            TranslateAnchor?.Invoke(feature);
            GapWidth?.Invoke(feature);
            Offset?.Invoke(feature);
            Blur?.Invoke(feature);
            Gradient?.Invoke(feature);
        }
    }
}