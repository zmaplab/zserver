# ZServer - Configuration & Stores

## OVERVIEW
Server configuration layer — loads layer definitions, data sources, grid sets, and style groups from JSON config. 24 C# files + runtime assets.

## STRUCTURE
```
ZServer/
├── Store/
│   ├── LayerStore.cs            # Layer configuration loader (361 lines)
│   ├── StyleGroupStore.cs       # Style group configuration (300 lines)
│   ├── GridSetStore.cs          # GridSet configuration (276 lines)
│   └── SourceStore.cs           # Data source configuration (238 lines)
├── Extensions/                  # Service registration extensions
├── location.svg                 # Default location marker
└── ZServer.csproj               # References ZMap, ZMap.Renderer.SkiaSharp, ZMap.Source.ShapeFile
```

## WHERE TO LOOK
| Task | Location | Notes |
|------|----------|-------|
| Add config section for new layer type | `Store/LayerStore.cs` | JSON deserialization + validation |
| Add data source config | `Store/SourceStore.cs` | Maps config to IVectorSource/IRasterSource instances |
| Add GridSet definition | `Store/GridSetStore.cs` | Predefined GridSets in DefaultGridSets |
| Add style group | `Store/StyleGroupStore.cs` | Style configuration + SLD references |
| Change DI registration | `Extensions/` | Extension methods for service collection |

## INTERNAL PATTERNS
- **Config loading**: All stores read from `conf/appsettings.json` sections, deserialize to typed config models
- **Store pattern**: Each store is a singleton service registered via extension methods. Load-on-startup pattern.
- **Layer config**: Layer definitions include source references, style references, CRS, bounding box, visibility limits
- **Source config**: Maps source type names to actual implementations (Postgre → ZMap.Source.Postgre, etc.)
- **GridSet config**: Predefined gridsets in `DefaultGridSets.cs` (245 lines) — EPSG:4326, EPSG:3857, etc.

## NOTES
- `LayerStore.cs` is the most complex store (361 lines) — handles nested layer trees, layer groups, resource groups
- `location.svg` is the default point marker icon for vector rendering
- Stores use `IConfiguration` from `Microsoft.Extensions.Configuration`
- The `ZServer` project bundles store logic separate from ZMap core to keep data access decoupled from mapping logic
