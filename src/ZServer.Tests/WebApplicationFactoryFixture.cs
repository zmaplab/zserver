using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using ZServer.API;

namespace ZServer.Tests;

public class WebApplicationFactoryFixture : IDisposable, IAsyncDisposable
{
    public WebApplicationFactory<Program> Instance { get; private set; } = new();

    public void Dispose()
    {
        Instance?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (Instance != null) await Instance.DisposeAsync();
    }
}

[CollectionDefinition("WebApplication collection")]
public class WebApplicationFactoryCollection : ICollectionFixture<WebApplicationFactoryFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}