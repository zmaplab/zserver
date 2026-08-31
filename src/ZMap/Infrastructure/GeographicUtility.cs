namespace ZMap.Infrastructure;

/// <summary>
/// 地理空间计算相关的方法
/// </summary>
public static class GeographicUtility
{
    private const double OgcPixelSizeMeters = 0.00028D;
    private const double OgcEquatorialRadiusMeters = 6378137D;

    /// <summary>
    /// 使用 OGC 固定像元尺寸计算 CRS 感知的比例尺分母。
    /// </summary>
    /// <param name="envelope">请求范围。</param>
    /// <param name="srid">请求范围使用的 EPSG SRID。</param>
    /// <param name="imageWidth">输出图像宽度（像素）。</param>
    /// <returns>OGC/SLD 比例尺分母。</returns>
    /// <exception cref="ArgumentNullException">范围为 null。</exception>
    /// <exception cref="ArgumentOutOfRangeException">图像宽度不是正数。</exception>
    /// <exception cref="ArgumentException">SRID 未注册、CRS 不是二维水平 CRS 或单位无效。</exception>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static double CalculateOGCScaleForSrid(Envelope envelope, int srid, int imageWidth)
    {
        ValidateEnvelope(envelope);
        ValidateImageWidth(imageWidth);

        var coordinateSystem = CoordinateReferenceSystem.Get(srid);
        if (coordinateSystem == null)
        {
            throw new ArgumentException($"未注册的坐标参考系统 EPSG:{srid}", nameof(srid));
        }

        if (coordinateSystem.Dimension != 2)
        {
            throw new ArgumentException("比例尺计算只支持二维水平坐标参考系统", nameof(srid));
        }

        var imageWidthMeters = RequirePositiveFinite(
            imageWidth * OgcPixelSizeMeters,
            "输出图像的 OGC 像元宽度必须是有限正数",
            nameof(imageWidth));

        var scale = coordinateSystem switch
        {
            ProjectedCoordinateSystem projected => CalculateProjectedOgcScale(
                envelope, projected, srid, imageWidthMeters),
            GeographicCoordinateSystem geographic => CalculateGeographicOgcScale(
                envelope, geographic, srid, imageWidthMeters),
            _ => throw new ArgumentException("比例尺计算只支持二维水平坐标参考系统", nameof(srid))
        };

        return RequirePositiveFinite(scale, "OGC 比例尺必须是有限正数", nameof(envelope));
    }

    /// <summary>
    /// 按打印输出语义计算比例尺分母。
    /// </summary>
    /// <param name="envelope">经度/纬度范围。</param>
    /// <param name="imageWidth">输出图像宽度（像素）。</param>
    /// <param name="dpi">打印输出 DPI。</param>
    /// <returns>打印语义的比例尺分母。</returns>
    /// <exception cref="ArgumentNullException">范围为 null。</exception>
    /// <exception cref="ArgumentOutOfRangeException">图像宽度或 DPI 不是正数。</exception>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static double CalculatePrintScale(Envelope envelope, int imageWidth, double dpi)
    {
        ValidateEnvelope(envelope);
        ValidateImageWidth(imageWidth);
        if (!double.IsFinite(dpi) || dpi <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dpi), dpi, "DPI 必须是正数");
        }

        var widthMeters = Math.Abs(envelope.Width) * Defaults.MetersPerDegreeAtEquator;
        return widthMeters / (imageWidth / dpi * 0.0254D);
    }

    /// <summary>
    /// 兼容旧调用方的打印比例尺计算入口。
    /// </summary>
    [Obsolete("请使用 CalculatePrintScale；WMS/SLD 比例尺请使用 CalculateOGCScaleForSrid。")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static double CalculateOGCScale(Envelope envelope, int width, double dpi)
    {
        return CalculatePrintScale(envelope, width, dpi);
    }

    /// <summary>
    /// 兼容旧调用方的打印比例尺计算入口。
    /// </summary>
    [Obsolete("请使用 CalculatePrintScale；WMS/SLD 比例尺请使用 CalculateOGCScaleForSrid。")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static double CalculateOGCScale(Envelope envelope, int srid, int width, double dpi)
    {
        ValidateEnvelope(envelope);
        var envelope4326 = envelope.Transform(srid, 4326);
        return CalculatePrintScale(envelope4326, width, dpi);
    }

    private static double CalculateProjectedOgcScale(Envelope envelope,
        ProjectedCoordinateSystem coordinateSystem, int srid, double imageWidthMeters)
    {
        var widthMeters = RequirePositiveFinite(
            envelope.Width * GetMetersPerUnit(coordinateSystem, srid),
            "范围转换后的宽度必须是有限正数",
            nameof(envelope));
        var scale = widthMeters / imageWidthMeters;
        return RequirePositiveFinite(scale, "OGC 比例尺必须是有限正数", nameof(envelope));
    }

    private static double CalculateGeographicOgcScale(Envelope envelope,
        GeographicCoordinateSystem coordinateSystem, int srid, double imageWidthMeters)
    {
        var widthRadians = RequirePositiveFinite(
            envelope.Width * GetRadiansPerUnit(coordinateSystem, srid),
            "范围转换后的角度宽度必须是有限正数",
            nameof(envelope));
        var widthMeters = RequirePositiveFinite(
            widthRadians * OgcEquatorialRadiusMeters,
            "范围转换后的米制宽度必须是有限正数",
            nameof(envelope));
        var scale = widthMeters / imageWidthMeters;
        return RequirePositiveFinite(scale, "OGC 比例尺必须是有限正数", nameof(envelope));
    }

    private static double GetMetersPerUnit(ProjectedCoordinateSystem coordinateSystem, int srid)
    {
        var metersPerUnit = coordinateSystem.LinearUnit?.MetersPerUnit;
        if (!metersPerUnit.HasValue || !IsPositiveFinite(metersPerUnit.Value))
        {
            throw new ArgumentException($"CRS EPSG:{srid} 缺少有效的线性单位", nameof(srid));
        }

        return metersPerUnit.Value;
    }

    private static double GetRadiansPerUnit(GeographicCoordinateSystem coordinateSystem, int srid)
    {
        var radiansPerUnit = coordinateSystem.AngularUnit?.RadiansPerUnit;
        if (!radiansPerUnit.HasValue || !IsPositiveFinite(radiansPerUnit.Value))
        {
            throw new ArgumentException($"CRS EPSG:{srid} 缺少有效的角度单位", nameof(srid));
        }

        return radiansPerUnit.Value;
    }

    private static void ValidateEnvelope(Envelope envelope)
    {
        if (envelope == null)
        {
            throw new ArgumentNullException(nameof(envelope));
        }

        if (envelope.IsNull ||
            !double.IsFinite(envelope.MinX) ||
            !double.IsFinite(envelope.MaxX) ||
            !double.IsFinite(envelope.MinY) ||
            !double.IsFinite(envelope.MaxY) ||
            !IsPositiveFinite(envelope.Width) ||
            !IsPositiveFinite(envelope.Height))
        {
            throw new ArgumentException("范围必须是有效的二维范围", nameof(envelope));
        }
    }

    private static void ValidateImageWidth(int imageWidth)
    {
        if (imageWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(imageWidth), imageWidth, "图像宽度必须是正数");
        }
    }

    private static bool IsPositiveFinite(double value)
    {
        return value > 0 && double.IsFinite(value);
    }

    private static double RequirePositiveFinite(double value, string message, string parameterName)
    {
        if (!IsPositiveFinite(value))
        {
            throw new ArgumentException(message, parameterName);
        }

        return value;
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
