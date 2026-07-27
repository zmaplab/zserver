# Overview Design

**Project**: ZServer - Distributed Map Tile Server
**Author**: Lewis Zou
**Date**: 2026-07-27
**Version**: 1.26.727.236

## Change Log

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.26.727.236 | 2026-07-27 | Lewis Zou | Initial version |

---

## 1. System Architecture

ZServer follows a layered architecture pattern combined with the Orleans Actor model for distributed processing. The system is organized into five primary layers:

```
┌─────────────────────────────────────────────────────────────────────┐
│                        HTTP Layer (ASP.NET Core)                     │
│  ┌──────────────┐ ┌──────────────┐ ┌────────────┐ ┌─────────────┐  │
│  │ WMSController │ │WMTSController│ │XYZController│ │ToolController│  │
│  └──────┬───────┘ └──────┬───────┘ └─────┬──────┘ └──────┬──────┘  │
│         │                │               │               │          │
│  ┌──────┴────────────────┴───────────────┴───────────────┴──────┐   │
│  │                     JWT Authentication Middleware             │   │
│  └──────────────────────────────────────────────────────────────┘   │
├─────────────────────────────────────────────────────────────────────┤
│                      Orleans Grain Layer                             │
│  ┌──────────────┐ ┌──────────────┐ ┌────────────────────────────┐  │
│  │   IWMSGrain   │ │  IWMTSGrain  │ │        IXyzGrain          │  │
│  │  (IntegerKey) │ │ (StringKey)  │ │      (StringKey)          │  │
│  └──────┬───────┘ └──────┬───────┘ └───────────┬────────────────┘  │
│         │                │                     │                    │
├─────────┴────────────────┴─────────────────────┴───────────────────┤
│                      Configuration Layer                             │
│  ┌──────────────┐ ┌──────────────┐ ┌────────────┐ ┌─────────────┐  │
│  │  LayerStore   │ │  SourceStore  │ │ GridSetStore│ │StyleGroup  │  │
│  │               │ │               │ │            │ │   Store    │  │
│  └──────────────┘ └──────────────┘ └────────────┘ └─────────────┘  │
├─────────────────────────────────────────────────────────────────────┤
│                       Rendering Layer (ZMap)                         │
│  ┌──────────┐ ┌───────────┐ ┌──────────┐ ┌──────────────────────┐  │
│  │   Map     │ │   Layer   │ │  Feature │ │   Style→Visitor→    │  │
│  │           │ │           │ │          │ │     Renderer        │  │
│  └──────────┘ └───────────┘ └──────────┘ └──────────────────────┘  │
├─────────────────────────────────────────────────────────────────────┤
│                       Data Source Layer                              │
│  ┌────────┐ ┌──────────┐ ┌────────┐ ┌──────┐ ┌─────────────────┐  │
│  │PostGIS │ │ShapeFile │ │  COG   │ │ GDAL │ │ Remote WMTS     │  │
│  └────────┘ └──────────┘ └────────┘ └──────┘ └─────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

### 1.1 Layer Descriptions

| Layer | Responsibility | Technology |
|-------|---------------|------------|
| **HTTP Layer** | API endpoint exposure, request validation, authentication | ASP.NET Core Controllers, JWT Middleware |
| **Grain Layer** | Distributed tile processing, state management, load balancing | Microsoft Orleans Grains |
| **Configuration Layer** | Layer/source/gridset/style metadata loading and caching | JSON file stores |
| **Rendering Layer** | Map rendering pipeline — style application, feature rendering | ZMap, SkiaSharp |
| **Data Source Layer** | Geospatial data access abstraction | PostGIS, GDAL, ShapeFile, COG |

---

## 2. Module Breakdown

The ZServer solution consists of 16 projects in the `ZServer.sln`:

| # | Project | Description | Dependencies |
|---|---------|-------------|--------------|
| 1 | **ZMap** | Core mapping engine: Map/Layer definitions, OGC protocol logic, style system, rendering pipeline abstractions, spatial indexing, feature model | NetTopologySuite, ProjNET |
| 2 | **ZMap.Renderer.SkiaSharp** | SkiaSharp implementation of the rendering pipeline (17 classes) | ZMap, SkiaSharp |
| 3 | **ZMap.TileGrid** | Tile grid mathematics: GridSets, coordinate reference system transforms | ProjNET |
| 4 | **ZMap.SLD** | Styled Layer Descriptor XML parsing and style generation | ZMap |
| 5 | **ZMap.DynamicCompiler** | Natasha-based C# runtime compilation for CQL filter expressions | ZMap |
| 6 | **ZMap.Source.Postgre** | PostgreSQL/PostGIS vector data source implementation | ZMap, Npgsql |
| 7 | **ZMap.Source.ShapeFile** | ShapeFile vector data source implementation | ZMap, GDAL |
| 8 | **ZMap.Source.CloudOptimizedGeoTIFF** | COG GeoTIFF raster data source implementation | ZMap, GDAL |
| 9 | **ZMap.Source.GDAL** | GDAL-based raster data source for format-agnostic access | ZMap, GDAL |
| 10 | **ZServer.API** | ASP.NET Core host: Controllers, middleware, JWT auth, program entry (25 files) | All ZMap/ZServer projects |
| 11 | **ZServer** | Server configuration stores: LayerStore, SourceStore, GridSetStore, StyleGroupStore (24 files) | ZMap |
| 12 | **ZServer.Interfaces** | Orleans grain contracts: IWMSGrain, IWMTSGrain, IXyzGrain | Orleans |
| 13 | **ZServer.Grains** | Orleans grain implementations: WMSGrain, WMTSGrain, XyzGrain | ZServer.Interfaces, ZMap |
| 14 | **ZServer.Silo** | Orleans silo configuration and extension methods | Orleans |
| 15 | **ZServer.SiloHost** | Standalone silo host entry point | ZServer.Silo |
| 16 | **ZServer.Tests** | xUnit test suite (50+ test files) | All projects |

**Supporting Projects:**

| # | Project | Description |
|---|---------|-------------|
| 17 | **ZServer.Benchmark** | BenchmarkDotNet performance benchmarks |
| 18 | **Client** | Sample WMS client with auto-generated OgcSymbologyEncoding |
| 19 | **Console** | Console demo application |
| 20 | **Web** | Leaflet.js frontend with Parcel bundler |

---

## 3. Core Request Flows

### 3.1 WMS GetMap Flow

```
Client ──→ GET /wms?SERVICE=WMS&REQUEST=GetMap&LAYERS=...&BBOX=...&...
                  │
                  ▼
         WMSController.GetMap()
                  │
                  ▼
         Parameter Validation
                  │
                  ▼
         Layer Query (ILayerQueryService)
                  │
                  ▼
         Permission Check (if auth enabled)
                  │
                  ▼
         WmsService.GetMapAsync()
                  │
          ┌───────┴───────┐
          ▼               ▼
    IWMSGrain.GetMap()  Grain activation (if needed)
          │
          ▼
    Map.RenderAsync()
          │
          ▼
    For each Layer (reversed order):
      │
      ├── CoordinateTransform (source CRS → target CRS)
      ├── IntersectionCheck (layer bounds vs requested BBOX)
      └── Dispatch to renderer:
          ├── VectorLayer → IVectorSource → SkiaSharp VectorRenderer
          ├── RasterLayer → IRasterSource → SkiaSharp RasterRenderer
          └── TiledLayer  → ITiledSource → tile compositing
          │
          ▼
    Return rendered image bytes
