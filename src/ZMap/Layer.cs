using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems.Transformations;
using ZMap.Extensions;
using ZMap.Source;
using ZMap.Style;
using ZMap.Utilities;

namespace ZMap
{
    public class Layer : ILayer
    {
        private readonly List<StyleGroup> _styleGroups;

        /// <summary>
        /// 图层名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 最小可视缩放
        /// </summary>
        public double MinZoom { get; set; }

        /// <summary>
        /// 最大可视缩放
        /// </summary>
        public double MaxZoom { get; set; }

        /// <summary>
        /// 缩放单位
        /// </summary>
        public ZoomUnits ZoomUnit { get; set; }

        /// <summary>
        /// 样式
        /// </summary>
        public IReadOnlyCollection<StyleGroup> StyleGroups => _styleGroups;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; private set; }

        /// <summary>
        /// 显示范围(一般由数据源覆盖范围决定)
        /// </summary>
        public Envelope Envelope { get; private set; }

        /// <summary>
        /// 数据源
        /// </summary>
        public ISource Source { get; private set; }

        /// <summary>
        /// 空间标识符
        /// </summary>
        public int SRID => Source.SRID;

        public Layer(string name, ISource source, List<StyleGroup> styleGroups, Envelope envelope = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
            Source = source ?? throw new ArgumentNullException(nameof(source), "图层数据源未配置");
            Enabled = true;
            Envelope = envelope ?? source.GetEnvelope();

            if (styleGroups == null || !styleGroups.Any())
            {
                throw new ArgumentNullException(nameof(styleGroups), "图层样式未配置");
            }

            _styleGroups = styleGroups;
        }

        public async Task RenderAsync(IGraphicsService graphicsService, Viewport viewport, Zoom zoom, int srid)
        {
            // 图层未启用或者不在显示级别
            if (!Enabled || !this.IsVisible(zoom))
            {
                return;
            }

            // 数据源为空: 在构造中检测了
            // 没有样式: 在构造中检测了
            // 所有的样式都不可显示
            // 则不需要渲染， 提前退出， 避免数据查询开销
            if (StyleGroups == null || StyleGroups.Count == 0 ||
                !StyleGroups.Any(styleGroup => styleGroup.IsVisible(zoom)))
            {
                return;
            }

            // 由前端的 SRID 的 extent 转换成数据源的 extent
            var extent = viewport.Extent.Transform(SRID, srid);

            // 不在当前图层的范围内，不需要渲染
            if (Envelope != null && !Envelope.Intersects(extent))
            {
                return;
            }

            // 使用应用层投影转换的原因是规避数据库支持问题
            // 假如只是使用数据库当成存储层，则不考虑空间计算问题，完全可以把矢量数据存成二进制，计算好图形的 extent 存为 minx, maxX, miny, maxy
            // 则可以通过计算求出相交的所有图形
            ICoordinateTransformation transformation = null;

            if (SRID != srid)
            {
                transformation = CoordinateTransformUtilities.GetTransformation(SRID, srid);
            }

            switch (Source)
            {
                case IVectorSource vectorSource:
                    await RenderVectorAsync(graphicsService, vectorSource, zoom,
                        transformation);
                    break;
                case IRasterSource rasterSource:
                    await RenderRasterAsync(graphicsService, rasterSource, extent, zoom);
                    break;
                // case IWMTSSource wmts:
                // {
                //     await RenderWMTSAsync(graphicsService, wmts, extent, zoom);
                //     break;
                // }
            }
        }

        private async Task RenderRasterAsync(IGraphicsService service, IRasterSource rasterSource, Envelope extent,
            Zoom zoom)
        {
            var image = await rasterSource.GetImageInExtentAsync(extent);

            foreach (var styleGroup in StyleGroups)
            {
                if (!styleGroup.IsVisible(zoom))
                {
                    continue;
                }

                foreach (var style in styleGroup.Styles)
                {
                    if (!style.IsVisible(zoom) || style is not RasterStyle rasterStyle)
                    {
                        continue;
                    }

                    service.Render(rasterStyle, image);
                }
            }
        }

