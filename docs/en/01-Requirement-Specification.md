# Requirement Specification

**Project**: ZServer - Distributed Map Tile Server
**Author**: Lewis Zou
**Date**: 2026-07-27
**Version**: 1.26.727.236

## Change Log

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.26.727.236 | 2026-07-27 | Lewis Zou | Initial version |

---

## 1. Overview

ZServer is a high-performance distributed map tile server built on the Microsoft Orleans Actor model. It implements the Open Geospatial Consortium (OGC) Web Map Service (WMS) and Web Map Tile Service (WMTS) standards, as well as the XYZ tile protocol commonly used by web mapping libraries.

The system is designed to serve geospatial data at scale by leveraging Orleans' distributed actor model, where each tile request maps to a lightweight actor (grain). This architecture enables lock-free concurrent tile rendering, automatic load balancing across a cluster of machines, and transparent horizontal scalability.

### 1.1 Key Capabilities

- **OGC WMS 1.3.0**: GetCapabilities, GetMap, GetFeatureInfo operations
- **OGC WMTS 1.0.0**: GetCapabilities, GetTile operations
- **XYZ Tiles**: Slippy-map-compatible tile endpoint
- **Multiple Data Sources**: PostgreSQL/PostGIS, ShapeFile, Cloud-Optimized GeoTIFF (COG), GDAL-supported formats, remote WMTS sources
- **Dynamic Styling**: SLD (Styled Layer Descriptor) support and JSON-based style configuration
- **Authentication**: JWT-based access control with scope-based authorization policies
- **Caching**: File-system tile cache for hot tiles
- **Cluster Support**: Orleans-based clustering with ADO.NET PostgreSQL membership

### 1.2 Technology Stack

| Component | Technology |
|-----------|-----------|
| Runtime | .NET 10, ASP.NET Core |
| Distributed Framework | Microsoft Orleans 10 |
| Rendering | SkiaSharp |
| Coordinate Systems | ProjNET |
| Geospatial Data | NetTopologySuite |
| Data Sources | PostgreSQL/PostGIS, GDAL, ShapeFile |
| Caching | File-system based |
| Authentication | JWT Bearer |
| Logging | Serilog |
| Testing | xUnit, BenchmarkDotNet |

---

## 2. Functional Requirements

### 2.1 OGC WMS Service

| ID | Requirement | Priority |
|----|-------------|----------|
| WMS-01 | The system shall support the WMS GetCapabilities operation, returning an XML document describing available layers, coordinate reference systems, and supported formats | P0 |
| WMS-02 | The system shall support the WMS GetMap operation, rendering a map image for the specified bounding box, layers, style, SRS/CRS, size, and format | P0 |
| WMS-03 | The system shall support the WMS GetFeatureInfo operation, returning attribute information for features at a specified pixel location | P1 |
| WMS-04 | The system shall support multiple output image formats including PNG, JPEG, and GIF | P0 |
| WMS-05 | The system shall support EPSG:4326 and EPSG:3857 coordinate reference systems | P0 |
| WMS-06 | The system shall support CQL (Common Query Language) filters for feature-level queries | P1 |

### 2.2 OGC WMTS Service

| ID | Requirement | Priority |
|----|-------------|----------|
| WMTS-01 | The system shall support the WMTS GetCapabilities operation, returning an XML document describing tile matrix sets and layers | P0 |
| WMTS-02 | The system shall support the WMTS GetTile operation, returning a single tile image for the specified layer, tile matrix, and tile coordinates | P0 |
| WMTS-03 | The system shall support multiple tile matrix sets including GlobalCRS84Pixel and GoogleMapsCompatible | P0 |
| WMTS-04 | Each tile request shall map to an Orleans grain for distributed processing | P0 |

### 2.3 XYZ Tile Service

| ID | Requirement | Priority |
|----|-------------|----------|
| XYZ-01 | The system shall support XYZ-style tile requests in the format `/xyz/{layers}?x={x}&y={y}&z={z}` | P0 |
| XYZ-02 | XYZ requests shall be internally mapped to WMTS processing | P0 |
| XYZ-03 | Multiple layers shall be composited in a single tile request | P1 |

### 2.4 Configuration Management

