# ZServer Architecture Refactoring

**Date**: 2026-06-11 | **Status**: Draft | **Approach**: B — Modular Monolith

## Goals

1. **Code organization** — Break up ZMap (204 files) into focused packages with clean boundaries
2. **Performance/scaling prep** — Make renderer/source truly pluggable, preparing for future Orleans distribution
3. **Technical debt** — Enable nullable, fix sync-over-async, consolidate logging, clean up Orleans config

## Section 1: Package Split

### Current

16 projects, ZMap is a single project with 204 files mixing 8 concerns.

### Target

```
src/
├── ZMap.Core/                    # Layer, Map, Feature, Envelope, Zoom, ResourceGroup
│                                 # ~15 files — pure domain, zero infrastructure deps
├── ZMap.Ogc/                     # WMS/WMTS protocol parsing, request validation
│   ├── Wms/                      # WmsService, ParameterValidator
│   └── Wmts/                     # WmtsService, tile matrix logic
│                                 # refs: ZMap.Core, ZMap.TileGrid
├── ZMap.Style/                   # StyleGroup, SldStyleVisitor, IStyleVisitor, style defs
│                                 # refs: ZMap.Core, ZMap.DynamicCompiler
├── ZMap.Rendering.Abstractions/  # IGraphicsService, IGraphicsServiceProvider, Viewport
│                                 # refs: ZMap.Core
├── ZMap.SLD/                     # Auto-generated XSD classes (112 files, moved from ZMap)
│                                 # refs: none (pure data model)
├── ZMap.Source.Abstractions/     # IVectorSource, IRasterSource, ITiledSource, ISource
│                                 # refs: ZMap.Core
├── ZMap.TileGrid/                # Unchanged — GridSet, GridSetFactory
├── ZMap.DynamicCompiler/         # Unchanged — Natasha-based compilation
│
├── ZMap.Renderer.SkiaSharp/      # Unchanged — SkiaSharp impl
├── ZMap.Source.Postgre/          # Unchanged
├── ZMap.Source.ShapeFile/        # Unchanged
├── ZMap.Source.GDAL/             # Unchanged
├── ZMap.Source.CloudOptimizedGeoTIFF/  # Unchanged
│
├── ZServer.Core/                 # Store layer + DI composition root
│   ├── Store/                    # LayerStore, SourceStore, GridSetStore, LayerQueryService
│   └── Extensions/               # Service registration, plugin wiring
│                                 # refs: ZMap.Core, ZMap.Ogc, ZMap.Style,
│                                 #   ZMap.Rendering.Abstractions, ZMap.Source.Abstractions
├── ZServer.Interfaces/           # Unchanged — grain contracts
├── ZServer.Grains/               # Unchanged — grain implementations
├── ZServer.Silo/                 # Unchanged (Orleans config cleaned in Section 3d)
├── ZServer.SiloHost/             # Unchanged
├── ZServer.API/                  # Web host — refs ZServer.Core only (no individual sources)
└── ZServer.Tests/                # Unit tests per package
```

### Migration Strategy

1. Extract `ZMap.SLD` first — pure move, no logic changes, zero risk
2. Extract `ZMap.Rendering.Abstractions` — IGraphicsService, IGraphicsServiceProvider, Viewport
3. Extract `ZMap.Source.Abstractions` — IVectorSource, IRasterSource, ISource
4. Extract `ZMap.Core` — Layer, Map, Feature, Envelope, Zoom, ResourceGroup
5. Extract `ZMap.Ogc` — WmsService, WmtsService, ParameterValidator
6. Extract `ZMap.Style` — StyleGroup, SldStyleVisitor, IStyleVisitor
7. Remaining files (Extensions, Indexing, Infrastructure, Permission) stay in ZMap temporarily. ZMap re-exports moved types via `[assembly: TypeForwardedTo]` to avoid breaking existing usages.
8. After all consumers migrate to new packages, the old ZMap project is archived.
9. Rename `ZServer` → `ZServer.Core` with consolidated stores

## Section 2: Plugin Architecture

### Problem

