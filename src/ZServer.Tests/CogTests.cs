using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ZMap.Source.CloudOptimizedGeoTIFF;

namespace ZServer.Tests;

[Collection("WebApplication collection")]
public class CogTests(WebApplicationFactoryFixture fixture)
{
    [Fact]
    public async Task GetCogImage()
    {
        var cog = new COGGeoTiffSource("/Users/lewis/Downloads/tiled_cog_chengdu.tif");
        var name = cog.GetType().FullName;
        await cog.LoadAsync();

        // for (var i = 0; i < 8; ++i)
        // {
        //     for (var j = 0; j < 9; ++j)
        //     {
        //         await AssertAllTiles(cog, "5", i, j);
        //     }
        // }

        await AssertAllTiles(cog, "3", 1, 0);
        await AssertAllTiles(cog, "3", 2, 0);
        await AssertAllTiles(cog, "3", 1, 1);
        await AssertAllTiles(cog, "3", 2, 0);


        // await AssertAllTiles(cog, "0", 0, 0);
        // await AssertAllTiles(cog, "1", 0, 0);
        // await AssertAllTiles(cog, "2", 0, 0);
        // await AssertAllTiles(cog, "2", 0, 1);

        // await AssertAllTiles(cog, "2", 1, 0);
        // await AssertAllTiles(cog, "2", 1, 1);
        // await AssertAllTiles(cog, "3", 2, 2);
        // await AssertAllTiles(cog, "4", 0, 0);
        // await AssertAllTiles(cog, "4", 4, 3);
    }

    [Fact(DisplayName = "Get cd tile")]
    public async Task GetRemoteWmts1Tile()
    {
        var service = fixture.Instance.Services.GetRequiredService<StoreRefreshService>();
        await service.StartAsync(default);
        var httpClient = fixture.Instance.CreateClient();
// TileMatrix=16&TileCol=54894&TileRow=10944
        // var result =
        //     await httpClient.GetAsync(
        //         "wmts?SERVICE=WMTS&REQUEST=GetTile&version=1.0.0&layer=cd&tileMatrixSet=EPSG:3857&format=image/png&TILEMATRIX=16&TILEROW=26889&TILECOL=51699");
        // var image = await result.Content.ReadAsByteArrayAsync();
        // await File.WriteAllBytesAsync("images/cd.png", image);
        // var hash1 = MurmurHashAlgorithmUtility.ComputeHash(image);
        // var bytes = await File.ReadAllBytesAsync("images/cd.png");
        // var hash2 = MurmurHashAlgorithmUtility.ComputeHash(bytes);
        // Assert.Equal(hash2, hash1);

        var result2 =
            await httpClient.GetAsync(
                "wmts?SERVICE=WMTS&REQUEST=GetTile&version=1.0.0&layer=cd&tileMatrixSet=EPSG:3857&format=image/png&TILEMATRIX=16&TILEROW=10794&TILECOL=51699");
        var image2 = await result2.Content.ReadAsByteArrayAsync();
        result2.EnsureSuccessStatusCode();
        var str = Encoding.UTF8.GetString(image2);
        await File.WriteAllBytesAsync("images/cd_2.png", image2);
    }

    [Fact(DisplayName = "Get cd tile cd")]
    public async Task GetWmts1Tile()
    {
        var service = fixture.Instance.Services.GetRequiredService<StoreRefreshService>();
        await service.StartAsync(default);
        var httpClient = fixture.Instance.CreateClient();

        var result2 =
            await httpClient.GetAsync(
                "wmts?SERVICE=WMTS&REQUEST=GetTile&version=1.0.0&layer=cd&tileMatrixSet=EPSG:4326&format=image/png&TILEMATRIX=17&TILEROW=21590&TILECOL=103398");
        var image2 = await result2.Content.ReadAsByteArrayAsync();
        result2.EnsureSuccessStatusCode();
        var str = Encoding.UTF8.GetString(image2);
        await File.WriteAllBytesAsync("images/cd_2.png", image2);
    }

    [Fact(DisplayName = "Get cd tile qtz")]
    public async Task GetWmtsQtzTile()
    {
        var service = fixture.Instance.Services.GetRequiredService<StoreRefreshService>();
        await service.StartAsync(default);
        var httpClient = fixture.Instance.CreateClient();

        var result1 =
            await httpClient.GetAsync(
                "wmts?SERVICE=WMTS&REQUEST=GetTile&version=1.0.0&layer=qtz&tileMatrixSet=EPSG:4326&format=image/png&TILEMATRIX=13&TILEROW=1382&TILECOL=6858&CQL_FILTER=");
        var image1 = await result1.Content.ReadAsByteArrayAsync();
        result1.EnsureSuccessStatusCode();

        await File.WriteAllBytesAsync("images/qtz_1.png", image1);

        var result2 =
            await httpClient.GetAsync(
                "wmts?SERVICE=WMTS&REQUEST=GetTile&version=1.0.0&layer=qtz&tileMatrixSet=EPSG:3857&format=image/png&TILEMATRIX=13&TILEROW=1382&TILECOL=6858&CQL_FILTER=");
        var image2 = await result2.Content.ReadAsByteArrayAsync();
        result2.EnsureSuccessStatusCode();

        await File.WriteAllBytesAsync("images/qtz_2.png", image2);
    }

    private async Task AssertAllTiles(COGGeoTiffSource cogGeoTiffSource, string zoom, int x, int y)
    {
        var image = await cogGeoTiffSource.GetImageAsync(zoom, x, y);
        if (image.Data is byte[] b)
        {
            await File.WriteAllBytesAsync("images/cd_" + zoom + "_" + x + "_" + y + ".png", b);
        }

        Assert.False(image.IsEmpty);
    }
}