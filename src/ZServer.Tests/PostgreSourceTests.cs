using System;
using System.Linq;
using System.Threading.Tasks;
using FreeSql;
using NetTopologySuite.Geometries;
using Npgsql;
using Xunit;
using ZMap.Source.Postgre;

namespace ZServer.Tests;

public class PostgreSourceTests
{
    [Fact]
    public void TestGeom()
    {
        var freeSql = new FreeSqlBuilder()
            .UseConnectionFactory(DataType.PostgreSQL, () =>
            {
                var connStr = Environment.GetEnvironmentVariable("ConnStr");
                if (string.IsNullOrEmpty(connStr))
                {
                    connStr = "User ID=postgres;Password=oVkr7GiT29CAkw;Host=10.0.10.190;Port=5432;Database=zserver_dev;Pooling=true;";
                }
                var dataSourceBuilder = new NpgsqlDataSourceBuilder(connStr);
                dataSourceBuilder.UseNetTopologySuite();
                var dataSource = dataSourceBuilder.Build();
                return dataSource.CreateConnection();
            })
            .Build();
        var list = freeSql.Select<object>().WithSql("SELECT * FROM osmbuildings LIMIT 10").ToList();
    }

    [Fact]
    public async Task GetFeaturesInExtentAsync()
    {
        Environment.SetEnvironmentVariable("EnableSensitiveDataLogging", "true");
        var connStr = Environment.GetEnvironmentVariable("ConnStr");
        if (string.IsNullOrEmpty(connStr))
        {
            connStr = "User ID=postgres;Password=oVkr7GiT29CAkw;Host=10.0.10.190;Port=5432;Database=zserver_dev;Pooling=true;";
        }
        var source =
            new PostgreSource(
                connStr)
            {
                Table = "osmbuildings",
                Geometry = "geom",
                Srid = 4326,
                Where = "id > 0",
                Name = "osmbuildings"
            };
        var filter = """
                     {
                       "Logic": "And",
                       "Filters": [
                         {
                           "Field": "height",
                           "Operator": "GreaterThanOrEqual",
                           "Value": 10
                         }
                       ]
                     }
                     """;
        source.Key = "XXX";

        var list =
            (await source.GetFeaturesAsync(new Envelope(52.31301, 52.41318, 13.12318, 13.22347), filter))
            .ToList();
    }
}