# Database Design

**Project**: ZServer - Distributed Map Tile Server
**Author**: Lewis Zou
**Date**: 2026-07-27
**Version**: 1.26.727.236

## Change Log

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.26.727.236 | 2026-07-27 | Lewis Zou | Initial version |

---

## 1. Design Philosophy

ZServer takes a **connect-to-existing** approach to spatial databases. Rather than defining a schema that ZServer owns through code-first migrations, the system connects to pre-existing spatial databases and renders whatever geospatial data they contain. This design choice reflects the reality that geospatial data is typically managed by dedicated GIS tools, ETL pipelines, or third-party systems.

**Key Principles:**

1. **Read-Only Access**: ZServer queries spatial databases in read-only mode. Writes to the spatial database are not part of ZServer's responsibility.
2. **Schema Agnostic**: The system works with any PostGIS table that has a geometry column and an SRID — no specific table structure is required.
3. **Configuration Over Convention**: Data source connections and table mappings are configured in JSON, not inferred from the database schema.
4. **SRID Awareness**: Each source configuration specifies the SRID of the source data, allowing ZServer to perform correct coordinate transformations.

---

## 2. Typical PostGIS Table Structure

While ZServer imposes no fixed schema, a typical PostGIS vector data source contains:

### 2.1 Example: `public.administrative_boundaries`

```sql
CREATE TABLE public.administrative_boundaries (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    admin_level INTEGER NOT NULL DEFAULT 2,
    population BIGINT,
    area_sqkm DOUBLE PRECISION,
    geometry geometry(MultiPolygon, 4326) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Spatial index (required for performance)
CREATE INDEX idx_admin_boundaries_geometry
    ON public.administrative_boundaries
    USING GIST (geometry);

-- Attribute index for common query filters
CREATE INDEX idx_admin_boundaries_level
    ON public.administrative_boundaries (admin_level);
```

### 2.2 Example: `public.roads`

```sql
CREATE TABLE public.roads (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255),
    road_class VARCHAR(50) NOT NULL,  -- 'highway', 'primary', 'secondary', 'residential'
    surface VARCHAR(50),
    lanes INTEGER,
    speed_limit INTEGER,
    geometry geometry(MultiLineString, 4326) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE INDEX idx_roads_geometry
    ON public.roads
    USING GIST (geometry);

CREATE INDEX idx_roads_class
    ON public.roads (road_class);
```

### 2.3 Column Recommendations

| Aspect | Recommendation | Rationale |
|--------|---------------|-----------|
| **Geometry Column** | Name it `geometry` or configure in source settings | Standard PostGIS convention |
| **SRID** | Use EPSG:4326 or EPSG:3857 for web mapping | Avoids on-the-fly reprojection for most use cases |
| **Spatial Index** | Always create a GIST index on the geometry column | Essential for bounding box query performance |
| **Attribute Indexes** | Create B-tree indexes on columns used in CQL filters or WHERE clauses | Speeds up filtered queries |
| **Data Types** | Use standard PostgreSQL types (INTEGER, VARCHAR, NUMERIC, TIMESTAMPTZ) | Simplifies configuration and reduces type mapping issues |

---

## 3. Configuration Storage

ZServer stores all operational configuration in **JSON files**, not in a database. This approach was chosen for:

1. **Simplicity**: JSON files are human-readable and version-controllable.
2. **No Database Dependency**: The system can operate without any database (file-based sources only).
3. **Runtime Reload**: Configuration can be updated by modifying JSON files and signaling a reload — no database connection is needed.

### 3.1 Configuration File Location

```
conf/
└── appsettings.json    # All ZServer configuration
```

### 3.2 Source Configuration Example

```json
{
  "sources": {
    "my_postgis_source": {
      "type": "Postgre",
      "connectionString": "Host=localhost;Database=geodata;Username=zserver;Password=...",
      "table": "public.administrative_boundaries",
      "geometryColumn": "geometry",
      "srid": 4326,
      "keyColumn": "id",
      "attributes": ["name", "admin_level", "population"]
    },
    "my_shapefile_source": {
      "type": "ShapeFile",
      "path": "/data/shapefiles/countries.shp",
      "encoding": "UTF-8"
    },
    "my_cog_source": {
      "type": "COG",
      "path": "/data/imagery/satellite.tif"
    }
  }
}
```

### 3.3 Layer Configuration Example

```json
{
  "layers": [
    {
      "name": "admin_boundaries",
      "title": "Administrative Boundaries",
      "source": "my_postgis_source",
      "sourceType": "Postgre",
      "crs": "EPSG:4326",
      "styles": ["boundary_style"],
      "minZoom": 0,
      "maxZoom": 18,
      "enabled": true
    }
  ]
}
```

---

## 4. Orleans Cluster Tables

When running in cluster mode, ZServer requires a PostgreSQL database for Orleans cluster membership management. This database is managed entirely by the Orleans framework — no ZServer-specific migration is needed.

### 4.1 Orleans-System Tables

The Orleans ADO.NET provider creates the following tables in the specified database:

| Table | Purpose | Managed By |
|-------|---------|------------|
| `OrleansMembershipTable` | Tracks silo cluster membership (active silos, heartbeats, roles) | Orleans |
| `OrleansMembershipVersionTable` | Monotonically increasing version counter for membership changes | Orleans |
| `OrleansRemindersTable` | (Optional) Stores reminder state for Orleans grains | Orleans |

These tables are created automatically by Orleans when using `UseAdoNetClustering()` with the PostgreSQL invariant.

### 4.2 Connection String Configuration

```json
{
  "orleans": {
    "connectionString": "Host=localhost;Database=orleans;Username=zserver;Password=...",
    "clusterId": "zserver-cluster",
    "serviceId": "zserver"
  }
}
```

Or configured via environment variable:

```bash
ZSERVER_Orleans__ConnectionString="Host=db;Database=orleans;Username=zserver;Password=..."
```

### 4.3 Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| **PostgreSQL for clustering** | Preferred for production environments with familiar tooling |
| **ADO.NET provider** | Allows using any supported database (PostgreSQL, SQL Server, MySQL) without code changes |
| **Non-persistent grains** | Tile grains do not persist state — they re-render on activation. Only the membership table is needed. |
| **Separate cluster database** | Recommended to use a separate database from the spatial data to avoid connection contention |

---

## 5. Index Recommendations

### 5.1 Spatial Indexes

Always create GIST indexes on geometry columns:

```sql
CREATE INDEX CONCURRENTLY idx_table_geometry
    ON schema.table
    USING GIST (geometry);
```

### 5.2 Attribute Indexes (for CQL Filtered Queries)

For columns frequently used in CQL expressions:

```sql
CREATE INDEX CONCURRENTLY idx_table_column
    ON schema.table (column_name);
```

### 5.3 Orleans Cluster Tables

The Orleans tables have built-in indexes. No additional index maintenance is required.

---

## 6. Connection Pooling

The PostgreSQL data source uses Npgsql's built-in connection pooling:

```json
{
  "connectionString": "Host=localhost;Database=geodata;Username=zserver;Password=...;Pooling=true;Maximum Pool Size=50;Connection Idle Lifetime=300"
}
```

**Recommended Pool Settings:**

| Parameter | Value | Rationale |
|-----------|-------|-----------|
| `Pooling` | `true` | Enable connection reuse |
| `Maximum Pool Size` | `50` | Per-silo-node connection limit — adjust based on concurrent request volume |
| `Connection Idle Lifetime` | `300` (seconds) | Release idle connections after 5 minutes |
| `Connection Pruning Interval` | `60` (seconds) | Check for idle connections every minute |
