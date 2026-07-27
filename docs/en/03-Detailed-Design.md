# Detailed Design

**Project**: ZServer - Distributed Map Tile Server
**Author**: Lewis Zou
**Date**: 2026-07-27
**Version**: 1.26.727.236

## Change Log

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.26.727.236 | 2026-07-27 | Lewis Zou | Initial version |

---

## 1. ZMap Core Engine

### 1.1 Map Class

The `Map` class is the central orchestrator of the rendering pipeline. It holds a collection of layers and manages the rendering lifecycle.

**Key Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `Layers` | `List<Layer>` | Ordered list of map layers (rendered in reverse order) |
| `SRS` | `string` | Spatial reference system (e.g., "EPSG:4326") |
| `Bounds` | `Envelope` | Map bounding box |
| `Width` | `int` | Output image width in pixels |
| `Height` | `int` | Output image height in pixels |

**Core Method: `RenderAsync()`**

```csharp
Task<byte[]> RenderAsync(
    IReadOnlyDictionary<string, string> parameters,
    string format,
    string srs,
    Envelope bounds,
    int width,
    int height,
    CancellationToken ct)
```

**Algorithm:**

1. **Parameter Extraction**: Parse rendering parameters from the query dictionary (layer order, styles, CQL filters, time dimensions, elevation).
2. **Layer Enumeration**: Iterate layers in **reverse order** — the first layer in configuration appears on top, rendered last.
3. **Per-Layer Processing**:
   - If the layer is excluded from the current request, skip it.
   - Execute coordinate transform from layer source CRS to requested output CRS.
   - Perform intersection check between layer's spatial bounds and the requested bounding box.
   - Dispatch rendering to the appropriate handler based on layer type (VectorLayer, RasterLayer, TiledLayer).
4. **Image Composition**: Composite all rendered layer images onto the output canvas.
5. **Format Encoding**: Encode the final image to the requested output format (PNG, JPEG, GIF).

### 1.2 Layer Class

The `Layer` abstract class defines the common interface for all layer types. Three concrete implementations exist:

| Layer Type | Description | Data Source Interface |
|------------|-------------|----------------------|
| `VectorLayer` | Renders vector features (points, lines, polygons) | `IVectorSource` |
| `RasterLayer` | Renders raster imagery (GeoTIFF, JPEG2000) | `IRasterSource` |
| `TiledLayer` | Renders from an existing tile service (remote WMTS) | `ITiledSource` |

**Core Method: `RenderAsync()` (VectorLayer example)**

```csharp
async Task RenderAsync(
    RenderTarget target,
    Envelope bounds,
    string srs,
    int width,
    int height,
    IReadOnlyDictionary<string, string> parameters,
    CancellationToken ct)
```

**Algorithm per vector layer:**

1. **Coordinate Transform**: Transform layer geometries from their native CRS to the output CRS using ProjNET coordinate transformation.
2. **Intersection Check**: Compare the transformed layer bounding box against the requested bounding box. Skip rendering if no overlap exists.
3. **Feature Query**: Query the data source (`IVectorSource`) for all features intersecting the requested bounds.
4. **CQL Filter Application**: If a CQL filter is specified, evaluate it using the dynamic compiler to filter features.
5. **Style Application**: For each feature, apply the configured style (color, stroke width, fill pattern, opacity, label).
6. **Rendering**: Dispatch styled features to the renderer for drawing on the output canvas.

### 1.3 Feature Class

The `Feature` class wraps geospatial features with attribute data:

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string` | Feature identifier |
| `Geometry` | `Geometry` | NetTopologySuite geometry (Point, LineString, Polygon, Multi* variants) |
| `Attributes` | `Dictionary<string, object>` | Feature attribute key-value pairs |
| `Style` | `Style` | Optional per-feature style override |

---

## 2. WMS Implementation

### 2.1 WmsService.GetMapAsync()

This is the primary WMS GetMap handler, implementing the OGC WMS 1.3.0 specification.

**Signature:**
```csharp
Task<byte[]> GetMapAsync(
    IDictionary<string, string> parameters,
    string format,
    string layers,
    string srs,
    string bbox,
    int width,
    int height,
    string styles,
    CancellationToken ct)
