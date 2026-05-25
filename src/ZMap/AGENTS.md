# ZMap - Core Mapping Library

## OVERVIEW
ZMap is the core mapping engine — layer definitions, OGC protocol logic, rendering pipeline, styling, spatial indexing, and data source abstractions. 204 C# files.

## STRUCTURE
```
ZMap/
├── Layer.cs, Map.cs, Feature.cs   # Core domain: layers, maps, geojson features
├── Ogc/                           # WMS/WMTS protocol (WmsService, WmtsService)
├── Renderer/                      # Rendering interfaces + pipeline (IRenderer, IVisitor)
├── Style/                         # Styling: SldStyleVisitor, style definitions
├── SLD/                           # SLD document model
├── Source/                        # Data source abstractions (IVectorSource, IRasterSource)
├── Store/                         # Layer/source storage abstractions
├── Infrastructure/                # CRS, proj, logging, coordinate transforms
├── Indexing/                      # Spatial indexing
├── Extensions/                    # Extension methods
└── Permission/                    # Authorization model
```

## WHERE TO LOOK
| Task | Location | Notes |
|------|----------|-------|
| Add OGC operation | `Ogc/Wms/WmsService.cs` | Protocol request parsing + response building |
| Add renderer implementation | `Renderer/` | Implement IRenderer, follow IVisitor pattern |
| Add style type | `Style/` | Style→Visitor→Renderer pipeline |
| Add spatial filter/index | `Indexing/` | Spatial indexing strategies |
| Add coordinate system | `Infrastructure/CoordinateReferenceSystem.cs` | EPSG registry, proj transforms |
| Change layer behavior | `Layer.cs` | RasterLayer, VectorLayer, TiledLayer subclasses |

## INTERNAL PATTERNS
- **Layer hierarchy**: `Layer` base → `RasterLayer`, `VectorLayer`, `TiledLayer` (all in `Layer.cs`)
- **Rendering pipeline**: Style → Visitor → Renderer. `IStyleVisitor` walks styles, `IRenderer` produces output
- **OGC flow**: `WmsService`/`WmtsService` parse OGC XML params → build map request → render via IRenderer
- **Source abstraction**: `IVectorSource` (features) and `IRasterSource` (imagery) — data source plugins implement these
- **Feature model**: GeoJSON `Feature` wraps NetTopologySuite geometries
- **Dynamic compilation**: CQL filters compiled at runtime via `CSharpDynamicCompiler` (Natasha-based)

## NOTES
- `CoordinateReferenceSystem.cs` (377 lines) is the CRS registry — modify carefully
- `SldStyleVisitor.cs` (337 lines) is the SLD→internal style translator
- `Functions.cs` contains global helper methods for styling expressions
