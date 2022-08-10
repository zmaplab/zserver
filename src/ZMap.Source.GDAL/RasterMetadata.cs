using System;
using NetTopologySuite.Index.Strtree;
using OSGeo.GDAL;

namespace ZMap.Source.GDAL
{
    public class RasterMetadata : IDisposable
    {
        public Dataset Dataset { get; set; }
        public string Projection { get; set; }
        public int SRID { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public STRtree<string> Index { get; set; }
        public NetTopologySuite.Geometries.Envelope Extent { get; set; }
        public int Bands { get; set; }

        internal GeoTransform Transform { get; set; }

        public void Dispose()
        {
            Dataset?.Dispose();
            Index = null;
            Extent = null;
            Transform = null;
        }
    }
}