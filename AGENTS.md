# ZServer AGENTS.md

This file provides guidelines for AI agents working on the ZServer codebase.

## 1. Build, Test & Run Commands

### Build
```bash
# Build entire solution
dotnet build ZServer.sln

# Build specific project
dotnet build src/ZServer.API/ZServer.API.csproj

# Build for release
dotnet build -c Release ZServer.sln
```

### Run
```bash
# Run API (standalone mode)
dotnet run --project src/ZServer.API/ZServer.API.csproj -- --Standalone true --Port 8200

# Run API (cluster mode)
dotnet run --project src/ZServer.API/ZServer.API.csproj -- --Standalone false --ClusterSiloPort 10001 --ClusterGatewayPort 20001 --Port 8100
```

### Test
```bash
# Run all tests
dotnet test src/ZServer.Tests/ZServer.Tests.csproj

# Run single test class
dotnet test src/ZServer.Tests/ZServer.Tests.csproj --filter "FullyQualifiedName~LayerStoreTests"

# Run single test method
dotnet test src/ZServer.Tests/ZServer.Tests.csproj --filter "FullyQualifiedName~LayerStoreTests.ShouldRefresh"

# Run with verbose output
dotnet test -v n src/ZServer.Tests/ZServer.Tests.csproj --filter "FullyQualifiedName~TestName"

# Run tests with coverage
dotnet test src/ZServer.Tests/ZServer.Tests.csproj --collect:"XPlat Code Coverage"
```

### Package
```bash
# Publish ZServer.API
dotnet publish src/ZServer.API/ZServer.API.csproj -c Release -r linux-x64 --self-contained

# Publish interfaces
./publish_interfaces.sh
```

---

## 2. Code Style Guidelines

### 2.1 Language & Framework

- **Target Framework**: net10.0 (latest), with fallback to net9.0, net8.0
- **C# Version**: Latest (primary constructors, pattern matching)
- **Nullable**: Disabled (no `#nullable enable`)

### 2.2 Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Class/Interface | PascalCase | `LayerStore`, `ILayerStore` |
| Method | PascalCase | `GetMapAsync`, `FindAsync` |
| Property | PascalCase | `Cache`, `LayerName` |
| Field (private static) | PascalCase | `PropertyCache`, `Logger` |
| Local variable | camelCase | `layerName`, `configurations` |
| Parameter | camelCase | `styleStore`, `resourceGroup` |
| Namespace | PascalCase | `ZServer.Store`, `ZMap.Renderer` |
| Enum | PascalCase | `ServiceType`, `GeometryType` |
| Constant | PascalCase | `DefaultBufferSize` |

### 2.3 File Organization

- **One public class per file**: Match filename to class name
- **Using statements**: Sorted alphabetically at top of file
- **Implicit usings**: Not used - explicit `using` for each namespace
- **Namespace**: Match folder structure (`src/ZServer/Store/LayerStore.cs` → `namespace ZServer.Store`)

```csharp
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using ZMap;
using ZMap.Infrastructure;
using ZMap.Source;
using ZMap.Style;

namespace ZServer.Store;
```

### 2.4 Class Structure

```csharp
// Primary constructor (C# 9+)
public class LayerStore(
    IStyleGroupStore styleStore,
    IResourceGroupStore resourceGroupStore,
    ISourceStore sourceStore,
    ISldStore sldStore)
    : ILayerStore
{
    // Private static readonly fields first
    private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> PropertyCache = new();
    private static readonly Lazy<ILogger> Logger = new(Log.CreateLogger<LayerStore>());
    private static readonly ConcurrentDictionary<string, Layer> Cache = new();

    // Public methods
    public async Task RefreshAsync(List<JObject> configurations) { }

    // Private methods
    private async Task<Layer> BindLayerAsync(...) { }
}
```

### 2.5 Documentation

- **XML Documentation**: Required for all public APIs
- **Language**: Chinese comments for user-facing APIs, English for internal

```csharp
/// <summary>
/// WMS 服务接口
/// </summary>
/// <param name="layers">图层名称，支持两种方式...</param>
[HttpGet]
public async Task GetAsync(...) { }
```

### 2.6 Error Handling

- **Custom exceptions**: Use specific exception types, not generic `Exception`
- **Global exception filter**: `GlobalExceptionFilter` in `ZServer.API/Filters/`
- **Logging**: Always log errors with context
- **Never swallow exceptions**: Never use empty catch blocks

```csharp
// GOOD - specific exception with logging
if (source == null)
{
    Logger.Value.LogError("图层 {ResourceGroup}:{Name} 的数据源 {SourceName} 不存在", 
        resourceGroup, name, sourceName);
    return null;
}

// GOOD - throw specific exception
throw new FileNotFoundException("Invariant sql is missing", scriptPath);

// BAD - generic exception
throw new Exception("Something went wrong");

// BAD - empty catch
catch (Exception) { }
```

### 2.7 Async/Await

- **Always use async**: Never use `.Result` or `.Wait()` on async methods
- **Use `ValueTask`**: For methods that may synchronously complete

```csharp
// GOOD
public async Task<Layer> FindAsync(string layerName)
{
    return await Task.FromResult(Cache.TryGetValue(layerName) ? Cache[layerName] : null);
}

// GOOD - ValueTask for potentially sync completion
public ValueTask<Layer> FindAsync(string layerName)
{
    return Cache.TryGetValue(layerName, out var item)
        ? new ValueTask<Layer>(item.Clone())
        : new ValueTask<Layer>();
}

// BAD - blocking on async
var ips = Dns.GetHostAddressesAsync(hostname).Result;  // DON'T DO THIS
```

