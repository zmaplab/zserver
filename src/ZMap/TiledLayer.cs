namespace ZMap;

public partial class Layer
{
    private async Task RenderAsync(IGraphicsService graphicsService,
        ITiledSource tiledSource, Envelope viewportExtent, int viewportSrid,
        Zoom zoom, Envelope dataSourceExtent)
    {
        var nearestGrid = tiledSource.GridSet.GetNearestLevel(zoom.Value);
        var gridArea = tiledSource.GridSet.GetGridArea(dataSourceExtent, nearestGrid);
        if (gridArea.IsEmpty)
        {
            return;
        }

        var level = gridArea.Level;
        for (var x = gridArea.MinX; x <= gridArea.MaxX; x++)
        {
            if (x < 0)
            {
                continue;
            }

            for (var y = gridArea.MinY; y <= gridArea.MaxY; y++)
            {
                if (y < 0)
                {
                    continue;
                }

                byte[] image = null;
                var tileEnvelope = tiledSource.GridSet.GetEnvelope(level, x, y);
                var area = tileEnvelope.Extent.Transform(tiledSource.CoordinateSystem,
                    CoordinateReferenceSystem.Get(viewportSrid));
                if (StyleGroups is { Count: > 0 })
                {
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

                            image ??= await tiledSource.GetImageAsync(level, x, y);
                            graphicsService.Render(viewportExtent, area, image, rasterStyle);
                        }
                    }
                }
                else
                {
                    image = await tiledSource.GetImageAsync(level, x, y);

                    // await File.WriteAllBytesAsync($"images/t_{level}_{y}_{x}.png", image);
                    // Logger.LogInformation("Combine render image: {level}, {y}, {x}", level, y, x);
                    graphicsService.Render(viewportExtent, area, image, new RasterStyle());
                }
            }
        }
    }
}