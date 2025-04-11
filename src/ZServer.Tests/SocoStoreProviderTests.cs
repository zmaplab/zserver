using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Xunit;
using ZServer.Store;

namespace ZServer.Tests;

[Collection("WebApplication collection")]
public class SocoStoreProviderTests(WebApplicationFactoryFixture fixture)
{
    [Fact]
    public async Task GetConfiguration()
    {
        var httpClientFactory = fixture.Instance.Services.GetRequiredService<IHttpClientFactory>();
        Environment.SetEnvironmentVariable("ZSERVER_SOCODB_APPID", "66457c75702a1e87e2f4b4ac");
        Environment.SetEnvironmentVariable("ZSERVER_SOCODB_APPSECRET", "OveFGUp8VdZa47rLElD1yEBtDig=");

        var provider =
            new SocoStoreProvider("https://jsyhswfz-api.zyjinke.cn/socodb/v1.0/tables/67f8adafe84e12c3a3dd862c/data",
                httpClientFactory, fixture.Instance.Services.GetRequiredService<ILogger<SocoStoreProvider>>());
        var b = await provider.GetConfigurationAsync();
    }

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