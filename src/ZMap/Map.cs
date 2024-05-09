namespace ZMap;

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

    public Map AddLayers(IEnumerable<Layer> layers)
    {
        foreach (var layer in layers)
        {
            _layers.Add(layer);
        }

        return this;
    }

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

        for (var i = _layers.Count - 1; i >= 0; --i)
        {
            var layer = _layers[i];
            await layer.RenderAsync(graphicsService, viewport, _zoom, _srid);
        }

        return graphicsService.GetImage(imageFormat, viewport.Bordered);
    }

    public void Dispose()
    {
    }
}