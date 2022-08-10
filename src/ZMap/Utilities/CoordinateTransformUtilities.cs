using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems.Transformations;

namespace ZMap.Utilities
{
    public static class CoordinateTransformUtilities
    {
        private static readonly GeometryFactory GeometryFactory =
            NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory();

        private static readonly ConcurrentDictionary<string, ICoordinateTransformation> CoordinateTransformations =
            new();

        private static readonly CoordinateTransformationFactory CoordinateTransformationFactory =
            new();

        // [SuppressMessage("ReSharper", "InconsistentNaming")]
        // public static (ICoordinateTransformation t1, ICoordinateTransformation t2) GetTransformationPair(int sourceSRID,
        //     int targetSRID)
        // {
        //     var t1 = GetTransformation(sourceSRID, targetSRID);
        //     var t2 = GetTransformation(targetSRID, sourceSRID);
        //     return (t1, t2);
        // }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        internal static ICoordinateTransformation GetTransformation(int sourceSRID, int targetSRID)
        {
            var key = $"{sourceSRID}:{targetSRID}";
            return CoordinateTransformations.GetOrAdd(key, _ =>
                CoordinateTransformationFactory.CreateFromCoordinateSystems(CoordinateSystemUtilities.Get(sourceSRID),
                    CoordinateSystemUtilities.Get(targetSRID)));
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static Envelope Transform(this Envelope extent, int sourceSRID, int targetSRID)
        {
            if (extent == null)
            {
                return null;
            }

            if (extent.IsNull)
            {
                return extent;
            }

            if (sourceSRID == targetSRID)
            {
                return extent.Copy();
            }

            var transformation = GetTransformation(sourceSRID, targetSRID);

            var corners = new[]
            {
                Transform(new Coordinate(extent.MinX, extent.MinY), transformation),
                Transform(new Coordinate(extent.MinX, extent.MaxY), transformation),
                Transform(new Coordinate(extent.MaxX, extent.MinY), transformation),
                Transform(new Coordinate(extent.MaxX, extent.MaxY), transformation),
            };

            var result = new Envelope(corners[0]);
            for (var i = 1; i < 4; i++)
            {
                result.ExpandToInclude(corners[i]);
            }

            return result;
        }

        /// <summary>
        ///  todo: 是否需要所有类型的图形都转换
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Geometry Transform(Geometry geometry, ICoordinateTransformation transform)
        {
            if (geometry == null || transform == null)
            {
                return null;
            }

            return geometry switch
            {
                Point point => Transform(point, transform),
                LineString lineString => Transform(lineString, transform),
                Polygon polygon => Transform(polygon, transform),
                MultiPoint point => Transform(point, transform),
                MultiLineString lineString => Transform(lineString, transform),
                MultiPolygon polygon => Transform(polygon, transform),
                GeometryCollection collection => Transform(collection, transform),
                _ => throw new ArgumentException("Could not transform geometry type '" + geometry.GetType() + "'")
            };
        }

        private static Geometry Transform(GeometryCollection geometryCollection, ICoordinateTransformation transform)
        {
            var geomList = new Geometry[geometryCollection.NumGeometries];
            for (var i = 0; i < geometryCollection.NumGeometries; i++)
            {
                geomList[i] = Transform(geometryCollection[i], transform);
            }

            return GeometryFactory.CreateGeometryCollection(geomList);
        }

        private static Geometry Transform(MultiPolygon multiPolygon, ICoordinateTransformation transform)
        {
            var polyList = new Polygon[multiPolygon.NumGeometries];
            for (var i = 0; i < multiPolygon.NumGeometries; i++)
            {
                var poly = (Polygon)multiPolygon[i];
                polyList[i] = Transform(poly, transform);
            }

            return GeometryFactory.CreateMultiPolygon(polyList);
        }

        private static Geometry Transform(MultiLineString multiLineString, ICoordinateTransformation transform)
        {
            var lineList = new LineString[multiLineString.NumGeometries];
            for (var i = 0; i < multiLineString.NumGeometries; i++)
            {
                var line = (LineString)multiLineString[i];
                lineList[i] = Transform(line, transform);
            }

            return GeometryFactory.CreateMultiLineString(lineList);
        }

        private static Polygon Transform(Polygon polygon, ICoordinateTransformation transform)
        {
            var shell = Transform((LinearRing)polygon.ExteriorRing, transform);
            LinearRing[] holes = null;
            var holesCount = polygon.NumInteriorRings;
            if (holesCount > 0)
            {
                holes = new LinearRing[holesCount];
                for (var i = 0; i < holesCount; i++)
                {
                    holes[i] = Transform((LinearRing)polygon.GetInteriorRingN(i), transform);
                }
            }

            return GeometryFactory.CreatePolygon(shell, holes);
        }

        private static LinearRing Transform(LinearRing linearRing, ICoordinateTransformation transform)
        {
            return GeometryFactory.CreateLinearRing(Transform(linearRing.Coordinates, transform));
        }

        private static Coordinate[] Transform(Coordinate[] coordinates, ICoordinateTransformation transform)
        {
            var res = new Coordinate[coordinates.Length];
            for (var i = 0; i < coordinates.Length; i++)
            {
                res[i] = Transform(coordinates[i], transform);
            }

            return res;
        }

        internal static Coordinate Transform(Coordinate coordinate, ICoordinateTransformation transform)
        {
            var xy = transform.MathTransform.Transform(coordinate.X, coordinate.Y);
            return new Coordinate(xy.x, xy.y);
        }

        private static LineString Transform(LineString lineString, ICoordinateTransformation transform)
        {
            return GeometryFactory.CreateLineString(Transform(lineString.Coordinates, transform));
        }

        private static Point Transform(Point point, ICoordinateTransformation transform)
        {
            return GeometryFactory.CreatePoint(Transform(point.Coordinate, transform));
        }

        private static Geometry Transform(MultiPoint multiPoint, ICoordinateTransformation transform)
        {
            return GeometryFactory.CreateMultiPointFromCoords(Transform(multiPoint.Coordinates, transform));
        }
    }
}