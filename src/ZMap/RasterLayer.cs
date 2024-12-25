namespace ZMap;

public partial class Layer
{
    private async Task RenderAsync(IGraphicsService service,
        IRasterSource rasterSource,
        Zoom zoom, Envelope dataSourceExtent)
    {
        var image = await rasterSource.GetImageAsync(dataSourceExtent);

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

                service.Render(dataSourceExtent, dataSourceExtent, image, rasterStyle);
            }
        }
    }
}