| ID | Requirement | Priority |
|----|-------------|----------|
| CFG-01 | The system shall support JSON configuration files for layers, data sources, grid sets, and style groups | P0 |
| CFG-02 | Configuration shall be reloadable at runtime without service restart | P1 |
| CFG-03 | Environment variables prefixed with `ZSERVER_` shall override configuration values | P0 |

### 2.5 Authentication and Authorization

| ID | Requirement | Priority |
|----|-------------|----------|
| AUTH-01 | The system shall support JWT Bearer token authentication | P1 |
| AUTH-02 | The system shall support scope-based authorization policies | P1 |
| AUTH-03 | The authorization system shall be configurable via the `EnableAuthorization` toggle | P1 |

### 2.6 Caching

| ID | Requirement | Priority |
|----|-------------|----------|
| CACHE-01 | The system shall cache rendered tiles on the file system | P1 |
| CACHE-02 | Cache behavior shall be configurable per-layer or globally | P2 |

### 2.7 Health Monitoring

| ID | Requirement | Priority |
|----|-------------|----------|
| MON-01 | The system shall expose a health check endpoint at `/healthz` | P1 |
| MON-02 | The system shall expose an Orleans dashboard at `/dashboard` | P2 |

---

## 3. Business Rules

### 3.1 Layer Rendering Order

Layers are rendered in reverse order — the last layer in the configuration is rendered first (bottom of the stack), and the first layer is rendered last (top of the stack). This ensures the first-configured layer appears on top visually.

### 3.2 Permission Checking

When authorization is enabled, each GetMap and GetTile request undergoes permission verification against the requested layers. Access is granted only if the JWT token's scopes include all requested resource groups.

### 3.3 Coordinate System Validation

The system validates that the requested CRS/SRS is supported before processing any WMS GetMap request. If an unsupported CRS is specified, an appropriate error response is returned.

### 3.4 Cache Invalidation

The file-system cache uses a time-to-live (TTL) approach. Tiles older than the configured threshold are considered stale and are re-rendered on the next request. Cache entries have no built-in invalidation trigger — they expire solely by age.

### 3.5 Dynamic Compilation Security

CQL filter expressions are compiled at runtime using the Natasha dynamic compiler. Compiled expressions execute within the server process and have access to the full .NET runtime. Only trusted administrators should configure CQL filters.

---

## 4. Non-Functional Requirements

### 4.1 Performance and Scalability

| ID | Requirement | Metric |
|----|-------------|--------|
| NFR-01 | The system shall scale horizontally by adding more silo nodes | N/A |
| NFR-02 | Tile rendering shall utilize all available CPU cores | N/A |
| NFR-03 | The system shall support concurrent tile requests without lock contention | Orleans actor model ensures single-threaded access per grain |
| NFR-04 | Cached tiles shall be served directly from the file system without re-rendering | Sub-millisecond response for cached tiles |

### 4.2 High Availability

| ID | Requirement | Description |
|----|-------------|-------------|
| NFR-05 | The system shall support multi-node cluster deployment | Orleans silos form a cluster with automatic failure detection |
| NFR-06 | A failed silo node shall not cause service interruption | Orleans automatically reactivates grains on surviving nodes |
| NFR-07 | Cluster membership shall be managed through a PostgreSQL database | ADO.NET membership provider |

### 4.3 Reliability

| ID | Requirement | Description |
|----|-------------|-------------|
| NFR-08 | The system shall handle invalid WMS/WMTS parameters gracefully | Return OGC-compliant exception XML or HTTP 400 |
| NFR-09 | The system shall log all errors and warnings via Serilog | Structured logging to file and console |

### 4.4 Maintainability

| ID | Requirement | Description |
|----|-------------|-------------|
| NFR-10 | The system shall follow a layered architecture | Separation of concerns: API → Grains → Configuration → Rendering → Data Sources |
| NFR-11 | The system shall support adding new data source types without modifying core rendering | IVectorSource and IRasterSource abstractions |

### 4.5 Security

| ID | Requirement | Description |
|----|-------------|-------------|
| NFR-12 | Authentication shall be enforced when enabled | JWT Bearer token validation on API endpoints |
| NFR-13 | Authorization scope checks shall prevent unauthorized layer access | Scope-based policy enforcement |
