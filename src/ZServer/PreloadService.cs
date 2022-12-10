using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZMap.Source.ShapeFile;
using ZMap.TileGrid;

namespace ZServer
{
    public class PreloadService : IHostedService
    {
        private readonly ILogger<PreloadService> _logger;
        private readonly IConfiguration _configuration;

        public PreloadService(ILogger<PreloadService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                if (!string.Equals(Environment.GetEnvironmentVariable("PREHEAT"), "false",
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    LoadAllShapes().Wait(cancellationToken);
                    _logger.LogInformation("Preload completed");
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
                _logger.LogInformation($"目录不存在: {directory}");
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
            _logger.LogInformation($"Start loading {path}");
            var shapeFileSource = new ShapeFileSource(path);

            // var fullExtent = DefaultGridSets.World4326.Transform(4326, shapeFileSource.SRID);
            var features = (await shapeFileSource.GetFeaturesInExtentAsync(DefaultGridSets.World4326)).ToList();
            _logger.LogInformation($"Load {path} success");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}