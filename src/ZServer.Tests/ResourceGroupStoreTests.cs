using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZServer.Store;
using Xunit;

namespace ZServer.Tests
{
    public class ResourceGroupStoreTests : BaseTests
    {
        [Fact]
        public async Task LoadFromJson()
        {
            var json = JsonConvert.DeserializeObject(await File.ReadAllTextAsync("layers.json")) as JObject;
            var store = new ResourceGroupStore();
            await store.Refresh(new List<JObject> { json });

            var resourceGroup = await store.FindAsync("resourceGroup1");
            Assert.NotNull(resourceGroup);
            Assert.Equal("resourceGroup1", resourceGroup.Name);
            Assert.Contains("This is my first resource group", resourceGroup.Description);

            var resourceGroups = await store.GetAllAsync();
            Assert.NotNull(resourceGroups);
            Assert.Equal(2, resourceGroups.Count);
            Assert.Equal("resourceGroup1", resourceGroups[0].Name);
            Assert.Contains("This is my first resource group", resourceGroups[0].Description);
            Assert.Equal("resourceGroup2", resourceGroups[1].Name);
            Assert.Contains("This is my second resource group", resourceGroups[1].Description);
        }

        [Fact]
        public async Task FindByName()
        {
            var store = GetScopedService<IResourceGroupStore>();

            var resourceGroup = await store.FindAsync("resourceGroup1");
            Assert.NotNull(resourceGroup);
            Assert.Equal("resourceGroup1", resourceGroup.Name);
            Assert.Contains("This is my first resource group", resourceGroup.Description);
        }

        [Fact]
        public async Task GetAll()
        {
            var store = GetScopedService<IResourceGroupStore>();

            var resourceGroups = await store.GetAllAsync();
            Assert.NotNull(resourceGroups);
            Assert.Equal(2, resourceGroups.Count);
            Assert.Equal("resourceGroup1", resourceGroups[0].Name);
            Assert.Contains("This is my first resource group", resourceGroups[0].Description);
            Assert.Equal("resourceGroup2", resourceGroups[1].Name);
            Assert.Contains("This is my second resource group", resourceGroups[1].Description);
        }
    }
}