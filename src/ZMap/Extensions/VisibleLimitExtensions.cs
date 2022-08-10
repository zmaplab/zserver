namespace ZMap.Extensions
{
    public static class VisibleLimitExtensions
    {
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

            return min <= zoom && zoom <= max;
        }
    }
}