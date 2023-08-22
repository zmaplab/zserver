using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Npgsql;
using ZServer;

// ReSharper disable InconsistentNaming
#pragma warning disable 649

namespace Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(x => x.AddConsole());
            serviceCollection.AddMemoryCache();
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            serviceCollection.AddSingleton<IConfiguration>();

            // 配置的存储
            serviceCollection.AddZServer(configuration, "conf/zserver.json");

            serviceCollection.BuildServiceProvider();

            // ZOOM=14
            // var bytes = await renderingService.GetImageAsync(new List<(string Workspace, string Layer, string Filter)>
            //     {
            //         ("ah2021", "ygpd", null)
            //     }, Get(117.345703125, 31.928339843750003, 117.36767578125, 31.950312500000003),
            //     1024, 1024, 180,
            //     "image/png", 4326);

            // await File.WriteAllBytesAsync("test.png", bytes);
            System.Console.WriteLine("Bye");
        }

        static async Task Cut()
        {
            // 切片范围

            // 切片原点
            var tileOrigin = new Point(-180.00, 180.00);

            // 切片大小
            var tileSize = 256;

            // 计算经纬度100, 39在8级时的行列号


            await using var conn =
                new NpgsqlConnection(
                    "User ID=postgres;Password=1qazZAQ!;Host=localhost;Port=5432;Database=hdy;Pooling=true;");

            // var extent = await conn.QuerySingleOrDefaultAsync<Extent>(
            //     $"select min(st_xmin(geom)) xmin, min(st_ymin(geom)) ymin, max(st_xmax(geom)) xmax, max(st_ymax(geom)) ymax from sldc_2019");
            //

            var featureExtents = (await conn.QueryAsync<Extent>(
                    $"select id, st_xmin(geom) xmin, st_ymin(geom) ymin, st_xmax(geom) xmax, st_ymax(geom) ymax from sldc_2019"))
                .ToArray();

            var data = new List<dynamic>();

            foreach (var featureExtent in featureExtents)
            {
                var leftTop = new Point(featureExtent.XMin, featureExtent.YMin);
                var rightTop = new Point(featureExtent.XMax, featureExtent.YMin);
                var leftBottom = new Point(featureExtent.XMin, featureExtent.YMax);
                var rightBottom = new Point(featureExtent.XMax, featureExtent.YMax);

                for (var i = 1; i <= 18; i++)
                {
                    var leftTopXyz = XYZ.Caculate(tileOrigin, i, leftTop, tileSize);
                    var rightTopXyz = XYZ.Caculate(tileOrigin, i, rightTop, tileSize);
                    var leftBottomXyz = XYZ.Caculate(tileOrigin, i, leftBottom, tileSize);
                    XYZ.Caculate(tileOrigin, i, rightBottom, tileSize);

                    for (var x = leftTopXyz.X; x <= rightTopXyz.X; ++x)
                    {
                        for (var y = leftBottomXyz.Y; y <= leftTopXyz.Y; ++y)
                        {
                            var xyz = new XYZ { X = x, Y = y, Z = i };
                            data.Add(new
                            {
                                XYZ = xyz.ToString(),
                                FeatureId = featureExtent.Id
                            });
                        }
                    }
                }
            }

            System.Console.WriteLine($"Trying insert {data.Count} pyramid");

            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }

            await using var writer =
                conn.BeginBinaryImport("COPY sldc_2019_pyramid (xyz, feature_id) FROM STDIN (FORMAT BINARY)");
            foreach (var wrapper in data)
            {
                await writer.StartRowAsync();
                await writer.WriteAsync(wrapper.XYZ);
                await writer.WriteAsync(wrapper.FeatureId);
            }

            await writer.CompleteAsync();

            System.Console.WriteLine($"Insert {data.Count} pyramid completed");
        }

        static Envelope Get(double x1, double y1, double x2, double y2)
        {
            return new Envelope(x1, x2, y1, y2);
        }

        public record XYZ
        {
            public int X;
            public int Y;
            public int Z;

            public static XYZ Caculate(Point tileOrigin, int zoom, Point point, int tileSize = 256)
            {
                var res = (tileOrigin.Y - tileOrigin.X) / tileSize / Math.Pow(2, zoom);
                var size = res * tileSize; // 4891.9698095703125
                var x = (int)Math.Floor((point.X - tileOrigin.X) / size);
                var y = (int)Math.Floor((tileOrigin.Y - point.Y) / size);
                return new XYZ
                {
                    Z = zoom,
                    X = x,
                    Y = y
                };
            }

            public override string ToString()
            {
                return $"{X},{Y},{Z}";
            }
        }

        public record Point(double X, double Y);


        public record Extent
        {
            public long Id;
            public double XMin;
            public double YMin;
            public double XMax;
            public double YMax;
        }
    }
}