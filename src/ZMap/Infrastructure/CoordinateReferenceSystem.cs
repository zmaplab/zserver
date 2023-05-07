using System;
using System.Collections.Concurrent;
using System.Xml;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace ZMap.Infrastructure
{
    public static class CoordinateReferenceSystem
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ConcurrentDictionary<int, CoordinateSystem> SRIDCache;

        private static readonly GeometryFactory GeometryFactory =
            NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory();

        private static readonly ConcurrentDictionary<string, ICoordinateTransformation> CoordinateTransformations =
            new();

        private static readonly CoordinateTransformationFactory CoordinateTransformationFactory =
            new();

        static CoordinateReferenceSystem()
        {
            SRIDCache = new ConcurrentDictionary<int, CoordinateSystem>();
            typeof(CoordinateReferenceSystem).Assembly.GetManifestResourceNames();
            using var stream =
                typeof(CoordinateReferenceSystem).Assembly.GetManifestResourceStream("ZMap.Infrastructure.proj.xml");
            if (stream == null)
            {
                return;
            }

            var coordinateSystemFactory = new CoordinateSystemFactory();
            var xml = new XmlDocument();
            xml.Load(stream);
            var nodes = xml.DocumentElement?.SelectNodes("/SpatialReference/*");
            if (nodes == null)
            {
                return;
            }

            foreach (XmlNode referenceNode in nodes)
            {
                var sridNode = referenceNode?.SelectSingleNode("SRID");
                if (sridNode == null)
                {
                    continue;
                }

                var wkt = referenceNode.LastChild?.InnerText;

                if (string.IsNullOrWhiteSpace(wkt) || !int.TryParse(sridNode.InnerText, out var srid))
                {
                    continue;
                }

                var coordinateSystem = coordinateSystemFactory.CreateFromWkt(wkt);
                SRIDCache.TryAdd(srid, coordinateSystem);
            }
        }

        public static CoordinateSystem Get(int srid)
        {
            return srid switch
            {
                4326 => GeographicCoordinateSystem.WGS84,
                3857 => ProjectedCoordinateSystem.WebMercator,
                _ => SRIDCache.TryGetValue(srid, out var coordinateSystem) ? coordinateSystem : null
            };
        }

        public static ICoordinateTransformation CreateTransformation(int sourceSrid, int targetSrid)
        {
            var key = $"{sourceSrid}:{targetSrid}";
            return CoordinateTransformations.GetOrAdd(key, _ =>
            {
                var source = Get(sourceSrid);
                var target = Get(targetSrid);

                if (source == null || target == null)
                {
                    return null;
                }

                return CoordinateTransformationFactory.CreateFromCoordinateSystems(source, target);
            });
        }

        public static Envelope Transform(this Envelope extent, int sourceSrid, int targetSrid)
        {
            if (extent == null)
            {
                return null;
            }

            if (extent.IsNull)
            {
                return extent;
            }

            if (sourceSrid == targetSrid)
            {
                return extent.Copy();
            }

            var transformation = CreateTransformation(sourceSrid, targetSrid);
            if (transformation == null)
            {
                throw new ArgumentException("创建投影转换失败");
            }

            var corners = new[]
            {
                Transform(new Coordinate(extent.MinX, extent.MinY), transformation),
                Transform(new Coordinate(extent.MinX, extent.MaxY), transformation),
                Transform(new Coordinate(extent.MaxX, extent.MinY), transformation),
                Transform(new Coordinate(extent.MaxX, extent.MaxY), transformation)
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
                _ => throw new ArgumentException("Can't transform geometry type '" + geometry.GetType() + "'")
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

        private static Coordinate Transform(Coordinate coordinate, ICoordinateTransformation transform)
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