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
        var freeSql = new FreeSql.FreeSqlBuilder()
            .UseConnectionFactory(DataType.PostgreSQL, () =>
            {
                var dataSourceBuilder = new NpgsqlDataSourceBuilder(
                    "User ID=postgres;Password=oVkr7GiT29CAkw;Host=10.0.10.190;Port=5432;Database=zserver_dev;Pooling=true;");
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

        var source =
            new PostgreSource(
                "User ID=postgres;Password=oVkr7GiT29CAkw;Host=10.0.10.190;Port=5432;Database=zserver_dev;Pooling=true;");
        source.Table = "osmbuildings";
        source.Geometry = "geom";
        source.Srid = 4326;
        source.Where = "id > 0";
        source.Name = "osmbuildings";
        source.Filter = """
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

        var list =
            (await source.GetFeaturesInExtentAsync(new Envelope(52.31301, 52.41318, 13.12318, 13.22347))).ToList();
    }
}