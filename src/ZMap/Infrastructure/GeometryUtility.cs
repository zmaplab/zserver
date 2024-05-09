namespace ZMap.Infrastructure;

/// <summary>
/// 图形计算相关的方法
/// </summary>
public static class GeometryUtility
{
    public const double Tolerance = 0.0001;
        
    /// <summary>
    /// Gets the bottom-left coordinate of the <see cref="Envelope"/>
    /// </summary>
    /// <param name="self">The envelope</param>
    /// <returns>The bottom-left coordinate</returns>
    public static Coordinate BottomLeft(this Envelope self)
    {
        return self.Min();
    }

    /// <summary>
    /// Subtracts two coordinates from one another
    /// </summary>
    /// <param name="self">The first coordinate</param>
    /// <param name="sub">The second coordinate</param>
    public static Coordinate Subtract(this Coordinate self, Coordinate sub)
    {
        if (self == null && sub == null)
            return null;
        if (sub == null)
            return self;
        if (self == null)
            return new Coordinate(-sub.X, -sub.Y);

        // for now we only care for 2D
        return new Coordinate(self.X - sub.X, self.Y - sub.Y);
    }

    /// <summary>
    /// Adds to coordinate's
    /// </summary>
    /// <param name="self">the first coordinate</param>
    /// <param name="sub">The second coordinate</param>
    public static Coordinate Add(this Coordinate self, Coordinate sub)
    {
        if (self == null && sub == null)
            return null;
        if (sub == null)
            return self;
        if (self == null)
            return sub;

        // for now we only care for 2D
        return new Coordinate(self.X + sub.X, self.Y + sub.Y);
    }

    /// <summary>
    /// Gets the top-right coordinate of the <see cref="Envelope"/>
    /// </summary>
    /// <param name="self">The envelope</param>
    /// <returns>The top-right coordinate</returns>
    public static Coordinate TopRight(this Envelope self)
    {
        return self.Max();
    }

    /// <summary>
    /// Gets the maximum coordinate of the <see cref="Envelope"/>
    /// </summary>
    /// <param name="self">The envelope</param>
    /// <returns>The maximum coordinate</returns>
    public static Coordinate Max(this Envelope self)
    {
        return new Coordinate(self.MaxX, self.MaxY);
    }

    /// <summary>
    /// Gets the minimum coordinate of the <see cref="Envelope"/>
    /// </summary>
    /// <param name="self">The envelope</param>
    /// <returns>The minimum coordinate</returns>
    public static Coordinate Min(this Envelope self)
    {
        return new Coordinate(self.MinX, self.MinY);
    }

    /// <summary>
    /// Abbreviation to counter clockwise function
    /// </summary>
    /// <param name="self">The ring</param>
    /// <returns><c>true</c> if the ring is oriented counter clockwise</returns>
    public static bool IsCcw(this LinearRing self)
    {
        return NetTopologySuite.Algorithm.Orientation.IsCCW(self.Coordinates);
    }

    public static Ordinate LongestAxis(this Envelope self)
    {
        return Math.Abs(self.MaxExtent - self.Width) < Tolerance ? Ordinate.X : Ordinate.Y;
    }

    // public static void Simplify(this Feature feature)
    // {
    //     var srid = feature.Geometry.SRID;
    //     var coordinateSystem = CoordinateSystemUtilities.Get(srid);
    //     if (coordinateSystem == null)
    //     {
    //         return;
    //     }
    //
    //     // 84 最小可用精度是 0.00001 度， 0.0001 以上图形异常
    //     // 大地坐标系最大可用精度是 0.0001 米， 0.00001 开始会消除不掉异常点
    //     var distanceTolerance = coordinateSystem is GeographicCoordinateSystem ? 0.00001 : 0.0001;
    //     feature.Geometry = VWSimplifier.Simplify(feature.Geometry, distanceTolerance);
    // }
}