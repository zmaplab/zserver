using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZMap;
using ZMap.Source.ShapeFile;
using ZMap.TileGrid;

namespace ZServer
{
    public class PreloadService : IHostedService
    {
        private readonly ILogger<PreloadService> _logger;

        public PreloadService(ILogger<PreloadService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                if (!string.Equals(Environment.GetEnvironmentVariable("PREHEAT"), "false",
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    await LoadAllShapes();
                }

                _logger.LogInformation("Preload completed");
            }, cancellationToken);
        }

        private static async Task LoadAllShapes()
        {
            await LoadDirectory("shapes");
        }

        private static async Task LoadDirectory(string directory)
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

        private static async Task LoadShape(string path)
        {
            Log.Logger.LogInformation($"Start loading {path}");
            var shapeFileSource = new ShapeFileSource(path, false);
            var features = (await shapeFileSource.GetFeaturesInExtentAsync(DefaultGridSets.World4326)).ToList();
            Log.Logger.LogInformation($"Load {path} success: {features.Count}");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}