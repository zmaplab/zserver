# API Documentation

**Project**: ZServer - Distributed Map Tile Server
**Author**: Lewis Zou
**Date**: 2026-07-27
**Version**: 1.26.727.236

## Change Log

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.26.727.236 | 2026-07-27 | Lewis Zou | Initial version |

---

## 1. WMS Service

Endpoints implementing the OGC Web Map Service (WMS) 1.3.0 specification.

### Base URL

```
GET /wms
```

### 1.1 GetCapabilities

Returns an XML document describing the server's available layers, coordinate reference systems, supported formats, and other metadata.

**Request Parameters:**

| Parameter | Required | Description | Default |
|-----------|----------|-------------|---------|
| `SERVICE` | Yes | Must be `WMS` | — |
| `REQUEST` | Yes | Must be `GetCapabilities` | — |
| `VERSION` | No | WMS version. Supported: `1.3.0` | `1.3.0` |

**Example:**

```
GET /wms?SERVICE=WMS&REQUEST=GetCapabilities
```

**Response:** `application/xml`

Returns an XML document conforming to the OGC WMS 1.3.0 capabilities schema.

---

### 1.2 GetMap

Renders a map image for the specified parameters.

**Request Parameters:**

| Parameter | Required | Description | Default |
|-----------|----------|-------------|---------|
| `SERVICE` | Yes | Must be `WMS` | — |
| `REQUEST` | Yes | Must be `GetMap` | — |
| `VERSION` | No | WMS version | `1.3.0` |
| `LAYERS` | Yes | Comma-separated list of layer names | — |
| `STYLES` | No | Comma-separated list of style names | Empty (default style) |
| `CRS` / `SRS` | Yes | Coordinate reference system (e.g., `EPSG:4326`, `EPSG:3857`) | — |
| `BBOX` | Yes | Bounding box: `minx,miny,maxx,maxy` in CRS units | — |
| `WIDTH` | Yes | Output image width in pixels | — |
| `HEIGHT` | Yes | Output image height in pixels | — |
| `FORMAT` | Yes | Output image format. Supported: `image/png`, `image/jpeg`, `image/gif` | — |
| `TRANSPARENT` | No | Whether the background should be transparent (`TRUE`/`FALSE`) | `FALSE` |
| `BGCOLOR` | No | Background color as hex (e.g., `0xFFFFFF`) | `0xFFFFFF` |
| `EXCEPTIONS` | No | Exception format | `application/vnd.ogc.se_xml` |
| `CQL_FILTER` | No | Common Query Language filter expression | — |

**Example:**

```
GET /wms?SERVICE=WMS&REQUEST=GetMap&VERSION=1.3.0
    &LAYERS=admin_boundaries,roads
    &STYLES=boundary_style,road_style
    &CRS=EPSG:4326
    &BBOX=116.0,39.5,117.0,40.5
    &WIDTH=800&HEIGHT=600
    &FORMAT=image/png
    &TRANSPARENT=TRUE
```

**Response:**

| Status Code | Content-Type | Description |
|-------------|--------------|-------------|
| `200` | `image/png`, `image/jpeg`, or `image/gif` | Rendered map image |
| `400` | `application/vnd.ogc.se_xml` | Invalid parameters (OGC exception XML) |
| `403` | `application/json` | Access denied (when authorization enabled) |

**OGC Exception XML Example:**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<ServiceExceptionReport version="1.3.0">
  <ServiceException code="InvalidParameterValue">
    Unknown layer: non_existent_layer
  </ServiceException>
</ServiceExceptionReport>
```

---

### 1.3 GetFeatureInfo

Returns attribute information about features at a specified pixel location on a previously rendered map.

**Request Parameters:**

| Parameter | Required | Description | Default |
|-----------|----------|-------------|---------|
| `SERVICE` | Yes | Must be `WMS` | — |
| `REQUEST` | Yes | Must be `GetFeatureInfo` | — |
| `VERSION` | No | WMS version | `1.3.0` |
| `LAYERS` | Yes | Comma-separated list of rendered layer names | — |
| `QUERY_LAYERS` | Yes | Comma-separated list of layers to query | — |
| `STYLES` | No | Style names matching the GetMap request | — |
| `CRS` / `SRS` | Yes | Coordinate reference system | — |
| `BBOX` | Yes | Bounding box matching the GetMap request | — |
| `WIDTH` | Yes | Map image width matching the GetMap request | — |
| `HEIGHT` | Yes | Map image height matching the GetMap request | — |
| `X` | Yes | Pixel column (from left) | — |
| `Y` | Yes | Pixel row (from top) | — |
| `INFO_FORMAT` | Yes | Output format. Supported: `text/html`, `text/plain`, `application/vnd.ogc.gml` | — |
| `FEATURE_COUNT` | No | Maximum number of features to return | `1` |

**Example:**

```
GET /wms?SERVICE=WMS&REQUEST=GetFeatureInfo&VERSION=1.3.0
    &LAYERS=admin_boundaries
    &QUERY_LAYERS=admin_boundaries
    &CRS=EPSG:4326
    &BBOX=116.0,39.5,117.0,40.5
    &WIDTH=800&HEIGHT=600
    &X=400&Y=300
    &INFO_FORMAT=text/html
    &FEATURE_COUNT=5
```

**Response (text/html):**
```html
<HTML>
  <BODY>
    <TABLE>
      <TR><TH>Feature</TH><TH>Value</TH></TR>
      <TR><TD>name</TD><TD>Beijing</TD></TR>
      <TR><TD>admin_level</TD><TD>1</TD></TR>
      <TR><TD>population</TD><TD>21540000</TD></TR>
    </TABLE>
  </BODY>
