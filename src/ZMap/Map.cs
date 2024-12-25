namespace ZMap;

/// <summary>
/// 每个地图实例需要设置
/// GridSet
/// 1. 渲染是当前地图的级别、GridSet通过级别、行列号获取对应的区域
/// 如果图层数据是矢量数据，则可以直接获取对应区域的矢量后进行样式渲染
/// 如果图层是栅格、瓦片数据，则需要根据区域重新算出相交的瓦片、然后获取瓦片数据进行渲染
/// </summary>
public class Map : IDisposable
{
    private static readonly ILogger Logger = Log.CreateLogger<Map>();
    private readonly List<Layer> _layers = new();
    private IGraphicsServiceProvider _graphicsServiceProvider;
    private Zoom _zoom;
    private int _srid;
    public IReadOnlyCollection<Layer> Layers => _layers;
    public string Id { get; private set; }

    public Map SetZoom(Zoom zoom)
    {
        _zoom = zoom;
        return this;
    }

    public Map SetSrid(int srid)
    {
        _srid = srid;
        return this;
    }

    public Map SetId(string id)
    {
        Id = id;
        return this;
    }

    public Map SetGraphicsContextFactory(IGraphicsServiceProvider graphicsServiceProvider)
    {
        _graphicsServiceProvider = graphicsServiceProvider;
        return this;
    }

    public async Task<Map> AddLayers(IEnumerable<Layer> layers)
    {
        foreach (var layer in layers)
        {
            await layer.LoadAsync();
            _layers.Add(layer);
        }

        return this;
    }

    /// <summary>
    /// 获取指定区域的图片
    /// </summary>
    /// <param name="viewport"></param>
    /// <param name="imageFormat"></param>
    /// <returns></returns>
    public async Task<Stream> GetImageAsync(Viewport viewport, string imageFormat)
    {
        if (viewport.Extent == null || viewport.Extent.IsNull)
        {
            return Stream.Null;
        }

        using var graphicsService =
            _graphicsServiceProvider.Create(viewport.Width, viewport.Height);
        if (graphicsService == null)
        {
            Logger.LogWarning("创建图形服务失败");
            return Stream.Null;
        }

        graphicsService.TraceIdentifier = Id;

        // 图层倒序渲染， 才不会被覆盖
        for (var i = _layers.Count - 1; i >= 0; --i)
        {
            var layer = _layers[i];
            await layer.RenderAsync(graphicsService, viewport, _srid, _zoom);
        }

        return graphicsService.GetImage(imageFormat, viewport.Bordered);
    }

    public void Dispose()
    {
    }
}