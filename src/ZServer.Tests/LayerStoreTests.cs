using System.Threading.Tasks;
using ZServer.Store;
using Xunit;
using ZMap.Source;
using ZMap.Style;
using ZMap.Source.ShapeFile;
using ZServer.Entity;
using System.Linq;
using ZMap;

namespace ZServer.Tests
{
    public class LayerStoreTests : BaseTests
    {
        [Fact]
        public async Task GetPgLayer()
        {
            var store = GetScopedService<ILayerStore>();
            var layer = await store.FindAsync("resourceGroup1", "berlin_db");
            Assert.NotNull(layer);
            Assert.Equal("berlin_db", layer.Name);
            Assert.NotNull(layer.Envelope);
            Assert.Equal(112.8, layer.Envelope.MinX);
            Assert.Equal(120.1, layer.Envelope.MaxX);
            Assert.Equal(21.4, layer.Envelope.MinY);
            Assert.Equal(23.2, layer.Envelope.MaxY);

            Assert.Single(layer.Buffers);
            Assert.Equal(32, layer.Buffers[0].Size);

            Assert.NotNull(layer.Source);
            Assert.True(layer.Source is SpatialDatabaseSource);
            var source = (SpatialDatabaseSource)layer.Source;
            Assert.Equal("geom", source.Geometry);
            Assert.Equal("osmbuildings", source.Table);
            Assert.Equal("berlin_db", source.Name);
            Assert.Equal("User ID=postgres;Password=1qazZAQ!;Host=localhost;Port=5432;Database=berlin;Pooling=true;",
                source.ConnectionString);
            Assert.Equal(4326, source.Srid);

            Assert.NotNull(layer.StyleGroups);
            Assert.Single(layer.StyleGroups);
            Assert.Equal("style1", layer.StyleGroups.ElementAt(0).Name);
            Assert.Equal(500, layer.StyleGroups.ElementAt(0).MinZoom);
            Assert.Equal(128000, layer.StyleGroups.ElementAt(0).MaxZoom);
            Assert.Equal(ZoomUnits.Scale, layer.StyleGroups.ElementAt(0).ZoomUnit);

            Assert.Equal(3, layer.StyleGroups.ElementAt(0).Styles.Count);
            Assert.True(layer.StyleGroups.ElementAt(0).Styles.ElementAt(0) is FillStyle);
            Assert.True(layer.StyleGroups.ElementAt(0).Styles[1] is LineStyle);
            Assert.True(layer.StyleGroups.ElementAt(0).Styles[2] is TextStyle);
            var fillStyle = (FillStyle)layer.StyleGroups.ElementAt(0).Styles.ElementAt(0);
            var lineStyle = (LineStyle)layer.StyleGroups.ElementAt(0).Styles[1];
            var textStyle = (TextStyle)layer.StyleGroups.ElementAt(0).Styles[2];

            Assert.True(fillStyle.Antialias);
            Assert.Equal(1, fillStyle.Opacity.Value);
            Assert.Equal("#3ed53e", fillStyle.Color.Value);

            Assert.Equal(1, lineStyle.Opacity.Value);
            Assert.Equal("#DC143C", lineStyle.Color.Value);

            Assert.Equal("city", textStyle.Label.Value);
            Assert.Equal("#008000", textStyle.Color.Value);
            Assert.Equal(2, textStyle.Font.Value.Count);
            Assert.Equal("Open Sans Regular", textStyle.Font.Value.ElementAt(0));
            Assert.Equal("Arial Unicode MS Regular", textStyle.Font.Value[1]);
            Assert.Equal(16, textStyle.Size.Value);
        }

