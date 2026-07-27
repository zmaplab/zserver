# Deployment & User Manual

**Project**: ZServer - Distributed Map Tile Server
**Author**: Lewis Zou
**Date**: 2026-07-27
**Version**: 1.26.727.236

## Change Log

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.26.727.236 | 2026-07-27 | Lewis Zou | Initial version |

---

## 1. Environment Requirements

### 1.1 Hardware Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| **CPU** | 4 cores | 8+ cores (rendering is CPU-bound) |
| **RAM** | 4 GB | 16 GB+ (more for large raster datasets) |
| **Disk** | 20 GB | 100 GB+ SSD (tile cache grows with usage) |
| **Network** | 100 Mbps | 1 Gbps |

### 1.2 Software Requirements

| Software | Version | Purpose |
|----------|---------|---------|
| .NET SDK | 10.0+ | Runtime and build tooling |
| PostgreSQL | 14+ (with PostGIS 3+) | Spatial data storage (optional) |
| GDAL | 3.5+ | Raster data source access (optional) |
| Docker | 24+ | Containerized deployment |
| Docker Compose | 2.20+ | Orchestrated deployment |

### 1.3 Supported Platforms

- **Linux** (x86_64, arm64): Production recommended
- **macOS**: Development only
- **Windows**: Development only
- **Docker**: Cross-platform container images available

---

## 2. Configuration Files

### 2.1 Main Configuration File

**Location:** `conf/appsettings.json`

**Full structure:**

```json
{
  "PostgreSQLConnectionString": "Host=localhost;Database=geodata;Username=zserver;Password=...",

  "authentication": {
    "enableAuthorization": false,
    "authority": "https://your-auth-server.com",
    "audience": "zserver-api",
    "metadataAddress": "https://your-auth-server.com/.well-known/openid-configuration"
  },

  "orleans": {
    "connectionString": "Host=localhost;Database=orleans;Username=zserver;Password=...",
    "clusterId": "zserver-cluster",
    "serviceId": "zserver"
  },

  "cache": {
    "directory": "./cache/tiles",
    "expirationMinutes": 60
  },

  "layers": [
    {
      "name": "example_layer",
      "title": "Example Layer",
      "source": "example_source",
      "sourceType": "Postgre",
      "crs": "EPSG:4326",
      "styles": ["default_style"],
      "minZoom": 0,
      "maxZoom": 18,
      "enabled": true
    }
  ],

  "sources": {
    "example_source": {
      "type": "Postgre",
      "connectionString": "Host=localhost;Database=geodata;Username=zserver;Password=...",
      "table": "public.example_table",
      "geometryColumn": "geometry",
      "srid": 4326,
      "keyColumn": "id",
      "attributes": ["name", "category"]
    }
  },

  "gridSets": {
    "GlobalCRS84Pixel": {
      "crs": "EPSG:4326",
      "origin": [-180, 90],
      "tileWidth": 256,
      "tileHeight": 256,
      "resolutions": [
        0.703125, 0.3515625, 0.17578125, 0.087890625,
        0.0439453125, 0.02197265625, 0.010986328125, 0.0054931640625,
        0.00274658203125, 0.001373291015625, 0.0006866455078125,
        0.00034332275390625, 0.000171661376953125, 0.0000858306884765625,
        0.00004291534423828125, 0.000021457672119140625, 0.0000107288360595703125,
        0.00000536441802978515625, 0.000002682209014892578125, 0.0000013411045074462890625
      ]
    },
    "GoogleMapsCompatible": {
      "crs": "EPSG:3857",
      "origin": [-20037508.342789244, 20037508.342789244],
      "tileWidth": 256,
      "tileHeight": 256,
      "resolutions": [
        156543.033928041, 78271.5169640205, 39135.7584820102,
        19567.8792410051, 9783.93962050256, 4891.96981025128,
        2445.98490512564, 1222.99245256282, 611.49622628141,
        305.748113140705, 152.874056570353, 76.4370282851763,
        38.2185141425881, 19.1092570712941, 9.55462853564703,
        4.77731426782352, 2.38865713391176, 1.19432856695588,
        0.59716428347794, 0.29858214173897
      ]
    }
  },

  "styleGroups": {
    "default_style": {
      "fill": { "color": "#3388ff", "opacity": 0.7 },
      "stroke": { "color": "#ffffff", "width": 2 }
    }
  },

  "resourceGroups": [
    {
      "name": "public",
      "layers": ["example_layer"]
    }
  ]
}
```

