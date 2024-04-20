using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using NetTopologySuite.Geometries;
using Xunit;
using ZMap.Source.Postgre;

namespace ZServer.Tests;

public class PostgreSourceTests
{
    [Fact]
    public async Task GetFeaturesInExtentAsync()
    {
        Environment.SetEnvironmentVariable("EnableSensitiveDataLogging", "true");

        var source =
            new PostgreSource(
                "User ID=postgres;Password=oVkr7GiT29CAkw;Host=10.0.10.190;Port=5432;Database=zserver_dev;Pooling=true;",
                "zserver_dev");
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
        var a = HttpUtility.UrlEncode(source.Filter);
        var list =
            (await source.GetFeaturesInExtentAsync(new Envelope(52.31301, 52.41318, 13.12318, 13.22347))).ToList();
    }
}