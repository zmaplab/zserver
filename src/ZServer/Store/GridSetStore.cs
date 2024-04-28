using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NetTopologySuite.Geometries;
using Newtonsoft.Json.Linq;
using ZMap.Infrastructure;
using ZMap.Store;
using ZMap.TileGrid;

namespace ZServer.Store;

public class GridSetStore : IGridSetStore
{
    private static readonly ConcurrentDictionary<string, GridSet> Cache = new();

    public Task Refresh(List<JObject> configurations)
    {
        var existKeys = Cache.Keys.ToList();
        var keys = new List<string>();
        foreach (var configuration in configurations)
        {
            var sections = configuration.SelectToken("gridSets");
            if (sections == null)
            {
                continue;
            }

            foreach (var section in sections.Children<JProperty>())
            {
                var name = section.Name;

                var entity = section.Value.ToObject<GridSetEntity>();

                var crs = CoordinateReferenceSystem.Get(entity.SRID);
                if (crs == null)
                {
                    throw new Exception("SRID is not exists or supported");
                }

                if (entity.Extent.Length != 4)
                {
                    throw new ArgumentException("GridSet extent should be 4 numbers");
                }

                var extent = new Envelope(entity.Extent[0], entity.Extent[1], entity.Extent[2],
                    entity.Extent[3]);

                int tileWidth;
                int tileHeight;
                if (entity.TileSize == null)
                {
                    tileWidth = 256;
                    tileHeight = 256;
                }
                else if (entity.TileSize.Length == 1)
                {
                    if (entity.TileSize[0] <= 0)
                    {
                        throw new ArgumentException("Tile width/height in grid set should large than 0");
                    }

                    tileWidth = entity.TileSize[0];
                    tileHeight = tileWidth;
                }
                else
                {
                    if (entity.TileSize[0] <= 0)
                    {
                        throw new ArgumentException("Tile width in grid set should large than 0");
                    }

                    if (entity.TileSize[1] <= 0)
                    {
                        throw new ArgumentException("Tile height in grid set should large than 0");
                    }

                    tileWidth = entity.TileSize[0];
                    tileHeight = entity.TileSize[1];
                }

                GridSet g;
                if (entity.Levels == null)
                {
                    g = GridSetFactory.CreateGridSet(
                        name,
                        entity.SRID,
                        extent,
                        entity.AlignTopLeft,
                        entity.Resolutions == null || entity.Resolutions.Length == 0 ? null : entity.Resolutions,
                        entity.ScaleDenominators == null || entity.ScaleDenominators.Length == 0
                            ? null
                            : entity.ScaleDenominators,
                        entity.MetersPerUnit,
                        entity.PixelSize ?? DefaultGridSets.DefaultPixelSizeMeter,
                        null,
                        tileWidth,
                        tileHeight,
                        entity.YCoordinateFirst);
                }
                else
                {
                    g = GridSetFactory.CreateGridSet(
                        name,
                        entity.SRID,
                        extent,
                        entity.AlignTopLeft,
                        entity.Levels.Value,
                        entity.MetersPerUnit,
                        entity.PixelSize ?? DefaultGridSets.DefaultPixelSizeMeter,
                        tileWidth,
                        tileHeight,
                        entity.YCoordinateFirst);
                }

                keys.Add(name);
                Cache.AddOrUpdate(name, g, (_, _) => g);
            }
        }

        var removedKeys = existKeys.Except(keys);
        foreach (var removedKey in removedKeys)
        {
            Cache.TryRemove(removedKey, out _);
        }

        return Task.CompletedTask;
    }

