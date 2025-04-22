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
    public void FreeSqlFilter()
    {
        var source =
            new PostgreSource(
                "User ID=postgres;Password=oVkr7GiT29CAkw;Host=10.0.10.190;Port=5432;Database=zserver_dev;Pooling=true;");
        source.Table = "osmbuildings";
        source.Geometry = "geom";
        source.Srid = 4326;
        source.Where = "id > 0";
        source.Name = "osmbuildings";
        var sql = source.CombineFilter("select * from table ",
            """
            {
              "Logic": "And",
              "Filters": [
                {
                  "Field": "risk_level",
                  "Operator": "Any",
                  "Value": "1,2"
                }
              ]
            }
            """
        );
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