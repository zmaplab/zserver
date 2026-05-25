# PROJECT KNOWLEDGE BASE

**Generated:** 2026-05-25
**Commit:** 9aadada
**Branch:** main

## OVERVIEW
ZServer is a distributed map tile server based on the Orleans Actor model, implementing OGC WMS and WMTS protocols. Each map tile maps to an Actor, enabling lock-free concurrent updates and horizontal scaling.

## STRUCTURE
```
zserver/
├── src/
│   ├── ZMap/                  # Core mapping library (204 .cs) — layers, OGC, rendering pipeline
│   ├── ZServer.API/           # ASP.NET Core host (25 .cs) — controllers, middleware, auth
│   ├── ZServer/               # Server config + stores (24 .cs) — layer/source/gridset loading
│   ├── ZServer.Interfaces/    # Orleans grain contracts (11 .cs) — IWMSGrain, IWMTSGrain, IXyzGrain
│   ├── ZServer.Grains/        # Orleans grain implementations (5 .cs)
│   ├── ZServer.Silo/          # Silo configuration extension (1 .cs)
│   ├── ZServer.SiloHost/      # Standalone silo host entry (2 .cs)
│   ├── ZServer.Tests/         # xUnit tests (50 .cs)
│   ├── ZMap.Renderer.SkiaSharp/ # SkiaSharp renderer (17 .cs)
│   ├── ZMap.TileGrid/         # Tile grid math — GridSets, CRS (7 .cs)
│   ├── ZMap.DynamicCompiler/  # Natasha-based C# dynamic compilation (1 .cs)
│   ├── ZMap.SLD/              # SLD styling support placeholder
│   ├── ZMap.Source.*/         # Data sources: Postgre, ShapeFile, GDAL, COG GeoTIFF
│   ├── ZServer.Benchmark/     # BenchmarkDotNet benchmarks
│   ├── Client/                # Sample WMS client (OgcSymbologyEncoding auto-gen)
│   ├── Console/               # Console demo
│   └── Web/                   # Leaflet frontend
├── docs/                      # Chinese usage docs
├── docker-compose.yml         # Production deployment
├── ZServer.sln                # 16 projects
└── package.props              # Shared MSBuild props (net10.0, Nullable=disable)
```

## WHERE TO LOOK
| Task | Location | Notes |
|------|----------|-------|
| Add a new OGC operation | `src/ZMap/Ogc/` + `src/ZServer.Interfaces/` | Protocol logic in ZMap, grain contracts in Interfaces |
| Add a new data source | `src/ZMap.Source/` + `src/ZMap.Source.*/` | Implement IVectorSource or IRasterSource |
| Add a rendering style | `src/ZMap/Style/` + `src/ZMap/Renderer/` | Style→Visitor→Renderer pipeline |
| Change API endpoints | `src/ZServer.API/Controllers/` | WMS, WMTS, XYZ, Tools controllers |
| Configure Orleans clustering | `src/ZServer.Silo/OrleansExtensions.cs` | Silo+client registration |
| Add authentication | `src/ZServer.API/JwtBearerAuthenticationExtensions.cs` | JWT + scope-based auth |
| Modify config loading | `src/ZServer/Store/` | LayerStore, SourceStore, GridSetStore, StyleGroupStore |

## CODE MAP
| Symbol | Type | Location | Role |
|--------|------|----------|------|
| `Layer` | Class | `src/ZMap/Layer.cs` | Base layer type (RasterLayer, VectorLayer, TiledLayer) |
| `Map` | Class | `src/ZMap/Map.cs` | Map definition — layers, SRS, bounds |
| `IWMSGrain` | Interface | `src/ZServer.Interfaces/WMS/` | WMS grain contract (GetCapabilities, GetMap, GetFeatureInfo) |
| `IWMTSGrain` | Interface | `src/ZServer.Interfaces/WMTS/` | WMTS grain contract |
| `WMSGrain` | Class | `src/ZServer.Grains/WMS/` | WMS Orleans grain impl |
| `IRenderer` | Interface | `src/ZMap/Renderer/` | Rendering pipeline entry |
| `IVectorSource` | Interface | `src/ZMap/Source/` | Vector data source contract |
| `IRasterSource` | Interface | `src/ZMap/Source/` | Raster data source contract |
| `ILayerQueryService` | Interface | `src/ZMap/` | Layer metadata query |
| `Feature` | Class | `src/ZMap/Feature.cs` | GeoJSON Feature wrapper |
| `CSharpDynamicCompiler` | Class | `src/ZMap.DynamicCompiler/` | Runtime C# expression compilation |

## CONVENTIONS
- **net10.0** target framework, `<LangVersion>latest</LangVersion>`
- **Nullable disabled** project-wide (not nullable-aware)
- **File-scoped namespaces** (`namespace Foo;`)
- **AllowUnsafeBlocks** enabled (needed by SkiaSharp/GDAL interop)
- **InternalsVisibleTo** `ZServer.Tests` for white-box testing
- Configuration: `conf/appsettings.json` + environment variables prefixed `ZSERVER_`
- Serilog for logging, configured via `conf/serilog.json`
- Chinese comments/documentation (primary)
- GeoJSON via NetTopologySuite (NtsGeometryServices configured at startup)
- Coordinate systems: EPSG:4326 default, EPSG:3857 supported via ProjNET

## ANTI-PATTERNS (THIS PROJECT)
- **Do NOT** add `Nullable=enable` without auditing all 300+ files
- **Do NOT** remove `InternalsVisibleTo` — tests depend on it
- **Do NOT** switch from Serilog to Microsoft.Extensions.Logging directly — Serilog is deeply wired
- **Do NOT** bypass the Orleans grain layer for tile operations (use IWMSGrain, not direct DB)
- **Do NOT** add synchronous HTTP calls in grain methods — always async

## COMMANDS
```bash
# Build
dotnet build ZServer.sln

# Test
dotnet test src/ZServer.Tests/ZServer.Tests.csproj

# Run API (standalone mode)
dotnet run --project src/ZServer.API --Standalone true --Port 8200

# Run API (cluster mode)
dotnet run --project src/ZServer.API --Standalone false --ClusterSiloPort 10001 --ClusterGatewayPort 20001 --Port 8100

# Run SiloHost (dedicated silo)
dotnet run --project src/ZServer.SiloHost

# Run benchmarks
dotnet run --project src/ZServer.Benchmark -c Release

# Docker
docker build -f API.Dockerfile -t zserver-api .
docker-compose up
```

## NOTES
- Orleans clustering: uses ADO.NET (PostgreSQL) for cluster membership
- Silo and client can co-host (standalone mode) or separate (cluster mode)
- `src/Client/OgcSymbologyEncoding.cs` is auto-generated (5139 lines) — do not hand-edit
- Frontend (`src/Web/`) is plain Leaflet with Parcel bundler, not integrated into the .NET build
- Dynamic compilation uses Natasha for C# expression evaluation at runtime (e.g., CQL filters)
