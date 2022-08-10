using System;
using System.IO;
using Xunit;
using ZMap.Indexing;
using ZMap.Source.ShapeFile;

namespace ZServer.Tests
{
    public class SpatialIndexTests
    {
        [Fact]
        public void Save()
        {
            var shpPath = Path.Combine(AppContext.BaseDirectory, "shapes/osmbuildings.shp");
          
            Delete(shpPath, ".shx");
            Delete(shpPath, ".sidx");
            new SpatialIndexFactory().TryCreate(shpPath, SpatialIndexType.STRTree);
            // var shp = new ShapeFileSource(shpPath);
        }

        private void Delete(string path, string extension)
        {
            var shxPath = Path.ChangeExtension(path, extension);
            if (File.Exists(shxPath))
            {
                File.Delete(shxPath);
            }
        }
    }
}