```

### 3.2 WMTS GetTile Flow

```
Client ──→ GET /wmts?SERVICE=WMTS&REQUEST=GetTile&LAYER=...&TILEMATRIX=...&TILEROW=...&TILECOL=...
                  │
                  ▼
         WMTSController.GetTile()
                  │
                  ▼
         Parameter Validation
                  │
                  ▼
         Cache Check (file system)
                  │
          ┌───────┴───────┐
          ▼               ▼
    Cache Hit? ──→ Return cached tile (200)
          │
          ▼ (miss)
    Resolve grain key = grid path string
          │
          ▼
    IWMTSGrain.GetTile(tileMatrix, tileRow, tileCol)
          │
          ▼
    WMTSGrain → Map.RenderAsync()
          │
          ▼
    Cache tile → Return rendered image
```

### 3.3 XYZ Tile Flow

```
Client ──→ GET /xyz/{layers}?x={x}&y={y}&z={z}
                  │
                  ▼
         XYZController.GetTile()
                  │
                  ▼
    Map XYZ parameters to WMTS equivalent
    (x → TileCol, y → TileRow, z → TileMatrix)
                  │
                  ▼
    Delegate to WMTS processing pipeline
```

### 3.4 Cluster Mode vs Standalone Mode

**Standalone Mode** (default for development):
```
┌──────────────────────────────────┐
│   Single Process                 │
│  ┌──────────┐  ┌──────────────┐ │
│  │ API Host │  │ Orleans Silo │ │
│  │ (Kestrel)│◄─┤ (in-process) │ │
│  └──────────┘  └──────────────┘ │
└──────────────────────────────────┘
```

**Cluster Mode** (production):
```
┌────────────────────────────────────────────────────────┐
│                    PostgreSQL                            │
│           (Orleans Membership Table)                     │
└────────────────────────────────────────────────────────┘
          ▲              ▲              ▲
          │              │              │
