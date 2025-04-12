using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;
using ZServer.API;

namespace ZServer.Tests;

public class WebApplicationFactoryFixture : IDisposable, IAsyncDisposable
{
    public WebApplicationFactory<Program> Instance { get; private set; } = new CustomWebApplicationFactory();

    public void Dispose()
    {
        Instance?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (Instance != null) await Instance.DisposeAsync();
    }

    /// <summary>
    /// 自定义Web应用程序工厂，用于在测试中覆盖配置
    /// </summary>
    private class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                // 从环境变量获取连接字符串
                var connStr = Environment.GetEnvironmentVariable("ConnStr");

                if (!string.IsNullOrEmpty(connStr))
                {
                    // 创建包含Orleans连接字符串的配置字典
                    var configDict = new Dictionary<string, string>
                    {
                        {"orleans:connectionString", connStr}
                    };

                    // 将配置添加到配置构建器中，这将覆盖任何现有的配置
                    configBuilder.AddInMemoryCollection(configDict);
                }
            });

            base.ConfigureWebHost(builder);
        }
    }
}

[CollectionDefinition("WebApplication collection")]
public class WebApplicationFactoryCollection : ICollectionFixture<WebApplicationFactoryFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}