### 2.2 Logging Configuration

**Location:** `conf/serilog.json`

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Orleans": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/zserver-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

---

## 3. Environment Variables

All configuration values can be overridden via environment variables prefixed with `ZSERVER_`. Nested keys use `__` (double underscore) separator.

### 3.1 Essential Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `ZSERVER_PostgreSQLConnectionString` | PostGIS database connection | `Host=db;Database=geodata;User=...` |
| `ZSERVER_Orleans__ConnectionString` | Orleans cluster DB connection | `Host=db;Database=orleans;User=...` |
| `ZSERVER_Orleans__ClusterId` | Orleans cluster identifier | `zserver-prod` |
| `ZSERVER_Standalone` | Run in standalone mode | `true` or `false` |
| `ZSERVER_Port` | HTTP listen port | `8200` |
| `ZSERVER_Authentication__EnableAuthorization` | Enable JWT auth | `true` or `false` |

### 3.2 Silo Configuration Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `ZSERVER_ClusterSiloPort` | Silo-to-silo communication port | `10001` |
| `ZSERVER_ClusterGatewayPort` | Client gateway port | `20001` |

---

## 4. Deployment Options

### 4.1 Docker Deployment (Recommended)

**Build the API image:**

```bash
docker build -f API.Dockerfile -t zserver-api .
```

**For ARM64 architecture:**

```bash
docker build -f API_ARM.Dockerfile -t zserver-api:arm64 .
```

**Run standalone (single container):**

```bash
docker run -d \
  --name zserver \
  -p 8200:8200 \
  -v /data/config:/app/conf \
  -v /data/cache:/app/cache \
  -e ZSERVER_PostgreSQLConnectionString="Host=host.docker.internal;Database=geodata;Username=zserver;Password=..." \
  -e ZSERVER_Standalone=true \
  -e ZSERVER_Port=8200 \
  zserver-api
```

### 4.2 Docker Compose Deployment

A `docker-compose.yml` file is included for production-like deployments:

```yaml
services:
  zserver:
    build:
      context: .
      dockerfile: API.Dockerfile
    ports:
      - "8200:8200"
    environment:
      - ZSERVER_PostgreSQLConnectionString=Host=postgis;Database=geodata;Username=zserver;Password=...
      - ZSERVER_Orleans__ConnectionString=Host=orleans-db;Database=orleans;Username=zserver;Password=...
      - ZSERVER_Standalone=true
      - ZSERVER_Port=8200
    volumes:
      - ./conf:/app/conf
      - tile-cache:/app/cache
    depends_on:
      - postgis
      - orleans-db

  postgis:
    image: postgis/postgis:16-3.4
    environment:
      - POSTGRES_USER=zserver
      - POSTGRES_PASSWORD=...
      - POSTGRES_DB=geodata
    volumes:
      - postgis-data:/var/lib/postgresql/data

  orleans-db:
    image: postgres:16
    environment:
      - POSTGRES_USER=zserver
      - POSTGRES_PASSWORD=...
      - POSTGRES_DB=orleans
    volumes:
      - orleans-data:/var/lib/postgresql/data

volumes:
  postgis-data:
  orleans-data:
  tile-cache:
```

**Start all services:**

```bash
docker-compose up -d
```

### 4.3 Standalone Mode (Development)

Run the API host with an in-process Orleans silo:

```bash
dotnet run --project src/ZServer.API --Standalone true --Port 8200
```

### 4.4 Cluster Mode (Production)

**Step 1: Start silo nodes**

On each silo machine:

```bash
dotnet run --project src/ZServer.SiloHost \
  --ClusterSiloPort 10001 \
  --ClusterGatewayPort 20001
```

**Step 2: Start API gateways**

On each API gateway machine:

```bash
dotnet run --project src/ZServer.API \
  --Standalone false \
  --Port 8100
```

Silos and API gateways must share the same Orleans database for cluster membership.

### 4.5 Manual Build and Publish

```bash
# Build the solution
dotnet build ZServer.sln -c Release

# Publish the API
dotnet publish src/ZServer.API/ZServer.API.csproj -c Release -o ./publish/api

# Publish the SiloHost
dotnet publish src/ZServer.SiloHost/ZServer.SiloHost.csproj -c Release -o ./publish/silo

# Run from publish output
cd ./publish/api && dotnet ZServer.API.dll --Standalone true --Port 8200
```

---