```

**Algorithm:**

1. **Parameter Validation**:
   - Validate that required parameters (SERVICE, REQUEST, LAYERS, BBOX, SRS, WIDTH, HEIGHT) are present.
   - Validate BBOX format: must be 4 comma-separated numeric values (minx, miny, maxx, maxy).
   - Validate WIDTH and HEIGHT are positive integers within maximum limits.
   - Validate FORMAT is a supported MIME type (image/png, image/jpeg, image/gif).

2. **Layer Resolution**:
   - Parse the comma-separated LAYERS parameter.
   - Query `ILayerQueryService` for layer metadata and configuration.
   - Validate that all requested layers exist.

3. **Permission Check** (if authorization enabled):
   - Extract resource group claims from the JWT token.
   - Verify the user has access to all requested layers' resource groups.
   - Return HTTP 403 if access is denied.

4. **Map Configuration**:
   - Construct a `Map` object with the requested parameters.
   - Assign layers in the order specified by the LAYERS parameter.
   - Apply any requested style overrides.

5. **Rendering**:
   - Call `Map.RenderAsync()` to produce the output image.
   - The map handles layer iteration, coordinate transforms, and compositing.

6. **Response**:
   - Return the rendered image bytes with the appropriate content type.

### 2.2 WmsService.GetFeatureInfoAsync()

Implements the WMS GetFeatureInfo operation, returning attribute data for features at a clicked pixel location.

**Signature:**
```csharp
Task<string> GetFeatureInfoAsync(
    IDictionary<string, string> parameters,
    string format,
    string layers,
    string srs,
    string bbox,
    int width,
    int height,
    int x,
    int y,
    string infoFormat,
    CancellationToken ct)
```

**Algorithm:**

1. **Parameter Validation**: Same as GetMap, plus validate QUERY_LAYERS, X, Y, and INFO_FORMAT parameters.

2. **Coordinate Conversion**: Convert the pixel position (X, Y) to geographic coordinates:
   ```
   geoX = minX + (pixelX / width) * (maxX - minX)
   geoY = maxY - (pixelY / height) * (maxY - minY)   // Y axis inverted
   ```

3. **Search Buffer Calculation**: Apply a 10-pixel sensitivity buffer around the click point:
   ```
   bufferGeoX = (bufferPixels / width) * (maxX - minX)
   bufferGeoY = (bufferPixels / height) * (maxY - minY)
   ```

4. **Feature Query**: For each queryable layer:
   - Query the vector source for features intersecting the buffered search area.
   - Apply any layer-specific CQL filters.
   - Collect feature attributes.

5. **Response Formatting**: Format results as HTML, plain text, or GML (depending on INFO_FORMAT).

---

## 3. WMTS Implementation

### 3.1 Cache-First Check

Before activating any grain, the WMTS controller performs a file-system cache lookup:

```csharp
string cachePath = Path.Combine(
    cacheDirectory,
    $"{layer}/{tileMatrix}/{tileRow}/{tileCol}.{extension}");
```

- **Cache Hit**: Return the cached tile bytes directly with appropriate content-type header.
- **Cache Miss**: Fall through to grain activation and tile rendering.

### 3.2 IWMTSGrain and WMTSGrain

**Grain Key**: The grain key is a string constructed from the tile path:
```
{srs}/{layer}/{style}/{TileMatrixSet}/{TileMatrix}/{TileRow}/{TileCol}
```

This ensures identical tile requests always route to the same grain for optimal caching within the Orleans activation.

**Core Method: `GetTileAsync()`**

```csharp
Task<byte[]> GetTileAsync(
    string tileMatrix,
    int tileRow,
    int tileCol,
    string layer,
    string style,
    string format,
    CancellationToken ct)
