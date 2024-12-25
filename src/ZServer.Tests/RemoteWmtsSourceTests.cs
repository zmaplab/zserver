using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ZMap.Source;

namespace ZServer.Tests;

[Collection("WebApplication collection")]
public class RemoteWmtsSourceTests(WebApplicationFactoryFixture fixture)
{
    [Fact]
    public async Task GetRemoteWmts()
    {
        var httpClientFactory = fixture.Instance.Services.GetRequiredService<IHttpClientFactory>();
        var token = Environment.GetEnvironmentVariable("TIANDITU_TOKEN");

        var source = new RemoteWmtsSource(
            $$"""
              https://t0.tianditu.gov.cn/img_c/wmts?tk={{token}}&layer=img&style=default&tilematrixset=c&Service=WMTS&Request=GetTile&Version=1.0.0&Format=tiles&TileMatrix={0}&TileRow={1}&TileCol={2}
              """);
        source.Name = "test";
        source.Key = "test";
        // source.Srid = 4326;
        // source.MatrixSet = "EPSG:4326";
        source.HttpClientFactory = httpClientFactory;

        await source.LoadAsync();
        var image = await source.GetImageAsync("9", 80, 423);
        await File.WriteAllBytesAsync("images/remote_wmts.png", image);
    }
}