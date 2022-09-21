using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using ZMap.TileGrid;
using ZMap.Utilities;

namespace ZServer.Store.Configuration
{
    public class GridSetStore : IGridSetStore
    {
        private readonly IConfiguration _configuration;
        private readonly ServerOptions _options;

        public GridSetStore(IConfiguration configuration,
            IOptionsMonitor<ServerOptions> options)
        {
            _configuration = configuration;
            _options = options.CurrentValue;
        }

        public Task<GridSet> FindAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var gridSet = DefaultGridSets.TryGet(name) ??
                          Cache.GetOrCreate($"{GetType().FullName}:{name}", entry =>
                          {
                              var section = _configuration.GetSection($"gridSets:{name}");
                              var srid = section.GetSection("srid").Get<int>();
                              var crs = CoordinateSystemUtilities.Get(srid);
                              if (crs == null)
                              {
                                  throw new Exception($"SRID is not exists or supported");
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
                                      pixelSize ?? DefaultGridSets.DEFAULT_PIXEL_SIZE_METER,
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
                                      pixelSize ?? DefaultGridSets.DEFAULT_PIXEL_SIZE_METER,
                                      tileWidth,
                                      tileHeight,
                                      yCoordinateFirst);
                              }

                              entry.SetValue(g);
                              entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(_options.ConfigurationCacheTtl));
                              return g;
                          });

            return Task.FromResult(gridSet);
        }
    }
}