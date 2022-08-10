using System.Threading.Tasks;
using ZMap.Source.Postgre;
using ZServer.Store;
using Xunit;
using ZMap.Source.ShapeFile;

namespace ZServer.Tests
{
    public class SourceStoreTests : BaseTests
    {
        [Fact]
        public async Task GetPgSource()
        {
            PostgreSource.Initialize();

            var sourceStore = GetScopedService<ISourceStore>();
            var dataSource = (PostgreSource)await sourceStore.FindAsync("berlin_db");

            Assert.NotNull(dataSource);
            Assert.Equal(
                "User ID=postgres;Password=1qazZAQ!;Host=localhost;Port=5432;Database=berlin;Pooling=true;",
                dataSource.ConnectionString);
            Assert.Equal("berlin", dataSource.Database);
        }

        [Fact]
        public async Task GetShapeSource()
        {
            var sourceStore = GetScopedService<ISourceStore>();
            var dataSource = (ShapeFileSource)await sourceStore.FindAsync("berlin_shp");

            Assert.NotNull(dataSource);
            Assert.Equal(
                "shapes/osmbuildings.shp",
                dataSource.File);
            Assert.Equal(4326, dataSource.SRID);
        }

        [Fact]
        public async Task GetAllSource()
        {
            var sourceStore = GetScopedService<ISourceStore>();
            var dataSources = await sourceStore.GetAllAsync();
            var dataSource1 = (PostgreSource)dataSources[0];
            Assert.NotNull(dataSource1);
            Assert.Equal(
                "User ID=postgres;Password=1qazZAQ!;Host=localhost;Port=5432;Database=berlin;Pooling=true;",
                dataSource1.ConnectionString);
            Assert.Equal("berlin", dataSource1.Database);

            var dataSource2 = (ShapeFileSource)dataSources[1];
            Assert.NotNull(dataSource2);
            Assert.Equal(
                "shapes/osmbuildings.shp",
                dataSource2.File);
            Assert.Equal(4326, dataSource2.SRID);
        }
    }
}