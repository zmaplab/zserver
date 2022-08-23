using System;
using System.Diagnostics.CodeAnalysis;
using NetTopologySuite.Geometries;

namespace ZMap.Utilities
{
    public static class GeoUtilities
    {
        public const double Tolerance = 0.0001;

        /// <summary>
        /// Conversion factor degrees to radians
        /// </summary>
        public const double DegToRad = Math.PI / 180d; //0.01745329252; // Convert Degrees to Radians

        /// <summary>
        /// Meters per degree at equator
        /// </summary>
        public const double MetersPerDegreeAtEquator = MetersPerMile * MilesPerDegreeAtEquator;

        /// <summary>
        /// Meters per mile
        /// </summary>
        public const double MetersPerMile = 1609.347219;

        /// <summary>
        /// Miles per degree at equator
        /// </summary>
        public const double MilesPerDegreeAtEquator = 69.171;

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
            var widthMeters = envelope.Width * MetersPerDegreeAtEquator;
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

            var distance = Math.Cos(lat * DegToRad) * MetersPerDegreeAtEquator * lonDistance;
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
            double pixelHeight, int x,
            int y)
        {
            var lon = bbox.MinX + pixelWidth * x;
            var lat = bbox.MinY + pixelHeight * y;
            return (lat, lon);
        }
    }
}