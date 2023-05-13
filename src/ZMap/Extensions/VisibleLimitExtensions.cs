namespace ZMap.Extensions
{
    public static class VisibleLimitExtensions
    {
        /// <summary>
        /// 判断给定的缩放是否需要显示
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="zoom"></param>
        /// <returns></returns>
        public static bool IsVisible(this IVisibleLimit limit, Zoom zoom)
        {
            // 单位一定要相同
            return limit.ZoomUnit == zoom.Units && IsVisible(zoom.Value, limit.MinZoom, limit.MaxZoom);
        }

        private static bool IsVisible(this double? zoom, double min, double max)
        {
            if (!zoom.HasValue || min <= 0 && max <= 0)
            {
                return true;
            }

            return min <= zoom && zoom < max;
        }
    }
}