</HTML>
```

---

## 2. WMTS Service

Endpoints implementing the OGC Web Map Tile Service (WMTS) 1.0.0 specification.

### Base URL

```
GET /wmts
```

### 2.1 GetCapabilities

Returns an XML document describing available tile matrix sets, layers, and supported formats.

**Request Parameters:**

| Parameter | Required | Description |
|-----------|----------|-------------|
| `SERVICE` | Yes | Must be `WMTS` |
| `REQUEST` | Yes | Must be `GetCapabilities` |

**Example:**

```
GET /wmts?SERVICE=WMTS&REQUEST=GetCapabilities
```

### 2.2 GetTile

Returns a single tile image.

**Request Parameters:**

| Parameter | Required | Description |
|-----------|----------|-------------|
| `SERVICE` | Yes | Must be `WMTS` |
| `REQUEST` | Yes | Must be `GetTile` |
| `VERSION` | No | WMTS version | `1.0.0` |
| `LAYER` | Yes | Layer name | — |
| `STYLE` | No | Style name | Default style |
| `FORMAT` | Yes | Output format (`image/png`, `image/jpeg`) | — |
| `TILEMATRIXSET` | Yes | Tile matrix set (`GlobalCRS84Pixel`, `GoogleMapsCompatible`) | — |
| `TILEMATRIX` | Yes | Tile matrix (zoom level) | — |
| `TILEROW` | Yes | Tile row index | — |
| `TILECOL` | Yes | Tile column index | — |

**Example:**

```
GET /wmts?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0
    &LAYER=admin_boundaries
    &STYLE=default
    &FORMAT=image/png
    &TILEMATRIXSET=GoogleMapsCompatible
    &TILEMATRIX=10
    &TILEROW=512
    &TILECOL=768
```

**Response:**

| Status Code | Content-Type | Description |
|-------------|--------------|-------------|
| `200` | `image/png` or `image/jpeg` | Tile image (256×256 or configured size) |
| `400` | XML | Invalid parameters |
| `404` | — | Tile not found in configured bounds |

---

## 3. XYZ Tile Service

RESTful tile endpoint compatible with standard web mapping libraries.

### Base URL

```
GET /xyz/{layers}?x={x}&y={y}&z={z}
```

### 3.1 Get Tile

**Path Parameters:**

| Parameter | Description |
|-----------|-------------|
| `{layers}` | Comma-separated layer names (e.g., `admin_boundaries,roads`) |

**Query Parameters:**

| Parameter | Required | Description | Default |
|-----------|----------|-------------|---------|
| `x` | Yes | Tile column index | — |
| `y` | Yes | Tile row index | — |
| `z` | Yes | Zoom level | — |
| `format` | No | Output format | `image/png` |

**Example:**

```
GET /xyz/admin_boundaries?x=512&y=768&z=10&format=image/png
```

**Response:** Tile image (256×256 pixels).

---

## 4. Tools API

### 4.1 CRS Authority Lookup

Resolves CRS authority codes and returns projection details.

**Endpoint:**

```
POST /api/v1.0/tools/crs_authority
```

**Request Body:**
```json
{
  "authority": "EPSG",
  "code": 4326
}
```

**Response:**
```json
{
  "authority": "EPSG",
  "code": 4326,
  "name": "WGS 84",
  "isGeographic": true,
  "unit": "degree",
  "areaOfUse": "World",
  "srid": 4326
}
```

---

## 5. Health Check

### 5.1 Health Endpoint

**Endpoint:**

```
GET /healthz
```

**Response (200 OK):**
```
Healthy
```

Returns a simple `200 OK` response with body `Healthy` when the server is operational and all dependencies are reachable.

---

## 6. Orleans Dashboard

### 6.1 Dashboard Endpoint

**Endpoint:**

```
GET /dashboard
```

**Response:** HTML page with Orleans cluster monitoring dashboard, showing:

- Active silos and their status
- Grain activation counts per type
- Request queue depth and processing rates
- Memory usage per silo

The dashboard is provided by the Orleans framework and requires the Orleans Dashboard NuGet package to be enabled.

---

## 7. Authentication

When authorization is enabled in configuration, all WMS, WMTS, XYZ, and Tools endpoints require a valid JWT Bearer token.

### 7.1 Request Format

```
Authorization: Bearer <token>
```

### 7.2 Scope-Based Authorization

The JWT token should contain a `scope` claim with resource group identifiers. Access is granted only when the token's scopes include the resource groups for all requested layers.

### 7.3 Configuration

```json
{
  "authentication": {
    "enableAuthorization": true,
    "authority": "https://your-auth-server.com",
    "audience": "zserver-api",
    "metadataAddress": "https://your-auth-server.com/.well-known/openid-configuration"
  }
}
```

---

## 8. Error Responses

### 8.1 HTTP Status Code Summary

| Status Code | Meaning | Typical Cause |
|-------------|---------|---------------|
| `200` | Success | Request processed successfully |
| `400` | Bad Request | Missing or invalid parameters |
| `401` | Unauthorized | Missing or invalid authentication |
| `403` | Forbidden | Insufficient permissions (correct auth but wrong scope) |
| `404` | Not Found | Layer or tile not found |
| `500` | Internal Server Error | Rendering failure or unhandled exception |

### 8.2 OGC Exception XML Format

WMS/WMTS operations return OGC-compliant exception XML for parameter errors:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<ServiceExceptionReport version="1.3.0">
  <ServiceException code="MissingParameterValue">
    Missing required parameter: BBOX
  </ServiceException>
</ServiceExceptionReport>
```

Standard `code` values:
- `InvalidParameterValue`
- `MissingParameterValue`
- `NoApplicableCode`
- `LayerNotDefined`
- `StyleNotDefined`
- `CurrentUpdateSequence`
- `InvalidUpdateSequence`