```

**Algorithm:**

1. **Grid Set Resolution**: Look up the tile matrix set definition for the requested `TileMatrix`.
2. **Bounding Box Calculation**: Compute the geographic bounding box for the requested tile:
   - Tile matrix origin + (tileCol × tile width) ... (tileRow × tile height)
   - Apply the matrix set's scale denominator and pixel size.
3. **Map Construction**: Build a `Map` object configured for single-tile rendering at the calculated bounding box.
4. **Rendering**: Call `Map.RenderAsync()` to render the tile.
5. **Caching**: Write the rendered tile to the file-system cache.
6. **Return**: Return the rendered tile bytes.

### 3.3 Matrix Set Support

| Grid Set | CRS | Description |
|----------|-----|-------------|
| `GlobalCRS84Pixel` | EPSG:4326 | Global grid in geographic coordinates |
| `GoogleMapsCompatible` | EPSG:3857 | Spherical Mercator grid (Google/Bing/OSM compatible) |

---

## 4. XYZ Implementation

### 4.1 XYZController

The XYZ controller provides a RESTful endpoint compatible with the standard slippy-map tile URL scheme:

```
GET /xyz/{layers}?x={x}&y={y}&z={z}
```

**Parameter Mapping:**

| XYZ Parameter | WMTS Equivalent | Description |
|---------------|-----------------|-------------|
| `z` | `TileMatrix` | Zoom level |
| `x` | `TileCol` | Tile column (longitude index) |
| `y` | `TileRow` | Tile row (latitude index) |
| `{layers}` | (path parameter) | Comma-separated layer names |

### 4.2 Processing Flow

```csharp
Task<byte[]> GetTile(
    string layers,
    int x, int y, int z,
    string format,
    CancellationToken ct)
{
    // Use EPSG:3857 GoogleMapsCompatible grid set
    // Set TileMatrix = z.ToString()
    // Set TileCol = x
    // Set TileRow = y (Y axis: TMS-flipped or standard)
    // Delegate to WMTS processing
    return await wmtsService.GetTileAsync(...);
}
```

---

## 5. Source Abstractions

The data source layer provides a clean abstraction over different geospatial data backends:

### 5.1 IVectorSource

```csharp
public interface IVectorSource
{
    string Name { get; }
    string Type { get; }  // "Postgre", "ShapeFile", etc.
    Task<List<Feature>> GetFeaturesAsync(
        Envelope bounds,
        string srs,
        string? filter,
        CancellationToken ct);
}
```

**Implementations:**

| Implementation | Backend | Key Characteristics |
|----------------|---------|---------------------|
| `PostgreVectorSource` | PostgreSQL/PostGIS | Spatial SQL queries with ST_Intersects, indexed lookups |
| `ShapeFileVectorSource` | ShapeFile (via GDAL) | File-based, suitable for smaller datasets |

### 5.2 IRasterSource

```csharp
public interface IRasterSource
{
    string Name { get; }
    string Type { get; }  // "GDAL", "COG", etc.
    Task<byte[]> GetRasterAsync(
        Envelope bounds,
        string srs,
        int width,
        int height,
        CancellationToken ct);
}
```

**Implementations:**

| Implementation | Backend | Key Characteristics |
|----------------|---------|---------------------|
| `GDALRasterSource` | GDAL | Supports GeoTIFF, JPEG2000, MrSID, and 100+ formats |
| `CloudOptimizedGeoTIFFSource` | GDAL (COG optimized) | Optimized for HTTP range requests on COG files |

### 5.3 ITiledSource

```csharp
public interface ITiledSource
{
    string Name { get; }
    string Type { get; }  // "WMTS"
    Task<byte[]> GetTileAsync(
        string tileMatrix,
        int tileRow,
        int tileCol,
        CancellationToken ct);
}
```

Used for proxying tiles from external WMTS services.

### 5.4 IRemoteHttpSource

```csharp
public interface IRemoteHttpSource
{
    Task<byte[]> GetAsync(string url, CancellationToken ct);
}
```

Generic HTTP source for fetching remote resources (e.g., external style files, remote tile services).

---

## 6. Rendering Pipeline

### 6.1 Architecture: Style → Visitor → Renderer

The rendering pipeline follows a visitor pattern for clean separation of concerns:

```
Style (data) → IStyleVisitor (processing) → IRenderer (output)
```

```
┌────────────────────────────────────────────────────────────┐
│                       Style Tree                            │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌───────────────┐ │
│  │ FillStyle │ │ LineStyle│ │MarkStyle │ │ TextStyle     │ │
│  │ - color   │ │ - color  │ │- shape   │ │ - fontFamily  │ │
│  │ - opacity │ │ - width  │ │- size    │ │ - size        │ │
│  │           │ │ - dash   │ │- fill    │ │ - color       │ │
│  └─────┬─────┘ └────┬─────┘ └────┬─────┘ └──────┬────────┘ │
│        │            │            │               │          │
│        └────────────┴────────────┴───────────────┘          │
│                           │                                  │
└───────────────────────────┼──────────────────────────────────┘
                            │
                            ▼
┌──────────────────────────────────────────────────────────────┐
│                   IStyleVisitor                               │
│  ┌──────────────────┐  ┌──────────────────┐                  │
│  │ VectorStyle      │  │ RasterStyle      │                  │
│  │ Visitor          │  │ Visitor          │                  │
│  └────────┬─────────┘  └────────┬─────────┘                  │
│           │                     │                            │
└───────────┼─────────────────────┼────────────────────────────┘
            │                     │
            ▼                     ▼