## 5. Data Preparation

### 5.1 PostGIS Data

**Step 1: Load spatial data into PostgreSQL/PostGIS**

```bash
# Using ogr2ogr (GDAL)
ogr2ogr -f "PostgreSQL" PG:"host=localhost dbname=geodata user=zserver" \
  /path/to/your/data.shp -lco GEOMETRY_NAME=geometry -lco SRID=4326

# Using shp2pgsql
shp2pgsql -s 4326 -I /path/to/your/data.shp public.table_name | psql -h localhost -U zserver -d geodata
```

**Step 2: Create spatial indexes**

```sql
CREATE INDEX CONCURRENTLY idx_table_geometry
  ON public.table_name USING GIST (geometry);
```

**Step 3: Configure the source in ZServer**

Add the source definition to `conf/appsettings.json` (see Section 2.1).

### 5.2 ShapeFile Data

Place shapefiles in a directory accessible to ZServer and configure the source:

```json
{
  "sources": {
    "my_shapefile": {
      "type": "ShapeFile",
      "path": "/data/shapefiles/countries.shp",
      "encoding": "UTF-8"
    }
  }
}
```

### 5.3 Cloud-Optimized GeoTIFF (COG)

COG files can be stored locally or on HTTP servers:

```json
{
  "sources": {
    "my_cog": {
      "type": "COG",
      "path": "/data/imagery/satellite.tif"
    }
  }
}
```

### 5.4 Remote WMTS Source

Configure a WMTS service as a tile layer:

```json
{
  "sources": {
    "my_remote_wmts": {
      "type": "WMTS",
      "url": "https://tiles.example.com/wmts",
      "layer": "satellite",
      "tileMatrixSet": "GoogleMapsCompatible"
    }
  }
}
```

---

## 6. Frontend Deployment

The frontend is a Leaflet-based web application built with Parcel.

### 6.1 Build the Frontend

```bash
cd src/Web
npm install
npx parcel build index.html
```

### 6.2 Deploy the Frontend

The built files will be in `src/Web/dist/`. Serve them via any static file server or integrate with Nginx:

```nginx
server {
    listen 80;
    server_name maps.example.com;

    location / {
        root /var/www/zserver-frontend;
        index index.html;
    }
}
```

### 6.3 Frontend Configuration

Edit `src/Web/config.js` or set environment variables during build:

```javascript
window.ZSERVER_CONFIG = {
  wmsUrl: "https://maps.example.com/wms",
  xyzUrl: "https://maps.example.com/xyz",
  defaultLayers: ["admin_boundaries"]
};
```

---

## 7. Operations Guide

### 7.1 Starting the Server

```bash
# Standalone mode
dotnet run --project src/ZServer.API --Standalone true --Port 8200

# Verify health
curl http://localhost:8200/healthz
# Expected: Healthy
```

### 7.2 Verifying Tile Service

**Check WMS capabilities:**

```bash
curl "http://localhost:8200/wms?SERVICE=WMS&REQUEST=GetCapabilities" | head -50
```

**Request a WMS map:**

```bash
curl "http://localhost:8200/wms?SERVICE=WMS&REQUEST=GetMap&LAYERS=admin_boundaries&CRS=EPSG:4326&BBOX=116.0,39.5,117.0,40.5&WIDTH=800&HEIGHT=600&FORMAT=image/png" -o map.png
```

**Request an XYZ tile:**

```bash
curl "http://localhost:8200/xyz/admin_boundaries?x=512&y=768&z=10" -o tile.png
```

### 7.3 Monitoring

- **Health check**: `/healthz` returns `200 OK` when the server is operational.
- **Orleans Dashboard**: `/dashboard` shows cluster status (requires Orleans Dashboard package).
- **Logs**: Configured via Serilog — default to console + rolling file in `logs/` directory.
- **Metrics**: Can be exported via .NET metrics or Orleans telemetry.

### 7.4 Cache Management

```bash
# View cache size
du -sh ./cache/tiles

# Clear the tile cache (stops the server first)
rm -rf ./cache/tiles/*

# Adjust cache TTL in configuration
# "cache": { "expirationMinutes": 120 }
```

### 7.5 Configuration Reload

Changes to `conf/appsettings.json` may require a service restart, depending on the store implementation. For reliable configuration updates:

1. Update the configuration file
2. Restart the server: `docker restart zserver`

### 7.6 Scaling

