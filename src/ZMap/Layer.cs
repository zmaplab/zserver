using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems.Transformations;
using ZMap.Extensions;
using ZMap.Infrastructure;
using ZMap.Source;
using ZMap.Style;

namespace ZMap
{
    public class Layer : IVisibleLimit
    {
        private static readonly IZMapStyleVisitor StyleVisitor = new ZMapStyleVisitor();
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
        public bool Enabled { get; set; }

        /// <summary>
        /// 显示范围(一般由数据源覆盖范围决定)
        /// </summary>
        public Envelope Envelope { get; private set; }

        /// <summary>
        /// 栅格缓冲
        /// </summary>
        public List<GridBuffer> Buffers { get; set; } = new();

        /// <summary>
        /// 数据源
        /// </summary>
        public ISource Source { get; private set; }

        /// <summary>
        /// 空间标识符
        /// </summary>
        public int Srid => Source.Srid;

        /// <summary>
        /// 
        /// </summary>
        public HashSet<ServiceType> Services { get; set; }

        /// <summary>
        /// 资源组
        /// </summary>
        public ResourceGroup ResourceGroup { get; set; }


        public Layer(ResourceGroup resourceGroup, HashSet<ServiceType> services, string name, ISource source,
            List<StyleGroup> styleGroups, Envelope envelope = null) :
            this(name, source, styleGroups, envelope)
        {
            ResourceGroup = resourceGroup;
            Services = services;
        }

        private Layer(string name, ISource source, List<StyleGroup> styleGroups, Envelope envelope = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "图层名称不能为空");
            }

            Name = name;
            Source = source ?? throw new ArgumentNullException(nameof(source), "图层数据源未配置");
            Enabled = true;
            Envelope = envelope ?? source.GetEnvelope();

            // if (styleGroups == null || !styleGroups.Any())
            // {
            //     throw new ArgumentNullException(nameof(styleGroups), "图层样式未配置");
            // }

            _styleGroups = styleGroups ?? new();
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
            // 所有的样式都不可显示，则不需要渲染，提前退出，避免数据查询开销
            if (!StyleGroups.Any(x => x.IsVisible(zoom)))
            {
                return;
            }

            // 由前端的 SRID 的 extent 转换成数据源的 extent
            // 先转换成数据源的 SRID 数据， 才能做相交判断是否超出数据范围
            var extent = viewport.Extent.Transform(srid, Srid);

            // 不在当前图层的范围内，不需要渲染
            if (Envelope != null && !Envelope.Intersects(extent))
            {
                return;
            }

            // if (EnvironmentVariables.EnableSensitiveDataLogging)
            // {
            //     if (srid != Srid)
            //     {
            //         Log.Logger.LogInformation(
            //             $"From viewport extent {viewport.Extent.MinX},{viewport.Extent.MinY},{viewport.Extent.MaxX},{viewport.Extent.MaxY} {srid} to {extent.MinX},{extent.MinY},{extent.MaxX},{extent.MaxY} {Srid}");
            //     }
            // }

            // comments: 此转换不可少的原因是，若以数据源的坐标系绘制图片，会导致偏差，在图片与图片接缝处对不齐
            // // 使用应用层投影转换的原因是规避数据库支持问题
            // // 假如只是使用数据库当成存储层， 则不考虑空间计算问题， 完全可以把矢量数据存成二进制， 计算好图形的 extent 存为 minx, maxX, miny, maxy
            // // 则可以通过计算求出相交的所有图形
            ICoordinateTransformation transformation = null;

            if (Srid != srid)
            {
                transformation = CoordinateReferenceSystem.CreateTransformation(Srid, srid);
            }

            var environments = new Dictionary<string, dynamic>
            {
                { Defaults.WmsScaleKey, zoom.Value }
            };