`ZServer.csproj` hard-references `ZMap.Renderer.SkiaSharp` and `ZMap.Source.ShapeFile`.
`ZServer.API.csproj` hard-references individual source implementations.
Adding a new renderer or source requires touching multiple projects.

### Solution

Each plugin ships a self-contained DI registration extension. `ZServer.Core` is the composition root.

**Plugin pattern**:
```csharp
// Each plugin exposes:
public static IServiceCollection AddXxx(this IServiceCollection services) { ... }

// ZServer.Core wires them:
services.AddSkiaSharpRenderer();
services.AddPostgreSource();
services.AddShapeFileSource();
services.AddCloudOptimizedGeoTIFFSource();
```

**Dependency flow**:
```
ZServer.API → ZServer.Core → ZMap.*.Abstractions (compile-time)
ZServer.Core → ZMap.Renderer.SkiaSharp, ZMap.Source.* (runtime, via DI)
```

**Benefits**:
- New renderer (e.g. ImageSharp)? One package + one extension method.
- New data source? Same pattern.
- No compile-time dependency from API/ZServer to individual implementations.
- Testability: unit tests swap renderer with a stub via DI.

## Section 3: Technical Debt

### 3a. Enable Nullable — Incrementally

- Remove `<Nullable>disable</Nullable>` from `package.props`
- Each new package enables `<Nullable>enable</Nullable>` in its own `.csproj`
- Files that genuinely need nullable disabled (JSON config deserialization) opt out per-file with `#nullable disable`
- Existing ZMap (during migration) keeps `#nullable disable` until moved to new packages

### 3b. Fix Sync-over-Async

3 files use `.Result`/`.Wait()` — replace with `await` up the call chain.
Likely locations: Orleans bootstrap (synchronous silo setup), GDAL interop.

### 3c. Consolidate Logging

7 files use `Console.WriteLine` (mostly Console/Client sample projects).
Replace with `Log.CreateLogger<T>()` using the existing Serilog infrastructure.

### 3d. Clean Up Orleans Config (`OrleansExtensions.cs`, 166 lines)

- Replace `Assembly.Load($"{invariant}")` with typed ADO.NET clustering registration
- Extract SQL provisioning to a separate `ClusterProvisioner` service
- Split `ConfigureSilo` into `ConfigureStandalone()` / `ConfigureClustered()`
- **Dashboard port sharing**: Replace `ISiloBuilder.UseDashboard()` (separate Kestrel) with `app.UseOrleansDashboard()` middleware on the API port, mounted at `/dashboard` path. This eliminates the need for a second port forward on the gateway.
  - Standalone mode: Remove `UseDashboard()` from silo config, add middleware in `Program.cs` API pipeline
  - Cluster mode: SiloHost still uses `UseDashboard()` on its own port (separate process, no ASP.NET Core host)

## Section 4: Store Layer Consolidation

### Problem

`LayerQueryService` delegates to `LayerGroupStore` + `LayerStore` + `StyleGroupStore`.
Duplicated "find by resourceGroup:layerName" logic across 3 classes.
Style setting interleaved with layer resolution.

### Solution

`LayerQueryService` becomes the single public entry point.
`LayerGroupStore`, `LayerStore`, `StyleGroupStore` become internal.
`IRefresher` removed from `ILayerGroupStore` — refresh handled by `RefreshConfigService`.

```
LayerQueryService ──► WmsService/WmtsService (single facade)
  └── internal: LayerGroupStore, LayerStore, StyleGroupStore
```

## Non-Goals (Out of Scope)

- Orleans grain distribution (Approach C — deferred)
- Tile caching strategy changes
- Frontend (Web/) changes
- Client/Console sample updates (best-effort)

## Risks

| Risk | Mitigation |
|------|------------|
| Package split breaks existing import paths | Incremental extraction; old ZMap re-exports via `[assembly: TypeForwardedTo]` where needed |
| Plugin DI breaks startup | Each extraction validated with `dotnet build && dotnet test` before next step |
| Nullable enable reveals hidden bugs | Per-package opt-in, not big-bang; existing tests catch regressions |
| Store consolidation changes behavior | `LayerQueryService` interface unchanged; internal refactor only |
