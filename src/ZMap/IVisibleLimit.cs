namespace ZMap
{
    public interface IVisibleLimit
    {
        /// <summary>
        /// 最小显示范围
        /// </summary>
        double MinZoom { get; set; }

        /// <summary>
        /// 最大显示范围
        /// </summary>
        double MaxZoom { get; set; }

        /// <summary>
        /// 显示范围单位: 缩放级别、比例尺
        /// </summary>
        ZoomUnits ZoomUnit { get; set; }
    }
}