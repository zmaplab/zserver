using System;
using System.IO;
using NetTopologySuite.Geometries;
using Xunit;
using ZMap.Indexing;
using ZMap.Source.ShapeFile;

namespace ZServer.Tests
{
    public class SpatialIndexTests
    {
        [Fact]
        public void SaveAndReload()
        {
            var shpPath = Path.Combine(AppContext.BaseDirectory, "shapes/osmbuildings.shp");

            Delete(shpPath, ".shx");
            Delete(shpPath, ".sidx");
            SpatialIndexFactory.Create(shpPath, new BinaryReader(File.OpenRead(shpPath)), SpatialIndexType.STRTree);

            var index = SpatialIndexFactory.Load(Path.ChangeExtension(shpPath, ".sidx"), SpatialIndexType.STRTree);
            var list = index.Query(new Envelope(-180, 180, -90, 90));
            Assert.Equal(1056, list.Count);
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