┌─────────┴──────┐ ┌────┴────────┐ ┌──┴─────────────┐
│  Silo Node 1   │ │ Silo Node 2 │ │ API Gateway    │
│  (Gateway:     │ │             │ │ (Silo-less     │
│   20001,       │ │             │ │  client)       │
│   Silo: 10001) │ │             │ │                │
│  ┌──────────┐  │ │ ┌────────┐ │ │ ┌────────────┐ │
│  │ Grains   │  │ │ │ Grains │ │ │ │ Grain      │ │
│  │          │  │ │ │        │ │ │ │ Client     │ │
│  └──────────┘  │ │ └────────┘ │ │ └────────────┘ │
└────────────────┘ └────────────┘ └─────────────────┘
                                 │
                                 ▼
                          HTTP Load Balancer
                                 │
                                 ▼
                          External Clients
```

---

## 4. Orleans Actor Model Design

### 4.1 Grain Architecture

ZServer uses three grain types to handle map requests:

| Grain | Key Type | Grain Key | Purpose | Load Balancing Strategy |
|-------|----------|-----------|---------|------------------------|
| **WMSGrain** | IntegerKey | Hash of (layers, bbox, srs, width, height, style, format) | WMS GetMap requests | Random distribution — identical requests hash to the same grain for caching |
| **WMTSGrain** | StringKey | Tile grid path (layer/matrix/row/col) | WMTS GetTile requests | Uniform distribution — each tile path maps to a unique grain |
| **XyzGrain** | StringKey | Tile grid path (mapped from XYZ params) | XYZ tile requests | Uniform distribution — delegates to WMTS grain |

### 4.2 Grain Activation and Lifecycle

- **Stateless workers**: All grains are marked as stateless workers where possible, allowing Orleans to activate multiple instances across the cluster.
- **No persistent state**: Tile grains maintain no persistent state — they re-render on each activation and rely on the file-system cache for performance.
- **Automatic deactivation**: Idle grains are deactivated by Orleans after the configured idle timeout, freeing cluster resources.

### 4.3 Why Grains for Tile Rendering?

1. **Lock-free concurrency**: Orleans guarantees single-threaded access to each grain, eliminating lock contention on tile rendering.
2. **Automatic load balancing**: Orleans distributes grain activations evenly across silo nodes.
3. **Transparent scaling**: Adding more silo nodes automatically redistributes grain activations.
4. **Fault tolerance**: If a silo fails, grains are automatically reactivated on surviving nodes.

---

## 5. Dependency Relationships

```
ZServer.SiloHost
    └── ZServer.Silo
         └── ZServer.Grains
              ├── ZServer.Interfaces (Orleans contracts)
              ├── ZMap (core engine)
              └── ZServer (configuration stores)

ZServer.API
    ├── ZServer.Interfaces
    ├── ZServer.Grains
    ├── ZMap
    └── ZServer (configuration)

ZMap
    ├── ZMap.TileGrid (tile math, CRS transforms)
    ├── ZMap.SLD (SLD parsing)
    ├── ZMap.DynamicCompiler (CQL runtime compilation)
    └── ZMap.Source.Postgre / ShapeFile / GDAL / COG (data sources)

ZMap.Renderer.SkiaSharp
    └── ZMap
```

---

## 6. Configuration Architecture

ZServer uses a JSON-based configuration system with environment variable overrides:

```
conf/
├── appsettings.json          # Main configuration
└── serilog.json              # Serilog logging configuration
```

### 6.1 Configuration Hierarchy

```
appsettings.json
    │
    ├── "layers": [...]               # Layer definitions (styles, sources, CRS)
    ├── "sources": {...}              # Data source connections
    ├── "gridSets": {...}             # Tile grid set definitions
    ├── "styleGroups": {...}          # Grouped style definitions
    ├── "resourceGroups": [...]       # Resource -> layer mapping for auth
    └── "orleans": {...}              # Orleans clustering config
```

### 6.2 Environment Variable Overrides

All configuration values can be overridden using environment variables prefixed with `ZSERVER_`. Nested JSON keys are flattened using `__` (double underscore) as a separator.

Example:
```bash
ZSERVER_PostgreSQLConnectionString="Host=db;Database=..."
ZSERVER_Orleans__ClusterOptions__ClusterId="zserver-prod"
```

### 6.3 Configuration Stores

The `ZServer` project provides four configuration stores:

| Store | Type | Description |
|-------|------|-------------|
| `LayerStore` | Singleton | Loads and caches layer definitions from JSON |
| `SourceStore` | Singleton | Manages data source connection configurations |
| `GridSetStore` | Singleton | Provides tile grid set definitions |
| `StyleGroupStore` | Singleton | Manages grouped style configurations |

These stores are populated at startup from the `appsettings.json` file and can optionally be refreshed at runtime.
