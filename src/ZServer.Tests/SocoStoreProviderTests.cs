using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xunit;
using ZServer.Store;

namespace ZServer.Tests;

public class SocoStoreProviderTests
{
    [Fact]
    public void JsonFormate()
    {
        var j1 = SocoStoreProvider.Read("""
                                        {
                                            "success": true,
                                            "code": 0,
                                            "msg": "",
                                            "data": [
                                            ]
                                        }
                                        """);
        Assert.Null(j1);
        var j2 = SocoStoreProvider.Read("""
                                        {
                                            "success": true,
                                            "code": 0,
                                            "msg": "",
                                            "data": [{}
                                            ]
                                        }
                                        """);
        Assert.True(j2 is JObject);
        SocoStoreProvider.Read(null);
        SocoStoreProvider.Read("");
    }
}