    // public Task Refresh(IEnumerable<IConfiguration> configurations)
    // {
    //     var existKeys = Cache.Keys.ToList();
    //     var keys = new List<string>();
    //
    //     foreach (var configuration in configurations)
    //     {
    //         var gridSetsSections = configuration.GetSection("gridSets");
    //         foreach (var section in gridSetsSections.GetChildren())
    //         {
    //             var name = section.Key;
    //             var srid = section.GetSection("srid").Get<int>();
    //             var crs = CoordinateReferenceSystem.Get(srid);
    //             if (crs == null)
    //             {
    //                 throw new Exception("SRID is not exists or supported");
    //             }
    //
    //             var levels = section.GetSection("levels").Get<int?>();
    //             var extentNumbers = section.GetSection("extent").Get<double[]>();
    //             if (extentNumbers is not { Length: 4 })
    //             {
    //                 throw new ArgumentException("GridSet extent should be 4 numbers");
    //             }
    //
    //             var extent = new Envelope(extentNumbers[0], extentNumbers[1], extentNumbers[2],
    //                 extentNumbers[3]);
    //             var alignTopLeft = section.GetSection("alignTopLeft").Get<bool>();
    //             var metersPerUnit = section.GetSection("metersPerUnit").Get<double?>();
    //             var pixelSize = section.GetSection("pixelSize").Get<double?>();
    //             var tileSize = section.GetSection("tileSize").Get<int[]>();
    //
    //             int tileWidth;
    //             int tileHeight;
    //             if (tileSize == null)
    //             {
    //                 tileWidth = 256;
    //                 tileHeight = 256;
    //             }
    //             else if (tileSize.Length == 1)
    //             {
    //                 if (tileSize[0] <= 0)
    //                 {
    //                     throw new ArgumentException("Tile width/height in grid set should large than 0");
    //                 }
    //
    //                 tileWidth = tileSize[0];
    //                 tileHeight = tileWidth;
    //             }
    //             else
    //             {
    //                 if (tileSize[0] <= 0)
    //                 {
    //                     throw new ArgumentException("Tile width in grid set should large than 0");
    //                 }
    //
    //                 if (tileSize[1] <= 0)
    //                 {
    //                     throw new ArgumentException("Tile height in grid set should large than 0");
    //                 }
    //
    //                 tileWidth = tileSize[0];
    //                 tileHeight = tileSize[1];
    //             }
    //
    //             var yCoordinateFirst = section.GetSection("yCoordinateFirst").Get<bool>();
    //
    //             GridSet g;
    //             if (levels == null)
    //             {
    //                 var resolutions = section.GetSection("resolutions").Get<double[]>();
    //                 var scaleDenominators = section.GetSection("scaleDenominators").Get<double[]>();
    //                 g = GridSetFactory.CreateGridSet(
    //                     name,
    //                     srid,
    //                     extent,
    //                     alignTopLeft,
    //                     resolutions == null || resolutions.Length == 0 ? null : resolutions,
    //                     scaleDenominators == null || scaleDenominators.Length == 0
    //                         ? null
    //                         : scaleDenominators,
    //                     metersPerUnit,
    //                     pixelSize ?? DefaultGridSets.DefaultPixelSizeMeter,
    //                     null,
    //                     tileWidth,
    //                     tileHeight,
    //                     yCoordinateFirst);
    //             }
    //             else
    //             {
    //                 g = GridSetFactory.CreateGridSet(
    //                     name,
    //                     srid,
    //                     extent,
    //                     alignTopLeft,
    //                     levels.Value,
    //                     metersPerUnit,
    //                     pixelSize ?? DefaultGridSets.DefaultPixelSizeMeter,
    //                     tileWidth,
    //                     tileHeight,
    //                     yCoordinateFirst);
    //             }
    //
    //             keys.Add(name);
    //             Cache.AddOrUpdate(name, g, (_, _) => g);
    //         }
    //     }
    //
    //     var removedKeys = existKeys.Except(keys);
    //     foreach (var removedKey in removedKeys)
    //     {
    //         Cache.TryRemove(removedKey, out _);
    //     }
    //
    //     return Task.CompletedTask;
    // }

    public Task<GridSet> FindAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var gridSet = Cache.GetValueOrDefault(name) ?? DefaultGridSets.TryGet(name);
        return Task.FromResult(gridSet);
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    private class GridSetEntity
    {
        public int SRID { get; set; }
        public int[] Extent { get; set; }
        public bool AlignTopLeft { get; set; }
        public int? Levels { get; set; }
        public double? PixelSize { get; set; }
        public double? MetersPerUnit { get; set; }
        public int[] TileSize { get; set; }
        public bool YCoordinateFirst { get; set; }
        public double[] Resolutions { get; set; }
        public double[] ScaleDenominators { get; set; }
    }
}