┌──────────────────────────────────────────────────────────────┐
│                   IRenderer (SkiaSharp)                       │
│  ┌──────────────────┐  ┌──────────────────┐                  │
│  │ VectorRenderer   │  │ RasterRenderer   │                  │
│  │ DrawLine()       │  │ DrawImage()      │                  │
│  │ DrawPolygon()    │  │                  │                  │
│  │ DrawPoint()      │  │                  │                  │
│  │ DrawLabel()      │  │                  │                  │
│  └──────────────────┘  └──────────────────┘                  │
└──────────────────────────────────────────────────────────────┘
```

### 6.2 Style Types

| Style | Properties | Applied To |
|-------|-----------|------------|
| `FillStyle` | Color, Opacity, OutlineColor, OutlineWidth | Polygon features |
| `LineStyle` | Color, Width, DashPattern, LineJoin, LineCap | Line features |
| `MarkStyle` | Shape (Circle, Square, Triangle, Star), Size, Fill, Stroke | Point features |
| `LabelStyle` | FontFamily, FontSize, Color, HaloColor, HaloRadius, Offset, Placement | Any feature with label text |
| `RasterStyle` | Opacity, Brightness, Contrast, ColorMap | Raster layers |

### 6.3 SkiaSharp Renderer

The `ZMap.Renderer.SkiaSharp` project provides production-ready SkiaSharp implementations:

| Class | Purpose |
|-------|---------|
| `SkiaSharpVectorRenderer` | Renders vector features (lines, polygons, points, labels) using SkiaSharp drawing primitives |
| `SkiaSharpRasterRenderer` | Renders raster data (image compositing, color mapping, opacity blending) |

---

## 7. Configuration Stores

The configuration stores in the `ZServer` project provide a singleton-based caching layer over the JSON configuration:

### 7.1 LayerStore

```csharp
public class LayerStore
{
    Task<List<MapLayer>> GetLayersAsync();
    Task<MapLayer> GetLayerAsync(string name);
    Task<MapLayer> GetLayerByResourceGroupAsync(string resourceGroup);
}
```

- Loads all layer definitions from the `layers` section of `appsettings.json`.
- Caches layer data in memory.
- Supports querying by layer name or resource group.

### 7.2 SourceStore

```csharp
public class SourceStore
{
    Task<Source> GetSourceAsync(string name);
    Task<Dictionary<string, Source>> GetSourcesAsync();
}
```

- Manages data source connection configurations.
- Each source definition includes connection parameters, source type, and source-specific options.

### 7.3 GridSetStore

```csharp
public class GridSetStore
{
    Task<GridSet> GetGridSetAsync(string name);
    Task<List<GridSet>> GetGridSetsAsync();
}
```

- Provides tile grid set definitions.
- Includes origin, scale denominators, tile dimensions, and CRS information.

### 7.4 StyleGroupStore

```csharp
public class StyleGroupStore
{
    Task<StyleGroup> GetStyleGroupAsync(string name);
}
```

- Manages grouped style definitions that can be referenced by layers.
- Enables sharing common style configurations across multiple layers.

---

## 8. Dynamic Compilation

### 8.1 CSharpDynamicCompiler

Located in `ZMap.DynamicCompiler`, this component enables runtime evaluation of CQL filter expressions:

```csharp
public class CSharpDynamicCompiler
{
    Expression<Func<TFeature, bool>> CompilePredicate<TFeature>(string cqlExpression);
}
```

**Technology**: Uses the Natasha library for on-the-fly C# compilation.

**Algorithm:**

1. **Parse**: Parse the CQL expression string into an AST.
2. **Code Generation**: Generate a C# lambda expression from the AST.
3. **Compilation**: Compile the generated C# code to a `Func<TFeature, bool>` delegate using Natasha.
4. **Caching**: Cache compiled delegates by their source expression hash.
5. **Execution**: Invoke the compiled delegate against each feature during rendering.

**CQL Expression Examples:**
```
population > 1000000
name LIKE '%River%'
type IN ('highway', 'primary')
YEAR(date) BETWEEN 2000 AND 2020
```

### 8.2 Performance Considerations

- Dynamic compilation occurs only once per unique CQL expression.
- The compiled delegate is cached and reused for all subsequent queries.
- Initial compilation has a warmup cost (~50-200ms depending on expression complexity), but subsequent evaluations are at native speed.
