# Test Plan

**Project**: ZServer - Distributed Map Tile Server
**Author**: Lewis Zou
**Date**: 2026-07-27
**Version**: 1.26.727.236

## Change Log

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.26.727.236 | 2026-07-27 | Lewis Zou | Initial version |

---

## 1. Unit Test Scope

ZServer includes an extensive test suite covering all major subsystems. The test project (`ZServer.Tests`) contains over 50 xUnit test files organized by functional area.

### 1.1 Test Project Structure

```
ZServer.Tests/
├── API/
│   ├── WMSTests.cs
│   ├── WMTSTests.cs
│   ├── XYZTests.cs
│   └── ControllerTests.cs
├── Coordinate/
│   ├── CoordinateTransformTests.cs
│   └── ProjectionTests.cs
├── Layer/
│   ├── LayerConfigLoadTests.cs
│   ├── LayerQueryTests.cs
│   └── LayerPermissionTests.cs
├── Renderer/
│   ├── RendererBasicTest.cs
│   ├── VectorRendererTests.cs
│   ├── RasterRendererTests.cs
│   └── StyleApplicationTests.cs
├── Grid/
│   ├── GridSetCalculationTests.cs
│   └── TileGridMathTests.cs
├── Source/
│   ├── PostgreSourceTests.cs
│   ├── ShapeFileSourceTests.cs
│   ├── GDALSourceTests.cs
│   └── COGSourceTests.cs
├── Cache/
│   └── FileCacheTests.cs
├── SLD/
│   └── SLDParsingTests.cs
├── Compiler/
│   └── DynamicCompilerTests.cs
├── Performance/
│   └── RenderingBenchmarks.cs
├── Integration/
│   ├── WMSIntegrationTests.cs
│   └── WMTSIntegrationTests.cs
└── Utilities/
    ├── GeometryHelperTests.cs
    └── ConfigLoadTests.cs
```

---

## 2. Key Test Cases

### 2.1 Coordinate Transform Tests

**Purpose:** Verify correctness of coordinate transformations between CRS systems.

| Test ID | Test Case | Input | Expected Result |
|---------|-----------|-------|-----------------|
| CRS-01 | Transform EPSG:4326 to EPSG:3857 | Point(116.4, 39.9) | Point(12958194.8, 4865942.3) within tolerance |
| CRS-02 | Transform EPSG:3857 to EPSG:4326 | Point(12958194.8, 4865942.3) | Point(116.4, 39.9) within tolerance |
| CRS-03 | Transform envelope bounds | BBOX in EPSG:4326 | Correct envelope in EPSG:3857 |
| CRS-04 | Identity transform (same CRS) | Any geometry | Geometry unchanged |
| CRS-05 | Invalid CRS code | "EPSG:99999" | Exception thrown |
| CRS-06 | Null/empty CRS input | null | Guard clause triggered |

### 2.2 Tile Grid Calculation Tests

**Purpose:** Verify tile grid mathematics for all supported grid sets.

| Test ID | Test Case | Input | Expected Result |
|---------|-----------|-------|-----------------|
| GRD-01 | Tile bounds at zoom 0, GoogleMapsCompatible | Tile(0,0,0) | World bounding box (-180,-85,180,85) in EPSG:4326 or global in EPSG:3857 |
| GRD-02 | Tile bounds at high zoom | Tile(512,768,10) | Correct geographic bounds |
| GRD-03 | Tile matrix dimensions | GoogleMapsCompatible zoom 10 | 1024×1024 tiles |
| GRD-04 | Scale denominator calculation | GoogleMapsCompatible zoom 10 | Correct scale value |
| GRD-05 | Pixel-to-tile coordinate conversion | Point + zoom level | Correct tile x, y |
| GRD-06 | GlobalCRS84Pixel bounds | Zoom 0 | Full world in EPSG:4326 |

### 2.3 Layer Configuration Loading Tests

**Purpose:** Verify configuration loading from JSON is correct and handles edge cases.

| Test ID | Test Case | Input | Expected Result |
|---------|-----------|-------|-----------------|
| CFG-01 | Load all layers from valid JSON | Complete config JSON | All layers loaded correctly |
| CFG-02 | Load single layer by name | Layer name string | Correct layer object returned |
| CFG-03 | Layer not found | Non-existent layer name | null or empty result |
| CFG-04 | Malformed JSON | Invalid JSON | Parse exception with clear message |
| CFG-05 | Missing required fields | JSON missing "name" | Configuration error |
| CFG-06 | Empty layers array | `{ "layers": [] }` | Empty list, no error |
| CFG-07 | Duplicate layer names | Two layers with same name | Last definition wins or error (deterministic behavior) |
| CFG-08 | Environment variable override | `ZSERVER_` env var set | Override applied correctly |

