using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using TagImageFileFormat;
using Xunit;
using ZMap.Infrastructure;
using ZMap.Source.CloudOptimizedGeoTIFF;

namespace ZServer.Tests;

[Collection("WebApplication collection")]
public class CogTests(WebApplicationFactoryFixture fixture)
{
    [Fact]
    public async Task GetHeader()
    {
        var url = "http://share-lhc.oss-cn-shanghai.aliyuncs.com/北仑区_webmerc_cog.tif";
        var httpClient = fixture.Instance.Services.GetRequiredService<IHttpClientFactory>().CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Range = new(0, 1024 * 64);
        var response = await httpClient.SendAsync(request);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        var j = Encoding.UTF8.GetString(bytes);
        await File.WriteAllBytesAsync("北仑区_webmerc_cog_header.tif", bytes);
        //
        // var crs = CoordinateReferenceSystem.Get(4549);

        var cog1 = new COGGeoTiffSource("北仑区_webmerc_cog_header.tif");
        await cog1.LoadAsync();

        // var cog2 = new COGGeoTiffSource("/Users/lewis/Downloads/北仑区_4549_cog.tif");
        // await cog2.LoadAsync();

        // var cog3 = new COGGeoTiffSource("/Users/lewis/Downloads/北仑区.tif");
        // await cog3.LoadAsync();
    }

    [Fact]
    public async Task GetCogImage()
    {
        // CogTileReader.ReadTile2("/Users/lewis/Downloads/tiled_cog_chengdu.tif");
        //     
        var cog = new COGGeoTiffSource("/Users/lewis/Downloads/tiled_cog_chengdu.tif");
        await cog.LoadAsync();

        // 3: 2X2
        await AssertAllTiles(cog, "3", 0, 0);
        await AssertAllTiles(cog, "3", 0, 1);
        await AssertAllTiles(cog, "3", 1, 0);
        await AssertAllTiles(cog, "3", 1, 1);
        // 4: 1X1
        await AssertAllTiles(cog, "4", 0, 0);
        // 5: 1X1
        await AssertAllTiles(cog, "5", 0, 0);

        using var reader = new BinaryReader(File.OpenRead("/Users/lewis/Downloads/tiled_cog_chengdu.tif"));
        var tiff = TIFF.Load(reader);
        await SaveImage(reader, tiff, 3, 0, 0);
        await SaveImage(reader, tiff, 3, 0, 1);
        await SaveImage(reader, tiff, 3, 1, 0);
        await SaveImage(reader, tiff, 3, 1, 1);
        await SaveImage(reader, tiff, 4, 0, 0);
        await SaveImage(reader, tiff, 5, 0, 0);
    }

    private async Task SaveImage(BinaryReader reader, TIFF tiff, int level, int col, int row)
    {
        var data = await tiff.GetTileAsync(reader, level, col, row);
        SaveImage($"images/tiff_{level}_{col}_{row}.png", data);
    }

    private void SaveImage(string path, int[] data)
    {
        var handle = GCHandle.Alloc(data, GCHandleType.Pinned);

        try
        {
            // int: 4 bytes, 32 bits
            // rgba8888: 4 bytes, 32 bits
            using var skiaImage =
                SKImage.FromPixels(
                    new SKImageInfo(512, 512, SKColorType.Rgba8888, SKAlphaType.Premul),
                    handle.AddrOfPinnedObject());
            using var bitmap = SKBitmap.FromImage(skiaImage);
            using var stream = new MemoryStream();
            bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
            File.WriteAllBytes(path, stream.ToArray());
        }
        finally
        {
            handle.Free();
        }
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

    private async Task AssertAllTiles(COGGeoTiffSource cogGeoTiffSourceSource, string zoom, int x, int y)
    {
        var image = await cogGeoTiffSourceSource.GetImageAsync(zoom, x, y);
        if (image is { IsEmpty: false })
        {
            var i = (int[])image.Data;
            var bitmap = new SKBitmap(image.Width, image.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
            var handle = GCHandle.Alloc(i, GCHandleType.Pinned);
            bitmap.SetPixels(handle.AddrOfPinnedObject());
            using var ms = new MemoryStream();
            using var skStream = new SKManagedWStream(ms);
            bitmap.Encode(skStream, SKEncodedImageFormat.Jpeg, 80);
            var resultArray = ms.ToArray();

            await File.WriteAllBytesAsync("images/cd_" + zoom + "_" + x + "_" + y + ".jpg", resultArray);
        }

        Assert.NotNull(image);
        Assert.False(image.IsEmpty);
    }
}