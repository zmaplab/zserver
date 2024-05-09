using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ZServer.Tests;

public class ApiTests(WebApplicationFactory<API.Program> factory)
    : IClassFixture<WebApplicationFactory<API.Program>>
{
    [Fact]
    public async Task Wms()
    {
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

        var client = factory.CreateClient();
        var requestMessage = new HttpRequestMessage(HttpMethod.Get,
            $"wms?SERVICE=WMS&VERSION=1.1.1&REQUEST=GetMap&FORMAT=image%2Fpng&TRANSPARENT=true&TILED=true&LAYERS=zserver%3Apolygon&bordered=true&WIDTH=256&HEIGHT=256&SRS=EPSG%3A4326&STYLES=&BBOX=12.513427734375%2C41.85791015625%2C12.5244140625%2C41.868896484375&CQL_FILTER={filter}");
        var response = await client.SendAsync(requestMessage);
        var bytes = await response.Content.ReadAsByteArrayAsync();
    }
}