        [Fact]
        public async Task GetShapeFileLayer()
        {
            var store = GetScopedService<ILayerStore>();
            var layer = await store.FindAsync(null, "berlin_shp") as LayerEntity;
            Assert.NotNull(layer);
            Assert.Equal("berlin_shp", layer.Name);

            Assert.Equal(2, layer.Services.Count);
            Assert.Equal(ServiceType.WMS, layer.Services.ElementAt(0));
            Assert.Equal(ServiceType.WMTS, layer.Services.ElementAt(1));

            Assert.NotNull(layer.Source);
            Assert.True(layer.Source is ShapeFileSource);
            var source = (ShapeFileSource)layer.Source;
            Assert.Equal(4326, source.Srid);
            Assert.EndsWith("osmbuildings.shp", source.File);

            Assert.NotNull(layer.StyleGroups);
            Assert.Single(layer.StyleGroups);
            Assert.Equal("style1", layer.StyleGroups.ElementAt(0).Name);
            Assert.Equal(500, layer.StyleGroups.ElementAt(0).MinZoom);
            Assert.Equal(128000, layer.StyleGroups.ElementAt(0).MaxZoom);
            Assert.Equal(ZoomUnits.Scale, layer.StyleGroups.ElementAt(0).ZoomUnit);

            Assert.Equal(3, layer.StyleGroups.ElementAt(0).Styles.Count);
            Assert.True(layer.StyleGroups.ElementAt(0).Styles.ElementAt(0) is FillStyle);
            Assert.True(layer.StyleGroups.ElementAt(0).Styles[1] is LineStyle);
            Assert.True(layer.StyleGroups.ElementAt(0).Styles[2] is TextStyle);
            var fillStyle = (FillStyle)layer.StyleGroups.ElementAt(0).Styles.ElementAt(0);
            var lineStyle = (LineStyle)layer.StyleGroups.ElementAt(0).Styles[1];
            var textStyle = (TextStyle)layer.StyleGroups.ElementAt(0).Styles[2];

            Assert.True(fillStyle.Antialias);
            Assert.Equal(1, fillStyle.Opacity.Value);
            Assert.Equal("#3ed53e", fillStyle.Color.Value);

            Assert.Equal(1, lineStyle.Opacity.Value);
            Assert.Equal("#DC143C", lineStyle.Color.Value);

            Assert.Equal("city", textStyle.Label.Value);
            Assert.Equal("#008000", textStyle.Color.Value);
            Assert.Equal(2, textStyle.Font.Value.Count);
            Assert.Equal("Open Sans Regular", textStyle.Font.Value.ElementAt(0));
            Assert.Equal("Arial Unicode MS Regular", textStyle.Font.Value[1]);
            Assert.Equal(16, textStyle.Size.Value);
        }