### 2.8 Dependency Injection

- **Constructor injection**: All dependencies via constructor (primary constructor)
- **Singleton for static resources**: Use `Lazy<T>` for lazy initialization

```csharp
public class LayerStore(
    IStyleGroupStore styleStore,    // injected
    IResourceGroupStore resourceGroupStore,
    ISourceStore sourceStore)
    : ILayerStore
{
    private static readonly Lazy<ILogger> Logger = new(Log.CreateLogger<LayerStore>());
}
```

### 2.9 HTTP Client

- **Use `IHttpClientFactory`**: Never instantiate `HttpClient` directly

```csharp
// GOOD
private readonly IHttpClientFactory _httpClientFactory;
public MyService(IHttpClientFactory httpClientFactory)
{
    _httpClientFactory = httpClientFactory;
}
public async Task MakeRequest()
{
    var client = _httpClientFactory.CreateClient();
    // use client...
}

// BAD - direct instantiation
var httpClient = new HttpClient();  // DON'T DO THIS
```

### 2.10 Caching

- **Always set size limit**: Never use unbounded `MemoryCache`
- **Set expiration**: Use `AbsoluteExpiration` or `SlidingExpiration`

```csharp
// GOOD - with limits
private static readonly IMemoryCache MemoryCache = new MemoryCache(
    new MemoryCacheOptions
    {
        SizeLimit = 1024,
        ExpirationScanInterval = TimeSpan.FromMinutes(5)
    });

// Use with size and expiration
MemoryCache.GetOrCreate(key, entry =>
{
    entry.Size = 1;
    entry.SlidingExpiration = TimeSpan.FromMinutes(30);
    return value;
});
```

### 2.11 Authentication & Authorization

- **Middleware order matters**: `UseAuthentication()` must come before `UseAuthorization()`
- **Use `[Authorize]`**: Decorate controllers/actions with authorization attributes

```csharp
// CORRECT middleware order
app.UseRouting();
app.UseAuthentication();  // FIRST
app.UseAuthorization(); // SECOND
app.UseEndpoints(...);
```

### 2.12 Configuration

- **No hardcoded secrets**: Never hardcode passwords, keys, or connection strings
- **Use `IConfiguration`**: Inject configuration via DI

```csharp
// GOOD - from configuration
public class MyService
{
    private readonly string _connectionString;
    public MyService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Postgre");
    }
}

// BAD - hardcoded
var connStr = "Host=localhost;Password=secret";  // DON'T
```

### 2.13 Testing

- **Framework**: xUnit
- **Pattern**: Arrange-Act-Assert
- **Base class**: Extend `BaseTests` for integration tests

```csharp
public class LayerStoreTests : BaseTests
{
    [Fact]
    public async Task ShouldRefresh_WhenValidConfig()
    {
        // Arrange
        var store = Service.GetRequiredService<ILayerStore>();
        
        // Act
        await store.RefreshAsync(configurations);
        
        // Assert
        var layer = await store.FindAsync("test-layer");
        Assert.NotNull(layer);
    }
}
```

---

## 3. Project Structure

```
src/
├── ZServer.API/           # ASP.NET Core API entry point
├── ZServer/               # Core business logic
├── ZServer.Grains/        # Orleans grain implementations
├── ZServer.Silo/          # Orleans silo configuration
├── ZServer.Interfaces/    # Shared interfaces
├── ZServer.Tests/         # xUnit tests
├── ZMap/                  # Core domain (rendering, styling)
├── ZMap.Renderer.SkiaSharp/ # SkiaSharp rendering
├── ZMap.Source.*/         # Data sources (PostgreSQL, ShapeFile, COG)
├── ZMap.TileGrid/        # Tile grid definitions
├── ZMap.SLD/             # SLD styling
├── ZMap.DynamicCompiler/ # Runtime C# compilation
├── Client/               # Client tools
├── Console/              # Console utilities
└── ZServer.Benchmark/    # Benchmarks
```

---

## 4. Key Patterns

### 4.1 Store Pattern
- `I[Entity]Store` interface in `ZServer.Store`
- `[Entity]Store` implementation
- Methods: `FindAsync`, `RefreshAsync`, `GetAllAsync`

### 4.2 Grain Pattern (Orleans)
- Grain interfaces in `ZServer.Interfaces`
- Grain implementations in `ZServer.Grains`
- Stateless worker grains for tile rendering

### 4.3 Source Pattern
- `ISource` interface in `ZMap.Source`
- Implementations: `PostgreSource`, `ShapeFileSource`, `COGGeoTiffSource`

---

## 5. Common Tasks

### 5.1 Adding a New Data Source
1. Create `ZMap.Source.[Name]/[Name]Source.cs` implementing `ISource`
2. Add to `ZServer.API` project reference
3. Register in `ServiceCollectionExtensions.cs`

### 5.2 Adding a New API Endpoint
1. Create controller in `src/ZServer.API/Controllers/`
2. Add `[Authorize]` if needed
3. Inject services via constructor
4. Use `HttpContext.WriteZServerResponseAsync()` for responses

### 5.3 Running a Single Test
```bash
dotnet test src/ZServer.Tests/ZServer.Tests.csproj \
  --filter "FullyQualifiedName~Namespace.ClassName.MethodName"
```

---

