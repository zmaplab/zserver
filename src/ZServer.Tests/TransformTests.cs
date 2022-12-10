using System;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using Xunit;
using ZMap.Utilities;

namespace ZServer.Tests
{
    public class TransformTests
    {
        [Fact]
        public void Transform4326To3857()
        {
            // 1389930.9223376447,5140129.298989603,1390236.6704507857,5140539.834725233

            var envelope = new Envelope(12.4859619140625, 12.48870849609375, 41.86065673828125, 41.8634033203125);

            var result = envelope.Transform(4326, 3857);
            Assert.Equal(1389930.9223376447, result.MinX);
            Assert.Equal(1390236.6704507857, result.MaxX);
            Assert.Equal(5140129.298989603, result.MinY);
            Assert.Equal(5140539.834725233, result.MaxY);
        }

        [Fact]
        public void Reproj()
        {
            // 4326 -> 4548
            // 117.36492919921875, 31.9448184967041 -> 534504.0463862874 3535791.699179905


            var factory = new GeometryFactory(new PrecisionModel(), 4326);
            var point = factory.CreatePoint(new Coordinate(117.36492919921875, 31.9448184967041));

            var newPoint = (Point)CoordinateTransformUtilities.Transform(point,
                CoordinateTransformUtilities.GetTransformation(4326, 4548));

            Assert.Equal(534504.046374663, newPoint.X);
            Assert.Equal(3535791.7026292756, newPoint.Y);
        }

        [Fact]
        public void ExtentTransform()
        {
            var extent =
                new Envelope(52.4978, 52.51, 13.26, 13.2814).Transform(4326, 3857);

            var extent2 = extent.Transform(3857, 4326);
            Assert.Equal(52.4978, extent2.MinX);
            Assert.Equal(52.509999999999998, extent2.MaxX);
            Assert.Equal(13.25999999988822, extent2.MinY);
            Assert.Equal(13.281399999888224, extent2.MaxY);
        }

        public static Envelope ConvertToMercatore(Envelope extent)
        {
            var precisionModel =
                new PrecisionModel(PrecisionModels.Floating);

            var wgs84 = GeographicCoordinateSystem.WGS84 as CoordinateSystem;
            var mercatore = ProjectedCoordinateSystem.WebMercator as CoordinateSystem;
            var cFac = new CoordinateSystemFactory();

            var SRID_wgs84 = Convert.ToInt32(wgs84.AuthorityCode); //WGS84 SRID
            var SRID_mercatore = Convert.ToInt32(mercatore.AuthorityCode); //Mercatore SRID

            var ctFact =
                new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();
            var transformation = ctFact.CreateFromCoordinateSystems(wgs84, mercatore);

            var factory_wgs84 =
                new GeometryFactory(precisionModel, SRID_wgs84);

            var bottomLeft = factory_wgs84.CreatePoint(new Coordinate(extent.MinX, extent.MinY));
            var topRight = factory_wgs84.CreatePoint(new Coordinate(extent.MaxX, extent.MinY));

            var coords_bl = transformation.MathTransform.Transform(new double[] { bottomLeft.X, bottomLeft.Y });
            var coords_tr = transformation.MathTransform.Transform(new double[] { topRight.X, topRight.Y });

            var factory_mercatore =
                new GeometryFactory(precisionModel, SRID_mercatore);

            var p1_bl = factory_mercatore.CreatePoint(new Coordinate(coords_bl[0], coords_bl[1]));
            var p2_tr = factory_mercatore.CreatePoint(new Coordinate(coords_tr[0], coords_tr[1]));

            return new Envelope(p1_bl.X, p1_bl.Y, p2_tr.X, p2_tr.Y);
        }
    }
}