### 2.4 Renderer Tests

**Purpose:** Verify the SkiaSharp rendering pipeline produces correct visual output.

| Test ID | Test Case | Input | Expected Result |
|---------|-----------|-------|-----------------|
| RND-01 | Render empty map | Map with no layers | Transparent or white image of requested size |
| RND-02 | Render single polygon feature | Single polygon with fill style | Correctly filled polygon on output image |
| RND-03 | Render multiple overlapping polygons | Two overlapping polygons | Correct z-order: second polygon on top |
| RND-04 | Render line feature | Line with stroke style | Correctly rendered line with specified width and color |
| RND-05 | Render point feature | Point with mark style | Correctly rendered symbol at point location |
| RND-06 | Render with opacity | Fill style with 50% opacity | Semi-transparent rendering |
| RND-07 | Label rendering | Point with label style | Label rendered at correct position |
| RND-08 | Output to PNG format | Map render with PNG | Valid PNG image bytes |
| RND-09 | Output to JPEG format | Map render with JPEG | Valid JPEG image bytes |
| RND-10 | Output image dimensions | 800×600 request | Image exactly 800×600 pixels |

### 2.5 Data Source Tests

**Purpose:** Verify data source connectivity and query correctness.

| Test ID | Test Case | Input | Expected Result |
|---------|-----------|-------|-----------------|
| SRC-01 | PostGIS bounding box query | Envelope + layer config | Features intersecting bounds returned |
| SRC-02 | PostGIS with CQL filter | Bounds + `population > 1000000` | Filtered features returned |
| SRC-03 | PostGIS empty result | Bounds in ocean | Empty feature list |
| SRC-04 | ShapeFile load and query | ShapeFile path + bounds | Features loaded from file |
| SRC-05 | GDAL raster query | GeoTIFF path + bounds | Image bytes for the requested area |
| SRC-06 | COG optimized read | COG file path + bounds | Image bytes using optimized HTTP range requests |
| SRC-07 | Remote WMTS source proxy | WMTS source config + tile params | Tile image from remote service |

### 2.6 API Integration Tests

**Purpose:** End-to-end tests of API controllers.

| Test ID | Test Case | Input | Expected Result |
|---------|-----------|-------|-----------------|
| API-01 | WMS GetMap success | Valid WMS parameters | HTTP 200 with image bytes |
| API-02 | WMS GetMap missing parameter | No BBOX parameter | HTTP 400 with OGC exception XML |
| API-03 | WMS GetMap invalid BBOX | `BBOX=abc` | HTTP 400 with OGC exception |
| API-04 | WMS GetMap unknown layer | Non-existent layer name | HTTP 400 with LayerNotDefined exception |
| API-05 | WMS GetFeatureInfo success | Valid GetFeatureInfo params | HTTP 200 with feature attributes |
| API-06 | WMTS GetTile success | Valid WMTS parameters | HTTP 200 with tile image |
| API-07 | WMTS GetTile cache hit | Same tile requested twice | Second request served from cache |
| API-08 | XYZ GetTile success | Valid XYZ parameters | HTTP 200 with tile image |
| API-09 | Health check endpoint | GET /healthz | HTTP 200, body "Healthy" |
| API-10 | CRS authority lookup | POST with valid EPSG code | HTTP 200 with JSON response |

### 2.7 Cache Tests

**Purpose:** Verify file-system cache behavior.

| Test ID | Test Case | Input | Expected Result |
|---------|-----------|-------|-----------------|
| CACHE-01 | Cache miss then hit | Request tile twice | First: rendered from source. Second: served from cache |
| CACHE-02 | Cache directory creation | First tile request | Cache directory structure created |
| CACHE-03 | Cache TTL expiration | Request after TTL expires | Tile re-rendered (not served from stale cache) |
| CACHE-04 | Concurrent cache access | Multiple simultaneous requests for same tile | One renders, rest read from cache (no duplicate work) |
| CACHE-05 | Invalid cached file | Corrupted cache file | Cache treated as miss, tile re-rendered |

### 2.8 SLD Parsing Tests

**Purpose:** Verify SLD XML parsing produces correct style objects.

| Test ID | Test Case | Input | Expected Result |
|---------|-----------|-------|-----------------|
| SLD-01 | Parse full SLD document | Valid SLD XML | Correct style objects created |
| SLD-02 | Parse point style | SLD with PointSymbolizer | MarkStyle with correct parameters |
| SLD-03 | Parse line style | SLD with LineSymbolizer | LineStyle with correct parameters |
| SLD-04 | Parse polygon style | SLD with PolygonSymbolizer | FillStyle with correct parameters |
| SLD-05 | Parse text style | SLD with TextSymbolizer | LabelStyle with correct parameters |
| SLD-06 | Invalid SLD XML | Malformed XML | Parse exception |
| SLD-07 | Empty SLD document | `<StyledLayerDescriptor/>` | Empty style list |

