using System.Collections.Concurrent;
using NetTopologySuite.Geometries;

namespace ZMap.TileGrid
{
    public static class DefaultGridSets
    {
        private static readonly ConcurrentDictionary<string, GridSet> Instances;
        public static readonly Envelope World4326 = new(-180.0, 180.0, -90.0, 90.0);
        public static readonly Envelope World3857 = new(-20037508.34, 20037508.34, -20037508.34, 20037508.34);
        public static readonly Envelope World4490 = new(73.62, 134.77, 16.7, 53.56);

        /// <summary>
        /// Default pixel size in meters, producing a default of 90.7 DPI
        /// </summary>
        public const double DefaultPixelSizeMeter = 0.00028;

        public const int DefaultLevels = 22;

        static DefaultGridSets()
        {
            Instances = new ConcurrentDictionary<string, GridSet>();

            var epsg4326 = GridSetFactory.CreateGridSet(
                "EPSG:4326",
                4326, World4326,
                false,
                DefaultLevels,
                null,
                DefaultPixelSizeMeter,
                256,
                256,
                true);
            Instances.TryAdd(epsg4326.Name, epsg4326);

            var epsg4326X2 = GridSetFactory.CreateGridSet(
                "EPSG:4326x2",
                4326,
                World4326,
                false,
                DefaultLevels,
                null,
                DefaultPixelSizeMeter,
                512,
                512,
                true);
            Instances.TryAdd(epsg4326X2.Name, epsg4326X2);

            var epsg3857 = GridSetFactory.CreateGridSet(
                "EPSG:3857",
                3857,
                World3857,
                false,
                GetCommonPractice900913Resolutions(),
                null,
                1.0D,
                DefaultPixelSizeMeter,
                null,
                256,
                256,
                true);
            Instances.TryAdd(epsg3857.Name, epsg3857);

            var epsg3857X2 = GridSetFactory.CreateGridSet(
                "EPSG:3857x2",
                3857,
                World3857,
                false,
                GetCommonPractice900913Resolutions(),
                null,
                1.0D,
                DefaultPixelSizeMeter,
                null,
                512,
                512,
                true);
            Instances.TryAdd(epsg3857X2.Name, epsg3857X2);

            var globalCrs84Pixel = GridSetFactory.CreateGridSet(
                "GlobalCRS84Pixel",
                4326,
                World4326,
                true,
                GetScalesCRS84PixelResolutions(),
                null,
                null,
                DefaultPixelSizeMeter,
                null,
                256,
                256,
                true);
            Instances.TryAdd(globalCrs84Pixel.Name, globalCrs84Pixel);

            var globalCrs84Scale = GridSetFactory.CreateGridSet(
                "GlobalCRS84Scale",
                4326,
                World4326,
                true,
                null,
                GetScalesCRS84ScaleDenominators(),
                null,
                DefaultPixelSizeMeter,
                null,
                256,
                256,
                true);
            Instances.TryAdd(globalCrs84Scale.Name, globalCrs84Scale);

            var googleCrs84Quad = GridSetFactory.CreateGridSet(
                "GoogleCRS84Quad",
                4326,
                World4326,
                true,
                null,
                GetScalesCRS84QuadScaleDenominators(),
                null,
                DefaultPixelSizeMeter,
                null,
                256,
                256,
                true);
            Instances.TryAdd(googleCrs84Quad.Name, googleCrs84Quad);

            var epsg4490 = GridSetFactory.CreateGridSet(
                "EPSG:4490",
                4490, World4490,
                false,
                DefaultLevels,
                null,
                DefaultPixelSizeMeter,
                256,
                256,
                true);
            Instances.TryAdd(epsg4490.Name, epsg4490);
        }

        public static GridSet TryGet(string name)
        {
            return Instances.TryGetValue(name, out var v) ? v : null;
        }

