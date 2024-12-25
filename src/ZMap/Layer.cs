using System.Net.Http;

namespace ZMap;

/// <summary>
/// 图层
/// </summary>
public partial class Layer : IVisibleLimit
{
    public static readonly IZMapStyleVisitor StyleVisitor = new ZMapStyleVisitor();
    private static readonly ILogger Logger = Log.CreateLogger<Layer>();

    /// <summary>
    /// 图层名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 资源组标识
    /// </summary>
    public string ResourceId { get; set; }

    /// <summary>
    /// 资源组
    /// </summary>
    public ResourceGroup ResourceGroup { get; set; }

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
    public List<StyleGroup> StyleGroups { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 显示范围(一般由数据源覆盖范围决定)
    /// </summary>
    public Envelope Envelope { get; set; }

    /// <summary>
    /// 栅格缓冲
    /// </summary>
    public List<GridBuffer> Buffers { get; set; }

    /// <summary>
    /// 数据源
    /// </summary>
    public ISource Source { get; set; }

    /// <summary>
    /// 空间标识符
    /// </summary>
    public int Srid => Source.Srid;

    /// <summary>
    /// 投影
    /// </summary>
    public CoordinateSystem CoordinateSystem => Source.CoordinateSystem;

    /// <summary>
    /// 
    /// </summary>
    public HashSet<ServiceType> Services { get; set; }

    /// <summary>
    /// 过滤条件
    /// </summary>
    public string Filter { get; set; }

    /// <summary>
    /// HttpClient 工厂
    /// </summary>
    public IHttpClientFactory HttpClientFactory { get; set; }

    public Layer(ResourceGroup resourceGroup, HashSet<ServiceType> services, string name,
        ISource source, List<StyleGroup> styleGroups,
        Envelope envelope = null, bool enabled = true)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name), "图层名称不能为空");
        }

        Name = name;
        Enabled = enabled;
        ResourceGroup = resourceGroup;
        ResourceId = resourceGroup?.Id ?? Name;
        Services = services;
        Source = source ?? throw new ArgumentNullException(nameof(source), "图层数据源不能为空");
        // 若用户未指定显示范围， 则使用数据源的范围
        // 注意： 用户可能会配错坐标系， 图层的坐标系和数据源的坐标系必须一致
        Envelope = envelope ?? source.Envelope;
        StyleGroups = styleGroups ?? [];
    }

    public void SetStyle(StyleGroup styleGroup)
    {
        StyleGroups = [styleGroup];
    }

    public Task LoadAsync()
    {
        return Source.LoadAsync();
    }

    public async Task RenderAsync(IGraphicsService graphicsService, Viewport viewport, int viewportSrid, Zoom zoom)
    {
        // 图层未启用或者不在显示级别
        if (!Enabled || !this.IsVisible(zoom))
        {
            return;
        }

        // 数据源为空: 在构造中检测了
        // 没有样式: 在构造中检测了
        // 所有的样式都不可显示，则不需要渲染，提前退出，避免数据查询开销
        if (StyleGroups.Count > 0 && !StyleGroups.Any(x => x.IsVisible(zoom)))
        {
            return;
        }

        // 由前端的 SRID 的 extent 转换成数据源的 extent
        // 先转换成数据源的 SRID 数据， 才能做相交判断是否超出数据范围
        var viewportCoordinateSystem = CoordinateReferenceSystem.Get(viewportSrid);
        if (viewportCoordinateSystem == null)
        {
            throw new ArgumentException($"不支持的坐标系: {viewportSrid}");
        }

        var sourceCoordinateSystem = CoordinateSystem ?? CoordinateReferenceSystem.Get(Srid);
        if (sourceCoordinateSystem == null)
        {
            throw new ArgumentException($"不支持的坐标系: {Srid}");
        }

        var viewportExtent = viewport.Extent;
        // 将视窗的范围转换成数据源的范围
        var dataSourceExtent = viewportExtent.Transform(viewportCoordinateSystem, sourceCoordinateSystem);

        // 不在当前图层的范围内，不需要渲染
        if (Envelope != null && !Envelope.Intersects(dataSourceExtent))
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

        var environments = new Dictionary<string, dynamic>
        {
            { Defaults.WmsScaleKey, zoom.Value }
        };

        if (Source is IRemoteHttpSource remoteHttpSource)
        {
            remoteHttpSource.HttpClientFactory = HttpClientFactory;
        }

        switch (Source)
        {
            case IVectorSource vectorSource:
            {
                await RenderAsync(graphicsService, vectorSource, viewportExtent, viewportSrid, zoom,
                    viewport.Width, viewport.Height, dataSourceExtent, environments);
                break;
            }
            case ITiledSource tiledSource:
            {
                await RenderAsync(graphicsService, tiledSource, viewportExtent, viewportSrid, zoom, dataSourceExtent);
                break;
            }
            case IRasterSource rasterSource:
            {
                await RenderAsync(graphicsService, rasterSource, zoom, dataSourceExtent);
                break;
            }
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
            ResourceId = ResourceId,
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

    public override string ToString()
    {
        return ResourceGroup == null ? Name : $"{ResourceGroup.Name}:{Name}";
    }
}