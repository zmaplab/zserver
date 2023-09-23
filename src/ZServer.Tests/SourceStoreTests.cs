using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZMap.Source.Postgre;
using ZServer.Store;
using Xunit;
using ZMap.Source.ShapeFile;
using ZMap.Style;

namespace ZServer.Tests
{
    public class SourceStoreTests : BaseTests
    {
        class MyClass
        {
            /// <summary>
            /// 
            /// </summary>
            // [JsonProperty("value")]
            public CSharpExpression<string> Value { get; set; }

            /// <summary>
            /// 
            /// </summary>
            [JsonProperty("value")]
            public string Value2 { get; set; }
        }

        [Fact]
        public async Task LoadFromJson()
        {
            var json = JsonConvert.DeserializeObject(await File.ReadAllTextAsync("layers.json")) as JObject;
            var store = new SourceStore(NullLogger<SourceStore>.Instance);
            await store.Refresh(new List<JObject> { json });

            var dataSource = (PostgreSource)await store.FindAsync("berlin_db");

            var b = (PostgreSource)dataSource.Clone();
            b.Where = "is_deleted = 'f'";

            Assert.True(string.IsNullOrEmpty(dataSource.Where));

            Assert.Equal(
                "User ID=postgres;Password=1qazZAQ!;Host=localhost;Port=5432;Database=berlin;Pooling=true;",
                dataSource.ConnectionString);
            Assert.Equal("berlin", dataSource.Database);
            Assert.True(string.IsNullOrEmpty(dataSource.Where));
            
            var dataSources = await store.GetAllAsync();
            var dataSource1 = (PostgreSource)dataSources.First(x => x is PostgreSource);
            Assert.NotNull(dataSource1);
            Assert.Equal(
                "User ID=postgres;Password=1qazZAQ!;Host=localhost;Port=5432;Database=berlin;Pooling=true;",
                dataSource1.ConnectionString);
            Assert.Equal("berlin", dataSource1.Database);

            var dataSource2 = (ShapeFileSource)dataSources.First(x => x is ShapeFileSource);
            Assert.NotNull(dataSource2);

            Assert.EndsWith("osmbuildings.shp", dataSource2.File);
            Assert.Equal(4326, dataSource2.Srid);
        }

        [Fact]
        public async Task Clone()
        {
            var sourceStore = GetScopedService<ISourceStore>();
            var store = (PostgreSource)await sourceStore.FindAsync("berlin_db");

            var b = (PostgreSource)store.Clone();
            b.Where = "is_deleted = 'f'";
        }

        [Fact]
        public async Task GetPgSource()
        {
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
            Assert.EndsWith("osmbuildings.shp", dataSource.File);
            Assert.Equal(4326, dataSource.Srid);
        }

        [Fact]
        public async Task GetAllSource()
        {
            var sourceStore = GetScopedService<ISourceStore>();
            var dataSources = await sourceStore.GetAllAsync();
            var dataSource1 = (PostgreSource)dataSources.First(x => x is PostgreSource);
            Assert.NotNull(dataSource1);
            Assert.Equal(
                "User ID=postgres;Password=1qazZAQ!;Host=localhost;Port=5432;Database=berlin;Pooling=true;",
                dataSource1.ConnectionString);
            Assert.Equal("berlin", dataSource1.Database);

            var dataSource2 = (ShapeFileSource)dataSources.First(x => x is ShapeFileSource);
            Assert.NotNull(dataSource2);

            Assert.EndsWith("osmbuildings.shp", dataSource2.File);
            Assert.Equal(4326, dataSource2.Srid);
        }
    }
}