**Vertical Scaling:** Increase CPU cores and RAM on the server machine. Rendering is CPU-bound, so more cores directly improve throughput.

**Horizontal Scaling:** Add more silo nodes:
1. Configure all nodes to use the same Orleans database
2. Start additional silos with unique silo ports
3. Orleans automatically distributes grain activations across the cluster

**Load Balancing:** Place a reverse proxy (Nginx, HAProxy) in front of multiple API gateways:

```nginx
upstream zserver_backend {
    least_conn;  # Load balance by least connections
    server api1.example.com:8100;
    server api2.example.com:8100;
    server api3.example.com:8100;
}

server {
    listen 80;
    server_name maps.example.com;
    location / {
        proxy_pass http://zserver_backend;
    }
}
```

---

## 8. FAQ

### 8.1 General

**Q: What operating systems are supported?**

A: Linux is recommended for production. macOS and Windows are suitable for development.

**Q: Does ZServer require a GPU?**

A: No. All rendering is CPU-based using SkiaSharp.

**Q: Can ZServer serve tiles to Google Maps or Leaflet?**

A: Yes. Use the XYZ endpoint with `GoogleMapsCompatible` grid set for standard slippy-map integration.

### 8.2 Performance

**Q: How many tiles per second can ZServer handle?**

A: This depends on your data complexity, layer count, and hardware. Cached tiles serve in <1ms. Uncached vector tiles typically render in 10-100ms. Run the benchmark project for hardware-specific numbers:

```bash
dotnet run --project src/ZServer.Benchmark -c Release
```

**Q: Why is tile rendering slow for my data?**

A: Common causes:
- Missing spatial indexes on PostGIS tables
- Complex geometries with high vertex counts
- Large raster datasets with many bands
- Slow data source connections

**Q: How much disk space does the tile cache use?**

A: This depends on usage patterns. For reference, a global dataset at zoom 0-10 generates approximately 1.1M tiles. At 20KB per tile, this is ~22GB.

### 8.3 Configuration

**Q: How do I add a new layer?**

A: 
1. Add a data source definition in the `sources` section
2. Add a layer definition in the `layers` array
3. (Optional) Add style definitions in `styleGroups`
4. Restart the server

**Q: Can I change configuration without restarting?**

A: Currently, configuration is loaded at startup. A restart is required for changes to take effect.

### 8.4 Data Sources

**Q: Can I connect to an existing PostGIS database?**

A: Yes. ZServer is designed to work with existing spatial databases. Configure the connection string, table name, and geometry column in the source configuration.

**Q: What CRS/SRS values are supported?**

A: Any CRS supported by ProjNET. Common values are EPSG:4326 (WGS 84) and EPSG:3857 (Web Mercator).

**Q: Can I use data from multiple databases?**

A: Yes. You can configure multiple PostGIS sources with different connection strings.

### 8.5 Authentication

**Q: How do I enable authentication?**

A: Set `"enableAuthorization": true` in the authentication section and configure your JWT authority.

**Q: What token format is required?**

A: Standard JWT Bearer tokens with a `scope` claim containing resource group identifiers.

### 8.6 Troubleshooting

**Q: The server starts but tiles return 404.**

A: Common causes:
- Layer not enabled in configuration (`"enabled": true`)
- Data source connection failure (check logs)
- Requested tile bounds are outside the configured layer bounds
- Coordinate reference system mismatch

**Q: The server throws an error about missing GDAL.**

A: GDAL is required for raster data sources (GDALRasterSource, CloudOptimizedGeoTIFF). Install GDAL or use only vector data sources.

**Q: Orleans silos cannot form a cluster.**

A: Verify:
- All silos use the same Orleans database connection string
- All silos have the same `clusterId` and `serviceId`
- Firewall allows traffic on the silo port and gateway port
- The Orleans database tables have been created (Orleans creates them automatically on first run)

### 8.7 Docker

**Q: The Docker container exits immediately.**

A: Check the container logs: `docker logs zserver`. Common issues:
- Missing or misconfigured `conf/appsettings.json`
- Database connection failure
- Port conflict on the host

**Q: How do I persist the tile cache across container restarts?**

A: Use a Docker volume for the cache directory as shown in the Docker Compose configuration.

### 8.8 Upgrading

**Q: How do I upgrade ZServer to a new version?**

A:
1. Pull the latest code or Docker image
2. Review the changelog for breaking configuration changes
3. Backup your configuration files and cache
4. Deploy the new version
5. Verify with health check and test tile requests