### 2.9 Dynamic Compilation Tests

**Purpose:** Verify CQL expression compilation and evaluation.

| Test ID | Test Case | Input | Expected Result |
|---------|-----------|-------|-----------------|
| COMP-01 | Numeric comparison | `population > 1000000` | Correct boolean evaluation |
| COMP-02 | String comparison | `name = 'Beijing'` | Correct boolean evaluation |
| COMP-03 | LIKE operator | `name LIKE '%River%'` | Correct pattern matching |
| COMP-04 | IN operator | `type IN ('highway', 'primary')` | Correct set membership |
| COMP-05 | AND/OR composition | `population > 1000000 AND area > 5000` | Correct logical combination |
| COMP-06 | BETWEEN operator | `YEAR(date) BETWEEN 2000 AND 2020` | Correct range evaluation |
| COMP-07 | Compilation caching | Same expression twice | Second evaluation uses cached delegate |
| COMP-08 | Invalid expression syntax | `population >>> 1000000` | Compilation error with message |

### 2.10 Performance Benchmarks

BenchmarkDotNet performance tests are in the `ZServer.Benchmark` project:

| Benchmark | Measurement | Description |
|-----------|-------------|-------------|
| MapRenderBenchmark | Time per render (ms) | End-to-end map rendering time |
| TileGridBenchmark | Time per calculation (μs) | Tile grid bound calculation throughput |
| CoordinateTransformBenchmark | Time per transform (μs) | Coordinate transform throughput |
| CQLCompilationBenchmark | Time per compilation (ms) | Dynamic expression compilation time |
| CQLExecutionBenchmark | Time per evaluation (μs) | Compiled expression evaluation throughput |

---

## 3. Test Data Preparation

### 3.1 Configuration Files

Test configurations are stored as embedded resources in the test project:

```
ZServer.Tests/TestData/
├── config/
│   ├── valid_config.json
│   ├── minimal_config.json
│   ├── malformed_config.json
│   └── extended_config.json
├── shapefiles/
│   ├── test_polygons.shp
│   ├── test_lines.shp
│   └── test_points.shp
├── sld/
│   ├── polygon_style.xml
│   ├── line_style.xml
│   ├── point_style.xml
│   └── invalid_sld.xml
└── tiles/
    └── sample_tile.png
```

### 3.2 ShapeFile Test Data

Test shapefiles are included in the repository under `ZServer.Tests/TestData/shapefiles/`. These contain small synthetic geometries suitable for unit testing:

- **test_polygons.shp**: 5 polygon features with varying attributes
- **test_lines.shp**: 3 line features with different lengths
- **test_points.shp**: 10 point features distributed across the test area

### 3.3 PostGIS Test Database

For PostGIS source tests, a test database is expected at:

```
Host=localhost;Database=zserver_test;Username=zserver_test;Password=zserver_test
```

The test database can be populated using the included SQL scripts in `ZServer.Tests/TestData/sql/`.

### 3.4 Test Fixtures

The test project uses xUnit's `IClassFixture` pattern for shared test fixtures:

| Fixture | Purpose |
|---------|---------|
| `ConfigFixture` | Loads and caches test configuration |
| `DataFixture` | Manages test data lifecycle |
| `RendererFixture` | Creates shared SkiaSharp surfaces and canvases |

---

## 4. Test Execution

### 4.1 Running Tests

```bash
# Run all tests
dotnet test src/ZServer.Tests/ZServer.Tests.csproj

# Run tests with verbose output
dotnet test src/ZServer.Tests/ZServer.Tests.csproj -v n

# Run specific test class
dotnet test src/ZServer.Tests/ZServer.Tests.csproj --filter "FullyQualifiedName~GridSetCalculationTests"

# Run benchmarks
dotnet run --project src/ZServer.Benchmark -c Release
```

### 4.2 Test Coverage Goals

| Module | Target Coverage | Current Status |
|--------|----------------|----------------|
| Coordinate transforms | ≥90% | Implemented |
| Tile grid math | ≥95% | Implemented |
| Layer config loading | ≥85% | Implemented |
| Rendering pipeline | ≥80% | Implemented |
| Data sources | ≥70% | Implemented |
| API controllers | ≥85% | Implemented |
| Cache | ≥90% | Implemented |
| SLD parsing | ≥90% | Implemented |
| Dynamic compiler | ≥90% | Implemented |
| **Overall** | **≥85%** | **In progress** |
