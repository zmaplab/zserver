namespace ZMap.Infrastructure;

/// <summary>
/// 地理空间计算相关的方法
/// </summary>
public static class GeographicUtility
{
    /// <summary>
    /// 经纬度坐标系的比例尺计算
    /// </summary>
    /// <param name="envelope"></param>
    /// <param name="width"></param>
    /// <param name="dpi"></param>
    /// <returns></returns>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static double CalculateOGCScale(Envelope envelope, int width, double dpi)
    {
        var widthMeters = envelope.Width * Defaults.MetersPerDegreeAtEquator;
        return widthMeters / (width / dpi * 0.0254D);
    }

    /// <summary>
    /// 经纬度坐标系的比例尺计算
    /// </summary>
    /// <param name="envelope"></param>
    /// <param name="srid"></param>
    /// <param name="width"></param>
    /// <param name="dpi"></param>
    /// <returns></returns>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static double CalculateOGCScale(Envelope envelope, int srid, int width, double dpi)
    {
        var envelope4326 = envelope.Transform(srid, 4326);
        return CalculateOGCScale(envelope4326, width, dpi);
    }

    /// <summary>
    /// Calculate the Representative Fraction Scale for a Lat/Long map.
    /// </summary>
    /// <param name="lon1">LowerLeft Longitude</param>
    /// <param name="lon2">LowerRight Longitude</param>
    /// <param name="lat">LowerLeft Latitude</param>
    /// <param name="widthPage">The width of the display area</param>
    /// <param name="dpi">DPI used to render the map</param>
    /// <returns></returns>
    public static double CalculateScaleLatLong(double lon1, double lon2, double lat, double widthPage, int dpi)
    {
        var distance = GreatCircleDistanceReflex(lon1, lon2, lat);
        var scale = CalculateScaleNonLatLong(distance, widthPage, 1, dpi);
        return scale;
    }

    public static double GreatCircleDistanceReflex(double lon1, double lon2, double lat)
    {
        var lonDistance = Math.Abs(lon2 - lon1);
        lat = Math.Abs(lat);
        if (lat >= 90.0)
        {
            lat = 89.999;
        }

        var distance = Math.Cos(lat * Defaults.DegToRad) * Defaults.MetersPerDegreeAtEquator * lonDistance;
        return distance;
    }

    public static double CalculateScaleNonLatLong(double mapWidthMeters, double mapSizeWidth, double mapUnitFactor,
        int dpi)
    {
        var pixelPerInch = dpi;
        double ratio;

        if (mapSizeWidth <= 0)
        {
            return 0.0;
        }

        var mapWidth = mapWidthMeters * mapUnitFactor;
        try
        {
            // todo: 去掉 try?
            var pageWidth = mapSizeWidth / pixelPerInch * 0.0254;
            ratio = Math.Abs(mapWidth / pageWidth);
        }
        catch
        {
            ratio = 0.0;
        }

        return ratio;
    }

    public static (double Lat, double Lon) CalculateLatLongFromGrid(Envelope bbox, double pixelWidth,
        double pixelHeight, int x, int y)
    {
        var lon = (float)bbox.MinX + pixelWidth * x;
        var lat = (float)bbox.MinY + pixelHeight * y;
        return (lat, lon);
    }
}