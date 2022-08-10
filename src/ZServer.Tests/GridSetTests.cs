using System.Threading.Tasks;
using Xunit;
using ZMap.TileGrid;
using ZServer.Store;

namespace ZServer.Tests
{
    public class GridSetTests : BaseTests
    {
        [Fact]
        public void GetEnvelope()
        {
            // 117.38953125,31.916933593750002,117.41150390625,31.938906250000002
            var gridSet = DefaultGridSets.TryGet("EPSG:4326");

            var tuple = gridSet.GetEnvelope("14", 27058, 5295);
            var e0 = tuple.Extent;
            Assert.Equal(117.26806640625, e0.MinX);
            Assert.Equal(31.81640625, e0.MinY);
            Assert.Equal(117.279052734375, e0.MaxX);
            Assert.Equal(31.827392578125, e0.MaxY);
        }

        [Fact]
        public void GetTileCoordForXyAndZ()
        {
            // 117.38953125,31.916933593750002,117.41150390625,31.938906250000002
            var gridSet = DefaultGridSets.TryGet("EPSG:4326");
            var xyz = gridSet.GetTileCoordForXyAndZ(120.20861471424968, 27.75598560808519, 11);
            Assert.Equal(3415, xyz.X);
            Assert.Equal(708, xyz.Y);
        }

        [Fact]
        public async Task GetDefaultGridSet()
        {
            var store = GetScopedService<IGridSetStore>();
            var gridSet4326 = await store.FindAsync("EPSG:4326");


            var tuple = gridSet4326.GetEnvelope("14", 27058, 5295);
            var e0 = tuple.Extent;
            Assert.Equal(117.26806640625, e0.MinX);
            Assert.Equal(31.81640625, e0.MinY);
            Assert.Equal(117.279052734375, e0.MaxX);
            Assert.Equal(31.827392578125, e0.MaxY);

            var gridSet43857 = await store.FindAsync("EPSG:3857");

            tuple = gridSet43857.GetEnvelope("14", 27058, 5295);
            e0 = tuple.Extent;
            Assert.Equal(46145951.213676751, e0.MinX);
            Assert.Equal(7083572.2842578124, e0.MinY);
            Assert.Equal(46148397.198581539, e0.MaxX);
            Assert.Equal(7086018.2691625971, e0.MaxY);
        }

        [Fact]
        public async Task GetStoreGridSet()
        {
            var store = GetScopedService<IGridSetStore>();

            var gridSet4326 = await store.FindAsync("epsg4326test");

            var tuple = gridSet4326.GetEnvelope("14", 27058, 5295);
            var e0 = tuple.Extent;
            Assert.Equal(117.26806640625, e0.MinX);
            Assert.Equal(31.81640625, e0.MinY);
            Assert.Equal(117.279052734375, e0.MaxX);
            Assert.Equal(31.827392578125, e0.MaxY);

            var gridSet43857 = await store.FindAsync("epsg3857test");

            tuple = gridSet43857.GetEnvelope("14", 27058, 5295);
            e0 = tuple.Extent;
            Assert.Equal(46145951.213676751, e0.MinX);
            Assert.Equal(7083572.2842578124, e0.MinY);
            Assert.Equal(46148397.198581539, e0.MaxX);
            Assert.Equal(7086018.2691625971, e0.MaxY);
        }
    }
}