        [Fact]
        public async Task GetAllLayer()
        {
            var store = GetScopedService<ILayerStore>();
            var layers = await store.GetAllAsync();
            Assert.NotNull(layers);
            Assert.Equal(2, layers.Count);

            var pgLayer = (LayerEntity)layers.First(x => x.Name == "berlin_db");
            var shpLayer = (LayerEntity)layers.First(x => x.Name == "berlin_shp");
            Assert.Equal("berlin_db", pgLayer.Name);
            Assert.Equal("berlin_shp", shpLayer.Name);

            Assert.NotNull(pgLayer.Source);
            Assert.True(pgLayer.Source is SpatialDatabaseSource);
            var pgSource = (SpatialDatabaseSource)pgLayer.Source;
            Assert.Equal("geom", pgSource.Geometry);
            Assert.Equal("osmbuildings", pgSource.Table);
            Assert.Equal("User ID=postgres;Password=1qazZAQ!;Host=localhost;Port=5432;Database=berlin;Pooling=true;",
                pgSource.ConnectionString);
            Assert.Equal(4326, pgSource.Srid);

            Assert.NotNull(pgLayer.StyleGroups);
            Assert.Single(pgLayer.StyleGroups);
            Assert.Equal("style1", pgLayer.StyleGroups.ElementAt(0).Name);
            Assert.Equal(500, pgLayer.StyleGroups.ElementAt(0).MinZoom);
            Assert.Equal(128000, pgLayer.StyleGroups.ElementAt(0).MaxZoom);
            Assert.Equal(ZoomUnits.Scale, pgLayer.StyleGroups.ElementAt(0).ZoomUnit);

            Assert.Equal(3, pgLayer.StyleGroups.ElementAt(0).Styles.Count);
            Assert.True(pgLayer.StyleGroups.ElementAt(0).Styles.ElementAt(0) is FillStyle);
            Assert.True(pgLayer.StyleGroups.ElementAt(0).Styles[1] is LineStyle);
            Assert.True(pgLayer.StyleGroups.ElementAt(0).Styles[2] is TextStyle);
            var pgFillStyle = (FillStyle)pgLayer.StyleGroups.ElementAt(0).Styles.ElementAt(0);
            var pgLineStyle = (LineStyle)pgLayer.StyleGroups.ElementAt(0).Styles[1];
            var pgTextStyle = (TextStyle)pgLayer.StyleGroups.ElementAt(0).Styles[2];

            Assert.True(pgFillStyle.Antialias);
            Assert.Equal(1, pgFillStyle.Opacity.Value);
            Assert.Equal("#3ed53e", pgFillStyle.Color.Value);

            Assert.Equal(1, pgLineStyle.Opacity.Value);
            Assert.Equal("#DC143C", pgLineStyle.Color.Value);

            Assert.Equal("city", pgTextStyle.Label.Value);
            Assert.Equal("#008000", pgTextStyle.Color.Value);
            Assert.Equal(2, pgTextStyle.Font.Value.Count);
            Assert.Equal("Open Sans Regular", pgTextStyle.Font.Value.ElementAt(0));
            Assert.Equal("Arial Unicode MS Regular", pgTextStyle.Font.Value[1]);
            Assert.Equal(16, pgTextStyle.Size.Value);

            Assert.Equal(2, shpLayer.Services.Count);
            Assert.Equal(ServiceType.WMS, shpLayer.Services.ElementAt(0));
            Assert.Equal(ServiceType.WMTS, shpLayer.Services.ElementAt(1));

            Assert.NotNull(shpLayer.Source);
            Assert.True(shpLayer.Source is ShapeFileSource);
            var source = (ShapeFileSource)shpLayer.Source;
            Assert.Equal(4326, source.Srid);
            Assert.EndsWith("osmbuildings.shp", source.File);

            Assert.NotNull(shpLayer.StyleGroups);
            Assert.Single(shpLayer.StyleGroups);
            Assert.Equal("style1", shpLayer.StyleGroups.ElementAt(0).Name);
            Assert.Equal(500, shpLayer.StyleGroups.ElementAt(0).MinZoom);
            Assert.Equal(128000, shpLayer.StyleGroups.ElementAt(0).MaxZoom);
            Assert.Equal(ZoomUnits.Scale, shpLayer.StyleGroups.ElementAt(0).ZoomUnit);

            Assert.Equal(3, shpLayer.StyleGroups.ElementAt(0).Styles.Count);
            Assert.True(shpLayer.StyleGroups.ElementAt(0).Styles.ElementAt(0) is FillStyle);
            Assert.True(shpLayer.StyleGroups.ElementAt(0).Styles[1] is LineStyle);
            Assert.True(shpLayer.StyleGroups.ElementAt(0).Styles[2] is TextStyle);
            var shpFillStyle = (FillStyle)shpLayer.StyleGroups.ElementAt(0).Styles.ElementAt(0);
            var shpLineStyle = (LineStyle)shpLayer.StyleGroups.ElementAt(0).Styles[1];
            var shpTextStyle = (TextStyle)shpLayer.StyleGroups.ElementAt(0).Styles[2];

            Assert.True(shpFillStyle.Antialias);
            Assert.Equal(1, shpFillStyle.Opacity.Value);
            Assert.Equal("#3ed53e", shpFillStyle.Color.Value);

            Assert.Equal(1, shpLineStyle.Opacity.Value);
            Assert.Equal("#DC143C", shpLineStyle.Color.Value);

            Assert.Equal("city", shpTextStyle.Label.Value);
            Assert.Equal("#008000", shpTextStyle.Color.Value);
            Assert.Equal(2, shpTextStyle.Font.Value.Count);
            Assert.Equal("Open Sans Regular", shpTextStyle.Font.Value.ElementAt(0));
            Assert.Equal("Arial Unicode MS Regular", shpTextStyle.Font.Value[1]);
            Assert.Equal(16, shpTextStyle.Size.Value);
        }
    }
}