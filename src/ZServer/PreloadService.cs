using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZMap.Source.ShapeFile;
using ZMap.TileGrid;

namespace ZServer;

public class PreloadService(ILogger<PreloadService> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            if (string.Equals(Environment.GetEnvironmentVariable("PREHEAT"), "true",
                    StringComparison.InvariantCultureIgnoreCase)
                ||
                string.Equals(Environment.GetEnvironmentVariable("PRE_LOAD_SHP"), "true",
                    StringComparison.InvariantCultureIgnoreCase))
            {
                await LoadAllShapes();
                logger.LogInformation("预加载矢量文件成功");
            }
        }, cancellationToken);
    }

    private async Task LoadAllShapes()
    {
        await LoadDirectory(Path.Combine(AppContext.BaseDirectory, "shapes"));
    }

    private async Task LoadDirectory(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return;
        }

        var directories = Directory.GetDirectories(directory);
        if (directories.Length > 0)
        {
            foreach (var directory1 in directories)
            {
                await LoadDirectory(directory1);
            }
        }

        var files = Directory.GetFiles(directory).Where(x => x.EndsWith(".shp"));
        foreach (var file in files)
        {
            await LoadShape(file);
        }
    }

    private async Task LoadShape(string path)
    {
        logger.LogInformation("开始加载矢量文件: {Path}", path);
        var shapeFileSource = new ShapeFileSource(path);

        // var fullExtent = DefaultGridSets.World4326.Transform(4326, shapeFileSource.SRID);
        // ReSharper disable once UnusedVariable
        foreach (var feature in await shapeFileSource.GetFeaturesInExtentAsync(DefaultGridSets.World4326))
        {
        }

        logger.LogInformation("加载矢量文件 {Path} 成功", path);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}