        // ReSharper disable once InconsistentNaming
        private static double[] GetScalesCRS84QuadScaleDenominators()
        {
            return new[]
            {
                559082264.0287178,
                279541132.0143589,
                139770566.0071794,
                69885283.00358972,
                34942641.50179486,
                17471320.75089743,
                8735660.375448715,
                4367830.187724357,
                2183915.093862179,
                1091957.546931089,
                545978.7734655447,
                272989.3867327723,
                136494.6933663862,
                68247.34668319309,
                34123.67334159654,
                17061.83667079827,
                8530.918335399136,
                4265.459167699568,
                2132.729583849784
            };
        }

        // ReSharper disable once InconsistentNaming
        private static double[] GetScalesCRS84ScaleDenominators()
        {
            // double[] scalesCRS84Pixel = { 1.25764139776733, 0.628820698883665, 0.251528279553466,
            // 0.125764139776733, 6.28820698883665E-2, 2.51528279553466E-2, 1.25764139776733E-2,
            // 6.28820698883665E-3, 2.51528279553466E-3, 1.25764139776733E-3, 6.28820698883665E-4,
            // 2.51528279553466E-4, 1.25764139776733E-4, 6.28820698883665E-5, 2.51528279553466E-5,
            // 1.25764139776733E-5, 6.28820698883665E-6, 2.51528279553466E-6, 1.25764139776733E-6,
            // 6.28820698883665E-7, 2.51528279553466E-7 };
            //
            // return scalesCRS84Pixel;
            return new[]
            {
                500E6, 250E6, 100E6, 50E6, 25E6, 10E6, 5E6, 2.5E6, 1E6, 500E3, 250E3, 100E3, 50E3, 25E3,
                10E3, 5E3, 2.5E3, 1000, 500, 250, 100
            };
        }

        // ReSharper disable once InconsistentNaming
        private static double[] GetScalesCRS84PixelResolutions()
        {
            var pixels = new double[18];
            pixels[0] = 2;
            pixels[1] = 1;
            pixels[2] = 0.5; // 30
            pixels[3] = pixels[2] * (2.0 / 3.0); // 20
            pixels[4] = pixels[2] / 3.0; // 10
            pixels[5] = pixels[4] / 2.0; // 5
            pixels[6] = pixels[4] / 5.0; // 2
            pixels[7] = pixels[4] / 10.0; // 1
            pixels[8] = 5.0 / 6.0 * 1E-2; // 30'' = 8.33E-3
            pixels[9] = pixels[8] / 2.0; // 15''
            pixels[10] = pixels[9] / 3.0; // 5''
            pixels[11] = pixels[9] / 5.0; // 3''
            pixels[12] = pixels[11] / 3.0; // 1''
            pixels[13] = pixels[12] / 2.0; // 0.5''
            pixels[14] = pixels[13] * (3.0 / 5.0); // 0.3''
            pixels[15] = pixels[14] / 3.0; // 0.1''
            pixels[16] = pixels[15] * (3.0 / 10.0); // 0.03''
            pixels[17] = pixels[16] / 3.0; // 0.01''

            return pixels;
        }

        private static double[] GetCommonPractice900913Resolutions()
        {
            return new[]
            {
                156543.03390625,
                78271.516953125,
                39135.7584765625,
                19567.87923828125,
                9783.939619140625,
                4891.9698095703125,
                2445.9849047851562,
                1222.9924523925781,
                611.4962261962891,
                305.74811309814453,
                152.87405654907226,
                76.43702827453613,
                38.218514137268066,
                19.109257068634033,
                9.554628534317017,
                4.777314267158508,
                2.388657133579254,
                1.194328566789627,
                0.5971642833948135,
                0.29858214169740677,
                0.14929107084870338,
                0.07464553542435169,
                0.037322767712175846,
                0.018661383856087923,
                0.009330691928043961,
                0.004665345964021981,
                0.0023326729820109904,
                0.0011663364910054952,
                5.831682455027476E-4,
                2.915841227513738E-4,
                1.457920613756869E-4
            };
        }
    }
}