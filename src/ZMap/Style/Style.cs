namespace ZMap.Style
{
    public abstract class Style : IVisibleLimit
    {
        /// <summary>
        /// 过滤表达式， 解析成一个 Func<Feature, bool> 方法
        /// 只有满足条件才会渲染
        /// </summary>
        public Expression<bool?> Filter { get; set; }

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

        public virtual void Accept(IZMapStyleVisitor visitor, Feature feature)
        {
            Filter?.Invoke(feature);
        }
    }
}