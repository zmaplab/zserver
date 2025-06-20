using System.Threading.Tasks;
using ProjNet.CoordinateSystems;
using ProjNet.IO.CoordinateSystems;
using Xunit;
using ZMap.Infrastructure;

namespace ZServer.Tests;

public class CoordinateSystemComparerTests
{
    [Fact]
    public void Compare4490()
    {
        var esri4490wkt = """
                          GEOGCS["GCS_China_Geodetic_Coordinate_System_2000",
                          DATUM["D_China_2000",
                              SPHEROID["CGCS2000",6378137.0,298.257222101]],
                          PRIMEM["Greenwich",0.0],
                          UNIT["Degree",0.0174532925199433]]
                          """;
        var cs1 = (CoordinateSystem)CoordinateSystemWktReader.Parse(esri4490wkt);
        var cs4490 = CoordinateReferenceSystem.Get(4490);
        var result = CoordinateSystemComparer.AreEqual(cs1, cs4490);
        Assert.True(result);
    }
}