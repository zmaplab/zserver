using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NetTopologySuite.Geometries;
using ZMap.Infrastructure;
using ZMap.TileGrid;

namespace ZServer.Store.Configuration
{
    public class GridSetStore : IGridSetStore
    {
        private static readonly ConcurrentDictionary<string, GridSet> Cache = new();

        public Task Refresh(IConfiguration configuration)
        {
            var gridSetsSections = configuration.GetSection("gridSets");
            foreach (var section in gridSetsSections.GetChildren())
            {
                var name = section.Key;
                var srid = section.GetSection("srid").Get<int>();
                var crs = CoordinateReferenceSystem.Get(srid);
                if (crs == null)
                {
                    throw new Exception("SRID is not exists or supported");
                }

                var levels = section.GetSection("levels").Get<int?>();
                var extentNumbers = section.GetSection("extent").Get<double[]>();
                if (extentNumbers is not { Length: 4 })
                {
                    throw new ArgumentException("GridSet extent should be 4 numbers");
                }

                var extent = new Envelope(extentNumbers[0], extentNumbers[1], extentNumbers[2],
                    extentNumbers[3]);
                var alignTopLeft = section.GetSection("alignTopLeft").Get<bool>();
                var metersPerUnit = section.GetSection("metersPerUnit").Get<double?>();
                var pixelSize = section.GetSection("pixelSize").Get<double?>();
                var tileSize = section.GetSection("tileSize").Get<int[]>();

                int tileWidth;
                int tileHeight;
                if (tileSize == null)
                {
                    tileWidth = 256;
                    tileHeight = 256;
                }
                else if (tileSize.Length == 1)
                {
                    if (tileSize[0] <= 0)
                    {
                        throw new ArgumentException("Tile width/height in grid set should large than 0");
                    }

                    tileWidth = tileSize[0];
                    tileHeight = tileWidth;
                }
                else
                {
                    if (tileSize[0] <= 0)
                    {
                        throw new ArgumentException("Tile width in grid set should large than 0");
                    }

                    if (tileSize[1] <= 0)
                    {
                        throw new ArgumentException("Tile height in grid set should large than 0");
                    }

                    tileWidth = tileSize[0];
                    tileHeight = tileSize[1];
                }

                var yCoordinateFirst = section.GetSection("yCoordinateFirst").Get<bool>();

                GridSet g;
                if (levels == null)
                {
                    var resolutions = section.GetSection("resolutions").Get<double[]>();
                    var scaleDenominators = section.GetSection("scaleDenominators").Get<double[]>();
                    g = GridSetFactory.CreateGridSet(
                        name,
                        srid,
                        extent,
                        alignTopLeft,
                        resolutions == null || resolutions.Length == 0 ? null : resolutions,
                        scaleDenominators == null || scaleDenominators.Length == 0
                            ? null
                            : scaleDenominators,
                        metersPerUnit,
                        pixelSize ?? DefaultGridSets.DefaultPixelSizeMeter,
                        null,
                        tileWidth,
                        tileHeight,
                        yCoordinateFirst);
                }
                else
                {
                    g = GridSetFactory.CreateGridSet(
                        name,
                        srid,
                        extent,
                        alignTopLeft,
                        levels.Value,
                        metersPerUnit,
                        pixelSize ?? DefaultGridSets.DefaultPixelSizeMeter,
                        tileWidth,
                        tileHeight,
                        yCoordinateFirst);
                }

                Cache.AddOrUpdate(name, g, (_, _) => g);
            }
            return Task.CompletedTask;
        }

        public Task<GridSet> FindAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var gridSet = Cache.GetValueOrDefault(name) ?? DefaultGridSets.TryGet(name);
            return Task.FromResult(gridSet);
        }
    }
}