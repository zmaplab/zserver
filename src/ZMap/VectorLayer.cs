namespace ZMap;

/// <summary>
/// 图层
/// </summary>
public partial class Layer
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="graphicsService"></param>
    /// <param name="vectorSource"></param>
    /// <param name="viewportExtent"></param>
    /// <param name="viewportSrid"></param>
    /// <param name="dataSourceExtent"></param>
    /// <param name="zoom"></param>
    /// <param name="height"></param>
    /// <param name="environments"></param>
    /// <param name="width"></param>
    private async Task RenderAsync(IGraphicsService graphicsService, IVectorSource vectorSource,
        Envelope viewportExtent, int viewportSrid,
        Zoom zoom, int width, int height, Envelope dataSourceExtent, IReadOnlyDictionary<string, dynamic> environments
    )
    {
        Envelope renderEnvelope;

        // 不同级别使用不同的 Buffer， 比如左右两个相领的图形， 右边图形绘制文字特别长， 但图形若没有交在左边， 会导到文字只显示部分
        // 不过，文字的绘制可以优化到 TextRender 如果超过边界，就进行移动（不一定合适， 只能在无 buffer 的情况下使用)
        var buffer = Buffers.FirstOrDefault(x => x.IsVisible(zoom));
        if (buffer is { Size: > 0 })
        {
            var xPerPixel = dataSourceExtent.Width / width;
            var yPerPixel = dataSourceExtent.Height / height;
            var x = xPerPixel * buffer.Size;
            var y = yPerPixel * buffer.Size;
            renderEnvelope = new Envelope(dataSourceExtent.MinX - x, dataSourceExtent.MaxX + x,
                dataSourceExtent.MinY - y,
                dataSourceExtent.MaxY + y);
        }
        else
        {
            renderEnvelope = dataSourceExtent;
        }

        var targetEnvelope = viewportExtent;

        // 需要考虑 CRS 的 AxisOrder.NORTH_EAST， 影响 Feature -> Word 的转换算法
        // comments: 此转换不可少的原因是，若以数据源的坐标系绘制图片，会导致偏差，在图片与图片接缝处对不齐
        // // 使用应用层投影转换的原因是规避数据库支持问题
        // // 假如只是使用数据库当成存储层， 则不考虑空间计算问题， 完全可以把矢量数据存成二进制， 计算好图形的 extent 存为 minx, maxX, miny, maxy
        // // 则可以通过计算求出相交的所有图形
        ICoordinateTransformation transformation = null;

        if (Srid != viewportSrid)
        {
            transformation = CoordinateReferenceSystem.CreateTransformation(Srid, viewportSrid);
        }

        var features = await vectorSource.GetFeaturesAsync(renderEnvelope, Filter);

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var count = 0;

        // comments: 此处是按 feature 顺序进行渲染， 优势是 features 是按 Reader 方式读取， 一次只有一条数据在内存中
        // 缺点是， 不同 feature 之间的空间关系可能导致 style 顺序渲染效果不达预期
        // 举个例子： 两个相邻的多边型 A、B， 都渲染了填充 + 文字， 其中 A 文字较长， 侵入了 B 的范围
        // 由于先把 A 的所有 style 渲染完， 再渲染 B， 导致 A 的文字被 B 的填充覆盖
        // 建议是， 文字类等独立图层， 再合并到图层组中来保证渲染效果
        // 我们还是内存控制优先， 一个 bbox 覆盖的矢量数据量太大， 内存直接炸掉
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

            foreach (var sg in StyleGroups)
            {
                var styleGroup = sg.Clone();

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

                    graphicsService.Render(targetEnvelope, feature.Geometry, vectorStyle);
                }
            }

            feature.Dispose();
            count++;
        }

        stopwatch.Stop();

        if (EnvironmentVariables.EnableSensitiveDataLogging)
        {
            Logger.Value.LogInformation(
                "[{Identifier}] layer: {Name}, bbox: {MinX},{MinY},{MaxX},{MaxY}, srid: {srid} width: {Width}, height: {Height}, filter: {Filter}, feature count: {Count}, rendering: {ElapsedMilliseconds}",
                graphicsService.TraceIdentifier, Name, renderEnvelope.MinX, renderEnvelope.MinY, renderEnvelope.MaxX,
                renderEnvelope.MaxY, viewportSrid, graphicsService.Width, graphicsService.Height, Filter, count,
                stopwatch.ElapsedMilliseconds);
        }
    }
}