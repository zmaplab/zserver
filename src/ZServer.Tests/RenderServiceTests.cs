// using System.Collections.Generic;
// using System.IO;
// using System.Threading.Tasks;
// using NetTopologySuite.Geometries;
// using Xunit;
// using ZMap;
// using ZMap.Utility;
//
// namespace ZServer.Tests
// {
//     public class RenderServiceTests : BaseTests
//     {
//         [Fact]
//         public async Task ShapeRender()
//         {
//             var renderService = (RenderService) GetScopedService<IRenderService>();
//             var bytes = await renderService.RenderAsync(new List<LayerQuery>
//                 {
//                     new("berlin_shp")
//                 }, new Viewport(new Envelope(52.4978, 52.51, 13.26, 13.2814), 4326, 256, 256)
//                 {
//                     ImageFormat = "image/png"
//                 }
//             );
//             await File.WriteAllBytesAsync($"images/{GetType().Name}_ShapeRender.png", bytes);
//         }
//
//         [Fact]
//         public async Task RenderPostGis()
//         {
//             var renderService = (RenderService) GetScopedService<IRenderService>();
//             var bytes = await renderService.RenderAsync(new List<LayerQuery>
//                 {
//                     new("polygon_db")
//                 }, new Viewport(new Envelope(52.4978, 52.51, 13.26, 13.2814), 4326, 256, 256)
//                 {
//                     ImageFormat = "image/png"
//                 }
//             );
//             await File.WriteAllBytesAsync($"images/{GetType().Name}_PGRender.png", bytes);
//         }
//
//         [Fact]
//         public async Task LayerGroup()
//         {
//             var renderService = (RenderService) GetScopedService<IRenderService>();
//             var bytes = await renderService.RenderAsync(new List<LayerQuery>
//             {
//                 new("berlin_group")
//             }, new Viewport(new Envelope(52.4978, 52.51, 13.26, 13.2814), 4326, 256, 256)
//             {
//                 ImageFormat = "image/png"
//             });
//             await File.WriteAllBytesAsync($"images/{GetType().Name}_LayerGroup.png", bytes);
//         }
//
//         [Fact]
//         public async Task ShapeTransform()
//         {
//             var extent =
//                 new Envelope(52.4978, 52.51, 13.26, 13.2814).Transform(4326, 3857);
//             var renderService = (RenderService) GetScopedService<IRenderService>();
//             var bytes = await renderService.RenderAsync(new List<LayerQuery>
//             {
//                 new("berlin_shp")
//             }, new Viewport(extent, 3857, 256, 256)
//             {
//                 ImageFormat = "image/png"
//             });
//             await File.WriteAllBytesAsync($"images/{GetType().Name}_ShapeTransform.png", bytes);
//         }
//     }
// }