        private async Task RenderVectorAsync(IGraphicsService service, IVectorSource vectorSource,
            Zoom zoom,
            ICoordinateTransformation transformation)
        {
            IEnumerable<Feature> features;

            var fetching = string.Empty;
            if (string.Equals(Environment.GetEnvironmentVariable("TRACE_FETCH"), "true",
                    StringComparison.InvariantCultureIgnoreCase))
            {
                var stopwatch1 = new Stopwatch();
                stopwatch1.Start();
                features = (await vectorSource.GetFeaturesInExtentAsync(service.Envelope)).ToList();
                stopwatch1.Stop();
                fetching = stopwatch1.ElapsedMilliseconds.ToString();
            }
            else
            {
                features = await vectorSource.GetFeaturesInExtentAsync(service.Envelope);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var count = 0;

            foreach (var feature in features)
            {
                if (feature == null || feature.IsEmpty)
                {
                    continue;
                }

                // simplify 后可能会有点减少， 因此放前面可以减少 transform 的工作量
                // simplify 的算法要控制，消点效果不是很好
                // feature.Simplify();

                if (transformation != null)
                {
                    feature.Transform(transformation);
                }

                foreach (var styleGroup in StyleGroups)
                {
                    if (!styleGroup.IsVisible(zoom))
                    {
                        continue;
                    }

                    foreach (var style in styleGroup.Styles)
                    {
                        if (!style.IsVisible(zoom) || style is not VectorStyle vectorStyle)
                        {
                            continue;
                        }

                        // 若有配置过滤表达式， 则计算
                        var expression = style.Filter?.Invoke(feature);
                        var func = DynamicCompilationUtilities.GetAvailableFunc(expression);
                        if (func != null && !func(feature))
                        {
                            continue;
                        }

                        service.Render(vectorStyle, feature);
                    }
                }

                count++;
            }

            stopwatch.Stop();
            if (count > 0)
            {
                Log.Logger.LogInformation(
                    fetching == string.Empty
                        ? $"[{service.MapId}] layer: {this}, width: {service.Width}, height: {service.Height}, filter: {vectorSource.Filter}, feature count: {count}, rendering: {stopwatch.ElapsedMilliseconds}"
                        : $"[{service.MapId}] layer: {this}, width: {service.Width}, height: {service.Height}, filter: {vectorSource.Filter}, feature count: {count}, fetching: {fetching}, rendering: {stopwatch.ElapsedMilliseconds}");
            }
            else
            {
#if DEBUG
                Log.Logger.LogInformation(
                    fetching == string.Empty
                        ? $"[{service.MapId}] layer: {this}, width: {service.Width}, height: {service.Height}, filter: {vectorSource.Filter}, feature count: {count}, rendering: {stopwatch.ElapsedMilliseconds}"
                        : $"[{service.MapId}] layer: {this}, width: {service.Width}, height: {service.Height}, filter: {vectorSource.Filter}, feature count: {count}, fetching: {fetching}, rendering: {stopwatch.ElapsedMilliseconds}");
#endif
            }
        }

//         private async Task PaintAsync(IVectorSource vectorSource, Envelope envelope,
//             ICoordinateTransformation transformation, RenderContext context,
//             string filter = null,
//             string traceId = null)
//         {
//             IEnumerable<Feature> features;
//
//             var fetching = string.Empty;
//             if (string.Equals(Environment.GetEnvironmentVariable("TRACE_FETCH"), "true",
//                     StringComparison.InvariantCultureIgnoreCase))
//             {
//                 var stopwatch1 = new Stopwatch();
//                 stopwatch1.Start();
//                 features = (await vectorSource.GetFeaturesInExtentAsync(envelope)).ToArray();
//                 stopwatch1.Stop();
//                 fetching = stopwatch1.ElapsedMilliseconds.ToString();
//             }
//             else
//             {
//                 features = await vectorSource.GetFeaturesInExtentAsync(envelope);
//             }
//
//             var stopwatch = new Stopwatch();
//             stopwatch.Start();
//
//             var count = 0;
//
//             foreach (var feature in features)
//             {
//                 if (feature == null || feature.IsEmpty)
//                 {
//                     continue;
//                 }
//
//                 // simplify 后可能会有点减少， 因此放前面可以减少 transform 的工作量
//                 // feature.Simplify();
//
//                 if (transformation != null)
//                 {
//                     feature.Transform(transformation);
//                 }
//
//                 foreach (var styleGroup in StyleGroups)
//                 {
//                     if (!styleGroup.IsVisible(context.Viewport.ZoomOrScale, context.Viewport.VisibilityUnits))
//                     {
//                         continue;
//                     }
//
//                     foreach (var style in styleGroup.Styles)
//                     {
//                         if (!style.IsVisible(context.Viewport.ZoomOrScale, context.Viewport.VisibilityUnits))
//                         {
//                             continue;
//                         }
//
//                         // 若有配置过滤表达式， 则计算
//                         var expression = style.Filter?.Invoke(feature);
//                         var func = DynamicCompilationUtilities.GetAvailableFunc(expression);
//                         if (func != null && !func(feature))
//                         {
//                             continue;
//                         }
//
//                         var renderer = context.RendererFactory.Create(style);
//                         
//                         if ( is IVectorRenderer render)
//                         {
//                               renderer.Render(context, feature);
//                         }
//                     }
//                 }
//
//                 count++;
//             }
//
//             stopwatch.Stop();
//             if (count > 0)
//             {
//                 Log.Logger.LogInformation(
//                     fetching == string.Empty
//                         ? $"[{traceId}] layer: {this}, width: {context.Viewport.Width}, height: {context.Viewport.Height}, filter: {filter}, feature count: {count}, rendering: {stopwatch.ElapsedMilliseconds}"
//                         : $"[{traceId}] layer: {this}, width: {context.Viewport.Width}, height: {context.Viewport.Height}, filter: {filter}, feature count: {count}, fetching: {fetching}, rendering: {stopwatch.ElapsedMilliseconds}");
//             }
//             else
//             {
// #if DEBUG
//                 Log.Logger.LogInformation(
//                     fetching == string.Empty
//                         ? $"[{traceId}] layer: {this}, width: {context.Viewport.Width}, height: {context.Viewport.Height}, filter: {filter}, feature count: {count}, rendering: {stopwatch.ElapsedMilliseconds}"
//                         : $"[{traceId}] layer: {this}, width: {context.Viewport.Width}, height: {context.Viewport.Height}, filter: {filter}, feature count: {count}, fetching: {fetching}, rendering: {stopwatch.ElapsedMilliseconds}");
// #endif
//             }
//         }

        // private async Task PaintAsync(IImageSource rasterSource, Envelope envelope, RenderContext context,
        //     string filter = null,
        //     string traceId = null)
        // {
        //     var image = await rasterSource.GetImageInExtentAsync(envelope);
        //     foreach (var styleGroup in StyleGroups)
        //     {
        //         if (!styleGroup.IsVisible(context.Viewport.ZoomOrScale, context.Viewport.VisibilityUnits))
        //         {
        //             continue;
        //         }
        //
        //         // transform points
        //         foreach (var style in styleGroup.Styles)
        //         {
        //             // 若有配置过滤表达式， 则计算
        //             if (context.RendererFactory.Create(style) is IRasterStyleRender render)
        //             {
        //                 await render.PaintAsync(context, image);
        //             }
        //         }
        //     }
        // }
    }
}