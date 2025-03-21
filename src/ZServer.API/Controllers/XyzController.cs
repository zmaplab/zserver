// using System.ComponentModel.DataAnnotations;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Http.Extensions;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Logging;
// using Orleans;
// using SkiaSharp;
// using ZMap.Source.CloudOptimizedGeoTIFF;
// using ZServer.Store;
//
// namespace ZServer.API.Controllers;
//
// [ApiController]
// [Microsoft.AspNetCore.Components.Route("[controller]")]
// public class XyzController(ILogger<XyzController> logger, IClusterClient clusterClient, ISourceStore sourceStore)
//     : ControllerBase
// {
//     [HttpGet("{layers}")]
//     public async Task<IActionResult> GetAsync([FromRoute, Required, StringLength(100)] string layers,
//         [FromQuery] int x, [FromQuery] int y, [FromQuery, StringLength(100)] string z,
//         [StringLength(20)] string format = "image/png")
//     {
//         var displayUrl = Request.GetDisplayUrl();
//         if (string.IsNullOrWhiteSpace(layers))
//         {
//             logger.LogError($"LAYERS should not be empty/null: {displayUrl}");
//             return NotFound();
//         }
//
//         // var sourceList = layers.Split(',', StringSplitOptions.RemoveEmptyEntries);
//         // foreach (var source in sourceList)
//         // {
//         //     
//         // }
//         var source = await sourceStore.FindAsync(layers);
//         if (source is COGGeoTiffSource cogSource)
//         {
//             var image = await cogSource.GetImageAsync(z, x, y);
//
//             var skiaImage = SKImage.FromEncodedData(image);
//             var imageFormat = GetImageFormat(format);
//             await using var stream = skiaImage.Encode(imageFormat, 90).AsStream();
//             return File(stream, "image/png");
//         }
//
//         return File([], "image/png");
//     }
//
//     private SKEncodedImageFormat GetImageFormat(string format)
//     {
//         return format switch
//         {
//             "image/png" => SKEncodedImageFormat.Png,
//             "image/jpeg" => SKEncodedImageFormat.Jpeg,
//             "image/webp" => SKEncodedImageFormat.Webp,
//             "image/gif" => SKEncodedImageFormat.Gif,
//             "image/bmp" => SKEncodedImageFormat.Bmp,
//             _ => SKEncodedImageFormat.Png
//         };
//     }
// }