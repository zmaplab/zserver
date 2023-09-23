using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using ZServer.Store;

namespace ZServer.Tests;

public class GridSetStoreTests
{
    [Fact]
    public async Task LoadFromJson()
    {
        var json = JsonConvert.DeserializeObject(await File.ReadAllTextAsync("layers.json")) as JObject;
        var gridSetStore = new GridSetStore();
        await gridSetStore.Refresh(new List<JObject> { json });
    }
}