            switch (Source)
            {
                case IVectorSource vectorSource:
                    Envelope renderEnvelope;
                    // 不同级别使用不同的 Buffer，比如左右两个相领的图形，右边图形绘制文字特别长，但图形若没有交在左边，会导到文字只显示部分
                    // 不过，文字的绘制可以优化到 TextRender 如果超过边界，就进行移动
                    var buffer = Buffers.FirstOrDefault(x => x.IsVisible(zoom));
                    if (buffer is { Size: > 0 })
                    {
                        var xPerPixel = extent.Width / viewport.Width;
                        var yPerPixel = extent.Height / viewport.Height;
                        var x = xPerPixel * buffer.Size;
                        var y = yPerPixel * buffer.Size;
                        renderEnvelope = new Envelope(extent.MinX - x, extent.MaxX + x,
                            extent.MinY - y,
                            extent.MaxY + y);
                    }
                    else
                    {
                        renderEnvelope = extent;
                    }

                    await RenderVectorAsync(graphicsService, vectorSource, renderEnvelope, viewport.Extent, zoom,
                        transformation, environments);
                    break;
                case IRasterSource rasterSource:
                    await RenderRasterAsync(graphicsService, rasterSource, viewport.Extent, zoom);
                    break;
            }
        }

        public Layer Clone()
        {
            var styleGroups = new List<StyleGroup>();
            foreach (var styleGroup in StyleGroups)
            {
                styleGroups.Add(styleGroup.Clone());
            }

            return new Layer(ResourceGroup, Services, Name, Source.Clone(), styleGroups, Envelope)
            {
                MinZoom = MinZoom,
                MaxZoom = MaxZoom,
                ZoomUnit = ZoomUnit,
                Enabled = Enabled,
                Buffers = Buffers
            };
        }

        // public void ClearEnvironments()
        // {
        //     _environments?.Clear();
        // }

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

                    service.Render(image, rasterStyle);
                }
            }
        }

        private async Task RenderVectorAsync(IGraphicsService service, IVectorSource vectorSource,
            Envelope queryEnvelope,
            Envelope targetExtent,
            Zoom zoom, ICoordinateTransformation transformation, IReadOnlyDictionary<string, dynamic> environments
        )
        {
            // 需要考虑 CRS 的 AxisOrder.NORTH_EAST， 影响 Feature -> Word 的转换算法

            var features = await vectorSource.GetFeaturesInExtentAsync(queryEnvelope);

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

                feature.SetEnvironment(environments);

                foreach (var styleGroup in StyleGroups)
                {
                    // 若有配置过滤表达式， 则计算
                    if (styleGroup.Filter is { Value: not null } && !styleGroup.Filter.Value.Value)
                    {
                        continue;
                    }

                    if (!styleGroup.IsVisible(zoom))
                    {
                        continue;
                    }

                    // 需要保证 StyleGroups 每次都是新对象
                    // 不然 Accept 后会导致数据异常
                    styleGroup.Accept(StyleVisitor, feature);

                    foreach (var style in styleGroup.Styles)
                    {
                        if (!style.IsVisible(zoom) || style is not VectorStyle vectorStyle)
                        {
                            continue;
                        }

                        // 若有配置过滤表达式， 则计算
                        if (style.Filter is { Value: not null } && !style.Filter.Value.Value)
                        {
                            continue;
                        }

                        service.Render(targetExtent, feature.Geometry, vectorStyle);
                    }
                }

                feature.Dispose();
                count++;
            }

            stopwatch.Stop();

            if (EnvironmentVariables.EnableSensitiveDataLogging)
            {
                Log.Logger.LogInformation(
                    "[{Identifier}] layer: {Name}, width: {Width}, height: {Height}, filter: {Filter}, feature count: {Count}, rendering: {ElapsedMilliseconds}",
                    service.Identifier, this.Name, service.Width, service.Height, vectorSource.Filter, count,
                    stopwatch.ElapsedMilliseconds);
            }
        }

        public override string ToString()
        {
            return ResourceGroup == null ? Name : $"{ResourceGroup.Name}:{Name}";
        }

//         private async Task PaintAsync(IVectorSource vectorSource, Envelope envelope,
//             ICoordinateTransformation transformation, RenderContext context,
//             string filter = null,
//             string traceId = null)
//         {
//             IEnumerable<Feature> features;
//
//             var fetching = string.Empty;
//             if (string.Equals("TRACE_FETCH", "true",
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