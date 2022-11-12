using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace ZMap.TileGrid
{
    public class GridSet
    {
        const int DECIMALS = 5;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public int SRID { get; set; }

        /// <summary>
        /// 瓦片像素宽度
        /// </summary>
        public int TileWidth { get; set; }

        /// <summary>
        /// 瓦片像素高度
        /// </summary>
        public int TileHeight { get; set; }

        /// <summary>
        /// Whether the y-coordinate tileOrigin is at the top (true) or at the bottom
        /// </summary>
        public bool YBaseToggle { get; set; }

        /// <summary>
        /// By default the coordinates are {x,y}, this flag reverses the output for WMTS getcapabilities
        /// </summary>
        public bool YCoordinateFirst { get; set; }

        public bool ScaleWarning { get; set; }

        public double MetersPerUnit { get; set; }

        public double PixelSize { get; set; }

        public Envelope Extent { get; set; }

        public Dictionary<string, Grid> GridLevels { get; set; }

        /// <summary>
        /// true if the resolutions are preserved and the scaleDenominators calculated
        /// false if the resolutions are calculated based on the scale denominators.
        /// </summary>
        public bool ResolutionsPreserved { get; set; }

        public (Envelope Extent, double ScaleDenominator) GetEnvelope(string matrixSet, long tileX, long tileY)
        {
            if (!GridLevels.ContainsKey(matrixSet))
            {
                return default;
            }

            var grid = GridLevels[matrixSet];

            var tileLength = grid.Resolution * TileWidth;
            var tileHeight = grid.Resolution * TileHeight;

            var minX = Extent.MinX + tileX * tileLength;
            var maxX = minX + tileLength;
            double minY, maxY;
            if (YBaseToggle)
            {
                minY = Extent.MinY + tileY * tileHeight;
                maxY = minY + tileHeight;
            }
            else
            {
                maxY = Extent.MaxY - tileY * tileHeight;
                minY = maxY - tileHeight;
            }

            return (new Envelope(minX, maxX, minY, maxY), grid.ScaleDenominator);
        }

        public Dictionary<int, GridArea> GetGridAreas(Envelope envelope, int minLevel, int maxLevel)
        {
            var result = new Dictionary<int, GridArea>();
            for (var i = minLevel; i <= maxLevel; i++)
            {
                var p1 = GetTileCoordForXyAndZ(envelope.MaxX, envelope.MaxY, i);
                var p2 = GetTileCoordForXyAndZ(envelope.MinX, envelope.MinY, i);
                var range = new GridArea
                {
                    Level = i,
                    MinX = p2.X,
                    MaxX = p1.X,
                    MinY = p1.Y,
                    MaxY = p2.Y
                };

                result.Add(i, range);
            }

            return result;
        }

        public (int Z, int X, int Y) GetTileCoordForXyAndZ(double x, double y, int z,
            bool reverseIntersectionPolicy = false)
        {
            var origin = YBaseToggle
                ? new[] { Extent.MinX, Extent.MinY }
                : new[] { Extent.MinX, Extent.MaxY };

            var grid = GridLevels[z.ToString()];
            var resolution = grid.Resolution;
            var tileSize = new[] { TileWidth, TileHeight };

            var tileCoordX = (x - origin[0]) / resolution / tileSize[0];
            var tileCoordY = (origin[1] - y) / resolution / tileSize[1];

            if (reverseIntersectionPolicy)
            {
                tileCoordX = Ceil(tileCoordX, DECIMALS) - 1;
                tileCoordY = Ceil(tileCoordY, DECIMALS) - 1;
            }
            else
            {
                tileCoordX = Floor(tileCoordX, DECIMALS);
                tileCoordY = Floor(tileCoordY, DECIMALS);
            }

            return (z, (int)tileCoordX, (int)tileCoordY);
        }

        private double Ceil(double n, int decimals)
        {
            return Math.Ceiling(Math.Round(n, decimals));
        }

        private double Floor(double n, int decimals)
        {
            return Math.Floor(Math.Round(n, decimals));
        }

        private (int x, int y) CalcXy(double lng, double lat, int level)
        {
            var x = (lng + 180) / 360;
            var titleX = (int)Math.Floor(x * Math.Pow(2, level));
            var latRad = lat * Math.PI / 180;
            var y = (1 - Math.Log(Math.Truncate(latRad) + 1 / Math.Cos(latRad)) / Math.PI) / 2;
            var titleY = (int)Math.Floor(y * Math.Pow(2, level));
            return (titleX, titleY);
        }
    }
}