using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using ZMap.Source.Postgre;
using ZServer.Store;

namespace ZServer.Tests;

public class DefaultFilterTests
{
    [Fact]
    public async Task LayerStore_Reads_DefaultFilter()
    {
        var json = JsonConvert.DeserializeObject(await File.ReadAllTextAsync("layers.json")) as JObject;
        var styleGroupStore = new StyleGroupStore();
        await styleGroupStore.RefreshAsync(new List<JObject> { json });
        var resourceGroupStore = new ResourceGroupStore();
        await resourceGroupStore.RefreshAsync(new List<JObject> { json });
        var sourceStore = new SourceStore();
        await sourceStore.RefreshAsync(new List<JObject> { json });
        var sldStore = new SldStore();
        await sldStore.RefreshAsync(new List<JObject> { json });

        var store = new LayerStore(styleGroupStore, resourceGroupStore, sourceStore, sldStore);
        await store.RefreshAsync(new List<JObject> { json });

        var layer = await store.FindAsync("resourceGroup1", "berlin_db_filtered");
        Assert.NotNull(layer);
        Assert.Equal("berlin_db_filtered", layer.Name);
        Assert.False(string.IsNullOrEmpty(layer.DefaultFilter));
        Assert.Contains("status", layer.DefaultFilter);
        Assert.Contains("Equal", layer.DefaultFilter);
    }

    [Fact]
    public async Task LayerStore_DefaultFilter_Null_When_Not_Configured()
    {
        var json = JsonConvert.DeserializeObject(await File.ReadAllTextAsync("layers.json")) as JObject;
        var styleGroupStore = new StyleGroupStore();
        await styleGroupStore.RefreshAsync(new List<JObject> { json });
        var resourceGroupStore = new ResourceGroupStore();
        await resourceGroupStore.RefreshAsync(new List<JObject> { json });
        var sourceStore = new SourceStore();
        await sourceStore.RefreshAsync(new List<JObject> { json });
        var sldStore = new SldStore();
        await sldStore.RefreshAsync(new List<JObject> { json });

        var store = new LayerStore(styleGroupStore, resourceGroupStore, sourceStore, sldStore);
        await store.RefreshAsync(new List<JObject> { json });

        var layer = await store.FindAsync("resourceGroup1", "berlin_db");
        Assert.NotNull(layer);
        Assert.Null(layer.DefaultFilter);
    }

    [Fact]
    public async Task Layer_Clone_Copies_DefaultFilter()
    {
        var json = JsonConvert.DeserializeObject(await File.ReadAllTextAsync("layers.json")) as JObject;
        var styleGroupStore = new StyleGroupStore();
        await styleGroupStore.RefreshAsync(new List<JObject> { json });
        var resourceGroupStore = new ResourceGroupStore();
        await resourceGroupStore.RefreshAsync(new List<JObject> { json });
        var sourceStore = new SourceStore();
        await sourceStore.RefreshAsync(new List<JObject> { json });
        var sldStore = new SldStore();
        await sldStore.RefreshAsync(new List<JObject> { json });

        var store = new LayerStore(styleGroupStore, resourceGroupStore, sourceStore, sldStore);
        await store.RefreshAsync(new List<JObject> { json });

        var layer = await store.FindAsync("resourceGroup1", "berlin_db_filtered");
        Assert.NotNull(layer);

        var cloned = layer.Clone();
        Assert.Equal(layer.DefaultFilter, cloned.DefaultFilter);
        Assert.Equal(layer.Name, cloned.Name);
    }

    [Fact]
    public async Task FilterMerger_Integration_With_DefaultFilter()
    {
        var json = JsonConvert.DeserializeObject(await File.ReadAllTextAsync("layers.json")) as JObject;
        var styleGroupStore = new StyleGroupStore();
        await styleGroupStore.RefreshAsync(new List<JObject> { json });
        var resourceGroupStore = new ResourceGroupStore();
        await resourceGroupStore.RefreshAsync(new List<JObject> { json });
        var sourceStore = new SourceStore();
        await sourceStore.RefreshAsync(new List<JObject> { json });
        var sldStore = new SldStore();
        await sldStore.RefreshAsync(new List<JObject> { json });

        var store = new LayerStore(styleGroupStore, resourceGroupStore, sourceStore, sldStore);
        await store.RefreshAsync(new List<JObject> { json });

        var layer = await store.FindAsync("resourceGroup1", "berlin_db_filtered");
        Assert.NotNull(layer);

        var requestFilter = """
            {
              "Logic": "And",
              "Filters": [
                {
                  "Field": "type",
                  "Operator": "Equal",
                  "Value": "building"
                }
              ]
            }
            """;

        var merged = ZMap.FilterMerger.Merge(layer.DefaultFilter, requestFilter);
        Assert.Contains("status", merged);
        Assert.Contains("type", merged);
        Assert.Contains("building", merged);
    }
}
