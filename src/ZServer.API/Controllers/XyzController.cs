// using System;
// using System.Text;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Http.Extensions;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Logging;
// using Orleans;
// using ZServer.Interfaces;
//
// namespace ZServer.API.Controllers
// {
//     [ApiController]
//     [Microsoft.AspNetCore.Components.Route("[controller]")]
//     public class XyzController : ControllerBase
//     {
//         private readonly ILogger<XyzController> _logger;
//         private readonly IClusterClient _clusterClient;
//
//         private static readonly float tileOriginX = -180.00F;
//
//         private static readonly float tileOriginY = 180.00F;
//         // private static readonly HashAlgorithmService HashAlgorithmService = new();
//
//         public XyzController(ILogger<XyzController> logger, IClusterClient clusterClient)
//         {
//             _logger = logger;
//             _clusterClient = clusterClient;
//         }
//
//         [HttpGet]
//         public async Task<IActionResult> GetAsync(string layers,
//             int x, int y, int z)
//         {
//             var displayUrl = Request.GetDisplayUrl();
//             if (string.IsNullOrWhiteSpace(layers))
//             {
//                 _logger.LogError($"LAYERS should not be empty/null: {displayUrl}");
//                 return NotFound();
//             }
//
//             var layersContainsWorkspace = layers.Split(',', StringSplitOptions.RemoveEmptyEntries);
//             var queryLayers = new Layer[layersContainsWorkspace.Length];
//             var stringBuilder = new StringBuilder();
//
//             for (var i = 0; i < layersContainsWorkspace.Length; ++i)
//             {
//                 var layer = layersContainsWorkspace[i];
//                 var data = layer.Split(':', StringSplitOptions.RemoveEmptyEntries);
//                 if (data.Length != 2)
//                 {
//                     _logger.LogError($"LAYERS should not be empty/null: {displayUrl}");
//                     return NotFound();
//                 }
//
//                 var dto = new Layer(data[0], data[1], string.Empty);
//                 queryLayers[i] = dto;
//                 stringBuilder.Append($"{dto.Workspace}:{dto.Name}:");
//             }
//
//             var friend = _clusterClient.GetGrain<ITileGrain>($"{z}/{x}/{y}");
//             var bytes = await friend.GetMapAsync(queryLayers, z, x, y);
//
//             return File(bytes, "image/png");
//         }
//     }
// }