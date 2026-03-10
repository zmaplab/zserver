# ZServer 使用文档

## 文档信息

| 版本 | 日期 | 作者 | 说明 |
|------|------|------|------|
| 1.0.0 | 2026-03-10 | ZServer Team | 初始版本 |

---

## 1. 项目概述

### 1.1 项目定位

**ZServer** 是一个基于 **Actor 模型**（Orleans 框架）实现的分布式计算地图服务器，完全符合 **OpenGIS Web 服务器规范**（OGC WMS/WMTS）。

### 1.2 核心能力

| 能力类别 | 具体描述 |
|----------|----------|
| **WMS 服务** | Web Map Service，支持 GetMap、GetFeatureInfo 等操作 |
| **WMTS 服务** | Web Map Tile Service，支持实时瓦片渲染和缓存 |
| **分布式计算** | 基于 Orleans Actor 模型，支持集群部署和水平扩展 |
| **多数据源支持** | PostgreSQL/PostGIS、ShapeFile、COG GeoTIFF、远程 WMTS |
| **样式支持** | SLD (Styled Layer Descriptor) 样式配置 |
| **瓦片缓存** | 本地文件系统缓存，支持高效瓦片服务 |

### 1.3 适用场景

- **GIS 政务/企业应用**：需要发布矢量地图、影像地图服务
- **位置服务 LBS**：提供地图瓦片、动态渲染服务
- **分布式 GIS 平台**：需要高并发、高可用地图服务
- **OGC 标准兼容**：需要符合 WMS/WMTS 标准的地图服务器

### 1.4 技术栈

| 层级 | 技术选型 |
|------|----------|
| 运行时 | .NET 9.0 / .NET 10.0 |
| 分布式框架 | Orleans (Actor 模型) |
| Web 框架 | ASP.NET Core |
| 地图渲染 | SkiaSharp |
| 空间数据库 | PostgreSQL + PostGIS |
| 瓦片缓存 | 本地文件系统 |

---

## 2. 快速上手

### 2.1 环境依赖

| 依赖 | 版本要求 | 说明 |
|------|----------|------|
| **Docker** | 最新版 | 用于运行 PostgreSQL |
| **PostgreSQL + PostGIS** | 13+ | 空间数据库 |
| **dotnet SDK** | 9.0 或 10.0 | 编译运行 |
| **Node.js** | 16+ | 前端开发（如需要） |

### 2.2 部署步骤

#### 步骤 1：启动 PostgreSQL

```bash
docker run --name postgis -p 5432:5432 -e POSTGRES_PASSWORD=1qazZAQ! -d postgis/postgis
```

#### 步骤 2：创建数据库

```sql
CREATE DATABASE zserver_dev;
CREATE EXTENSION postgis;
```

#### 步骤 3：准备测试数据

通过 QGIS/ArcGIS 导入 `src/ZServer.API/shapes/polygon.shp` 到数据库，表名为 `polygon`。

#### 步骤 4：编译项目

```bash
dotnet build ZServer.sln
```

### 2.3 最简启动流程

#### 单机模式启动

```bash
dotnet run --project src/ZServer.API/ZServer.API.csproj -- \
  --Standalone true \
  --Port 8200 \
  --ClusterDashboard true \
  --ClusterDashboardPort 8182
```

#### 集群模式启动

```bash
# 启动第一个节点
dotnet run --project src/ZServer.API/ZServer.API.csproj -- \
  --Standalone false \
  --ClusterSiloPort 10001 \
  --ClusterGatewayPort 20001 \
  --Port 8100

# 启动第二个节点
dotnet run --project src/ZServer.API/ZServer.API.csproj -- \
  --Standalone false \
  --ClusterSiloPort 10002 \
  --ClusterGatewayPort 20002 \
  --Port 8200
```

### 2.4 Docker Compose 部署

```yaml
version: "3"
services:
  zserver-api:
    image: "zlzforever/zserver-api:latest"
    restart: always
    ports:
      - 8200:8200
      - 8201:8182
      - 41113:41113
      - 31113:31113
    volumes:
      - ./conf:/app/conf
      - ./cache:/app/cache
      - ./symbols:/app/Symbols
      - ./fonts:/app/fonts
      - ./shapes:/app/shapes
      - ./sld:/app/sld
    environment:
      - PORT=8200
      - standalone=false
      - ClusterDashboard=true
      - ClusterDashboardPort=8182
      - HOST_IP=192.168.0.244
      - TZ=Asia/Shanghai
```

---

## 3. 功能模块说明

### 3.1 整体架构

```
┌─────────────────────────────────────────────────────────────┐
│                         ZServer.API                         │
│                    (ASP.NET Core Web API)                   │
├─────────────────────────────────────────────────────────────┤
│  Controllers: WMSController | WMTSController | ToolController │
├─────────────────────────────────────────────────────────────┤
│                    Orleans Cluster Client                    │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐  │
│  │   WMS Grain     │  │   WMTS Grain   │  │  XYZ Grain   │  │
│  │  (分布式渲染)   │  │  (瓦片服务)     │  │  (XYZ瓦片)   │  │
│  └─────────────────┘  └─────────────────┘  └──────────────┘  │
├─────────────────────────────────────────────────────────────┤
│                       ZServer Store                          │
│  LayerStore | SourceStore | StyleGroupStore | ResourceGroup  │
├─────────────────────────────────────────────────────────────┤
│                         ZMap Core                            │
│  Layer | StyleGroup | Source (Postgre/ShapeFile/COG)      │
├─────────────────────────────────────────────────────────────┤
│                    SkiaSharp Renderer                        │
└─────────────────────────────────────────────────────────────┘
```

### 3.2 核心模块

#### 3.2.1 WMS 服务 (WMSController)

**模块职责**：提供符合 OGC WMS 标准的地图服务

**业务流程**：
1. 接收 HTTP GET 请求（GetMap / GetFeatureInfo）
2. 验证请求参数（图层、样式、范围、坐标系等）
3. 调用 WMSGrain 进行分布式渲染
4. 返回 PNG/JPEG 等图像格式

**使用场景**：
- 动态地图渲染
- 要素查询 (GetFeatureInfo)
- 多图层叠加显示

#### 3.2.2 WMTS 服务 (WMTSController)

**模块职责**：提供符合 OGC WMTS 标准的瓦片服务

**业务流程**：
1. 接收瓦片请求 (layer, tileMatrix, tileRow, tileCol)
2. 检查瓦片缓存
3. 调用 WMTSGrain 渲染瓦片
4. 返回瓦片图像

**使用场景**：
- 高性能地图瓦片服务
- 离线地图应用
- 大规模地图展示

#### 3.2.3 工具服务 (ToolController)

**模块职责**：提供 CRS authority 查询等辅助工具

**使用场景**：
- 坐标系 WKT 解析
- EPSG 代码查询

#### 3.2.4 数据存储层 (Store)

| Store | 职责 |
|-------|------|
| **LayerStore** | 管理图层配置，提供图层查询、刷新 |
| **SourceStore** | 管理数据源（PostgreSQL、ShapeFile、COG 等） |
| **StyleGroupStore** | 管理样式组配置 |
| **ResourceGroupStore** | 管理资源组 |
| **LayerGroupStore** | 管理图层分组 |
| **GridSetStore** | 管理瓦片网格定义 |
| **SldStore** | 管理 SLD 样式文档 |

#### 3.2.5 Orleans Grains

| Grain | 职责 |
|-------|------|
| **IWMSGrain** | WMS 请求处理，分布式渲染 |
| **IWMTSGrain** | WMTS 瓦片渲染，支持缓存 |
| **IXyzGrain** | XYZ 格式瓦片服务 |

---

## 4. API 调用说明

### 4.1 接口清单

| 接口路径 | 方法 | 说明 |
|----------|------|------|
| `/wms` | GET | WMS 服务（GetMap/GetFeatureInfo） |
| `/wmts` | GET | WMTS 瓦片服务 |
| `/api/v1.0/tools/crs_authority` | POST | CRS Authority 查询 |

---

### 4.2 WMS 服务接口

#### 4.2.1 GetMap - 地图渲染

**请求地址**：`GET /wms?request=GetMap&...`

**请求参数**：

| 参数名 | 类型 | 必填 | 说明 | 取值说明 |
|--------|------|------|------|----------|
| `request` | string | 是 | 请求类型 | 固定值：`GetMap` |
| `layers` | string | 是 | 图层名称 | 支持两种方式：<br>1. 资源组图层：`resourceGroup:layer`<br>2. 直接访问图层：`layer`<br>多个图层用逗号分隔 |
| `styles` | string | 否 | 样式名称 | 逗号分隔，与 layers 顺序对应 |
| `srs` | string | 是 | 空间参考 | 如 `EPSG:900913`、`EPSG:4326` |
| `bbox` | string | 是 | 地理范围 | 格式：`minX,minY,maxX,max5` |
| `width` | int | 是 | 输出图像宽度 | 像素值 |
| `height` | int | 是 | 输出图像高度 | 像素值 |
| `format` | string | 否 | 输出格式 | 默认 `image/png`，支持 `image/png`、`image/jpeg` |
| `transparent` | bool | 否 | 是否透明 | 默认 `false` |
| `bgColor` | string | 否 | 背景色 | 十六进制颜色，如 `0xFFFFFF` |
| `Z_FILTER` | string | 否 | 过滤条件 | SQL WHERE 条件 |
| `buffer` | int | 否 | 缓冲区大小 | 默认 0 |
| `bordered` | bool | 否 | 是否加边框 | 默认 `false` |

**请求示例**：

```
GET /wms?request=GetMap&layers=polygon&srs=EPSG:4326&bbox=123.3984375,27.7734375,126.9140625,31.2890625&width=800&height=600&format=image/png
```

**成功响应**：
- Content-Type: `image/png`
- 返回图像二进制数据

**失败响应**：
```json
{
  "success": false,
  "msg": "错误信息",
  "code": "错误码"
}
```

#### 4.2.2 GetFeatureInfo - 要素查询

**请求地址**：`GET /wms?request=GetFeatureInfo&...`

**请求参数**：

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `request` | string | 是 | 固定值：`GetFeatureInfo` |
| `layers` | string | 是 | 查询图层 |
| `srs` | string | 是 | 空间参考 |
| `bbox` | string | 是 | 地理范围 |
| `width` | int | 是 | 图像宽度 |
| `height` | int | 是 | 图像高度 |
| `x` | float | 是 | 查询点 X 坐标（像素） |
| `y` | float | 是 | 查询点 Y 坐标（像素） |
| `featureCount` | int | 否 | 返回要素数量，默认 1 |

**成功响应**：
```json
{
  "type": "FeatureCollection",
  "features": []
}
```

---

### 4.3 WMTS 服务接口

#### 4.3.1 GetTile - 瓦片获取

**请求地址**：`GET /wmts?layer=...&tileMatrix=...&tileRow=...&tileCol=...`

**请求参数**：

| 参数名 | 类型 | 必填 | 说明 | 取值说明 |
|--------|------|------|------|----------|
| `layer` | string | 是 | 图层名称 | 支持多图层逗号分隔 |
| `style` | string | 否 | 样式名称 | 默认 `default` |
| `tileMatrix` | string | 是 | 瓦片矩阵 | 缩放级别标识 |
| `tileRow` | int | 是 | 瓦片行号 |  |
| `tileCol` | int | 是 | 瓦片列号 |  |
| `format` | string | 否 | 瓦片格式 | 默认 `image/png` |
| `tileMatrixSet` | string | 是 | 瓦片矩阵集 | 如 `EPSG:4326`、`EPSG:3857` |
| `Z_FILTER` | string | 否 | 过滤条件 |  |

**请求示例**：

```
GET /wmts?layer=polygon&style=default&tileMatrix=5&tileRow=10&tileCol=20&format=image/png&tileMatrixSet=EPSG:4326
```

**成功响应**：
- Content-Type: `image/png`
- 返回瓦片图像二进制

**失败响应**：
```json
{
  "success": false,
  "msg": "错误信息",
  "code": "错误码"
}
```

---

### 4.4 工具服务接口

#### 4.4.1 CRS Authority 查询

**请求地址**：`POST /api/v1.0/tools/crs_authority`

**请求头**：
- Content-Type: `text/plain`

**请求体**：WKT 格式的坐标系定义

**请求示例**：

```
POST /api/v1.0/tools/crs_authority
Content-Type: text/plain

GEOGCS["WGS 84",DATUM["WGS_1984",SPHEROID["WGS 84",6378137,298.257223563,AUTHORITY["EPSG","7030"]],AUTHORITY["EPSG","6326"]],PRIMEM["Greenwich",0,AUTHORITY["EPSG","8901"]],UNIT["degree",0.0174532925199433,AUTHORITY["EPSG","9122"]]]
```

**成功响应**：
```json
{
  "success": true,
  "code": 0,
  "data": "EPSG:4326",
  "msg": ""
}
```

**失败响应**：
```json
{
  "success": false,
  "code": -1,
  "data": "",
  "msg": "WKT 解析失败"
}
```

---

## 5. 配置文件说明

### 5.1 配置文件路径

| 文件 | 路径 | 说明 |
|------|------|------|
| 应用配置 | `conf/appsettings.json` | 主配置文件 |
| 服务配置 | `conf/zserver.json` | 图层、样式、数据源配置 |
| 日志配置 | `conf/serilog.json` | Serilog 日志配置 |

### 5.2 appsettings.json

```json
{
  "standalone": true,
  "jwtBearer": {
    "validateAudience": false,
    "validateIssuer": false,
    "KeyPath": ""
  },
  "tokens": ["682be39c9d8786225072af8b"],
  "apiName": "zserver-api",
  "permissionApi": "",
  "config": {
    "provider": "socodb | file",
    "address": "conf/zserver.json",
    "address1": "http://localhost:5000/api/v1.0/tables/6925c4c80235b7f51dbe1434/data",
    "appId": "573d8247de6efe296966e8ab",
    "appSecret": "qn+uIuS43SDPH1rK+C+FmQ=="
  },
  "orleans": {
    "connectionString": "User ID=postgres;Password=;Host=192.168.100.254;Port=5432;Database=zserver_dev;Pooling=true;",
    "invariant": "Npgsql",
    "siloName": "zserver",
    "clusterId": "zserver",
    "serviceId": "zserver",
    "gatewayPort": 41113,
    "siloPort": 31113,
    "dashboard": true
  }
}
```

#### 配置项详解

| 配置项                              | 类型       | 默认值             | 作用            | 注意事项 |
|----------------------------------|----------|-----------------|---------------|----------|
| `standalone`                     | bool     | `true`          | 单机/集群模式       | `true` 单机，`false` 集群 |
| `jwtBearer.authority`            | string   | `""`            | JWT 颁发者       | 生产环境建议配置 |
| `jwtBearer.requireHttpsMetadata` | bool     | `false`         | 是否需要 HTTPS    | 生产环境建议 `true` |
| `jwtBearer.validateAudience`     | bool     | `false`         | 验证受众          | 生产环境建议 `true` |
| `jwtBearer.validateIssuer`       | bool     | `false`         | 验证颁发者         | 生产环境建议 `true` |
| `jwtBearer.keyPath`              | string   |                 | JWT 解析 所使用的私钥 | 生产环境建议 `true` |
| `tokens`                         | string[] | `[]`            | Token 认证列表    | 用于 Token 认证方式 |
| `apiName`                        | string   | `"zserver-api"` | API 名称        | 用于 JWT scope |
| `permissionApi`                  | string   | `""`            | 权限 API 地址     | 外部权限服务地址 |
| `config.provider`                | string   | `""`            | 配置提供者         | `socodb` 或 `file` |
| `config.address`                 | string   | `""`            | 配置文件路径        | 当 provider=file 时使用 |
| `orleans.connectionString`       | string   | `""`            | Orleans 数据库连接 | PostgreSQL 连接字符串 |
| `orleans.invariant`              | string   | `"Npgsql"`      | 数据库驱动         | 目前仅支持 Npgsql |
| `orleans.siloName`               | string   | `"zserver"`     | Silo 名称       | 集群内唯一 |
| `orleans.clusterId`              | string   | `"zserver"`     | 集群 ID         | 同一集群内一致 |
| `orleans.serviceId`              | string   | `"zserver"`     | 服务 ID         | 区分不同服务 |
| `orleans.gatewayPort`            | int      | `41113`         | 网关端口          | 客户端连接端口 |
| `orleans.siloPort`               | int      | `31113`         | Silo 端口       | 内部通信端口 |
| `orleans.dashboard`              | bool     | `true`          | 是否启用仪表盘       | 开发环境有用 |

### 5.3 zserver.json

此文件定义图层、数据源、样式等核心配置。

```json
{
  "sources": {
    "postgresql_source": {
      "provider": "ZMap.Source.Postgre.PostgreSource, ZMap.Source.Postgre",
      "connectionString": "User ID=postgres;Password=xxx;Host=localhost;Port=5432;Database=zserver_dev;Pooling=true;"
    },
    "shapefile_source": {
      "provider": "ZMap.Source.ShapeFile.ShapeFileSource, ZMap.Source.ShapeFile",
      "file": "shapes/polygon.shp"
    },
    "cog_source": {
      "provider": "ZMap.Source.CloudOptimizedGeoTIFF.COGGeoTiffSource, ZMap.Source.CloudOptimizedGeoTIFF",
      "file": "/data/raster.tif"
    },
    "remote_wmts": {
      "provider": "ZMap.Source.RemoteWmtsSource, ZMap",
      "url": "https://t3.tianditu.gov.cn/img_c/wmts?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER=img&STYLE=default&TILEMATRIXSET=c&FORMAT=tiles&TILECOL={2}&TILEROW={1}&TILEMATRIX={0}&tk="
    }
  },
  "styleGroups": {
    "polygon_style": {
      "zoomUnit": "scale",
      "minZoom": 100,
      "maxZoom": 9990000,
      "styles": [
        { "type": "fill", "color": "#66FF99", "opacity": "0.7" },
        { "type": "line", "color": "#66FF66", "width": "1", "opacity": "0.9" },
        { "type": "text", "label": "{{ feature['name'] }}", "font": ["SimSun"], "size": 16 }
      ]
    }
  },
  "resourceGroups": {
    "default": { "description": "默认资源组" }
  },
  "layers": {
    "polygon": {
      "resourceGroup": "default",
      "source": "postgresql_source",
      "sourceTable": "polygon",
      "sourceGeometry": "geom",
      "sourceSRID": 4326,
      "styleGroups": ["polygon_style"],
      "buffers": [
        { "minZoom": 1, "maxZoom": 300000, "zoomUnit": "scale", "size": 64 }
      ]
    }
  }
}
```

#### 配置项详解

##### 5.3.1 数据源 (sources)

| 配置项 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `provider` | string | 是 | 数据源类型：`ZMap.Source.Postgre.PostgreSource`、`ZMap.Source.ShapeFile.ShapeFileSource`、`ZMap.Source.CloudOptimizedGeoTIFF.COGGeoTiffSource`、`ZMap.Source.RemoteWmtsSource` |
| `connectionString` | string | PostgreSQL 必填 | PostgreSQL 连接字符串 |
| `file` | string | ShapeFile/COG 必填 | 文件路径 |
| `url` | string | 远程 WMTS 必填 | WMTS 瓦片地址模板 |

##### 5.3.2 样式组 (styleGroups)

| 配置项 | 类型 | 说明 |
|--------|------|------|
| `zoomUnit` | string | 缩放单位：`scale` 或 `zoom` |
| `minZoom` | double | 最小缩放级别 |
| `maxZoom` | double | 最大缩放级别 |
| `styles` | array | 样式数组 |

**styles 子项**：

| 配置项 | 类型 | 说明 |
|--------|------|------|
| `type` | string | 样式类型：`fill`、`line`、`text` |
| `color` | string | 颜色（十六进制） |
| `width` | string/number | 线宽 |
| `opacity` | string/number | 透明度 0-1 |
| `label` | string | 标签模板，如 `{{ feature['name'] }}` |
| `font` | string[] | 字体列表 |
| `size` | number | 文字大小 |

##### 5.3.3 图层 (layers)

| 配置项 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `resourceGroup` | string | 是 | 资源组名称 |
| `source` | string | 是 | 数据源名称 |
| `sourceTable` | string | 否 | 数据表名（PostgreSQL/ShapeFile） |
| `sourceGeometry` | string | 否 | 几何字段名 |
| `sourceSRID` | int | 否 | 空间参考编号 |
| `styleGroups` | string[] | 否 | 样式组名称列表 |
| `buffers` | array | 否 | 栅格缓冲配置 |
| `extent` | array | 否 | 显示范围 [minX, minY, maxX, maxY] |

### 5.4 命令行参数

| 参数 | 说明 | 默认值 |
|------|------|--------|
| `--Port` | HTTP 服务端口 | 8200 |
| `--Standalone` | 是否单机模式 | true |
| `--ClusterSiloPort` | Silo 端口（集群模式） | 31113 |
| `--ClusterGatewayPort` | 网关端口（集群模式） | 41113 |
| `--ClusterDashboard` | 启用集群仪表盘 | false |
| `--ClusterDashboardPort` | 仪表盘端口 | 8182 |
| `--HOST_IP` | 主机 IP | 自动检测 |

### 5.5 环境变量

| 变量名 | 说明 |
|--------|------|
| `PORT` | HTTP 服务端口 |
| `HOST_IP` | Orleans 主机 IP |
| `TZ` | 时区，如 `Asia/Shanghai` |
| `EnableSensitiveDataLogging` | 启用敏感数据日志 |
| `LOG_PATH` | 日志文件路径 |

---

## 6. 核心数据结构

### 6.1 Layer (图层)

```csharp
public class Layer
{
    /// 图层名称
    public string Name { get; set; }
    
    /// 资源组标识
    public string ResourceId { get; set; }
    
    /// 资源组
    public ResourceGroup ResourceGroup { get; set; }
    
    /// 最小可视缩放
    public double MinZoom { get; set; }
    
    /// 最大可视缩放
    public double MaxZoom { get; set; }
    
    /// 缩放单位 (Scale/Zoom)
    public ZoomUnits ZoomUnit { get; set; }
    
    /// 样式组
    public List<StyleGroup> StyleGroups { get; set; }
    
    /// 是否启用
    public bool Enabled { get; set; }
    
    /// 显示范围
    public Envelope Envelope { get; set; }
    
    /// 栅格缓冲
    public List<GridBuffer> Buffers { get; set; }
    
    /// 数据源
    public ISource Source { get; set; }
    
    /// 空间标识符
    public int Srid => Source.Srid;
    
    /// 支持的服务类型
    public HashSet<ServiceType> Services { get; set; }
    
    /// 过滤条件
    public string Filter { get; set; }
}
```

### 6.2 ResourceGroup (资源组)

```csharp
public class ResourceGroup
{
    /// 标识
    public string Id { get; set; }
    
    /// 名称
    public string Name { get; set; }
    
    /// 描述
    public string Description { get; set; }
}
```

### 6.3 ZServerResponse (服务响应)

```csharp
public record ZServerResponse
{
    /// 异常（为空则成功）
    public ServerException Exception { get; init; }
    
    /// 内容格式
    public string ContentType { get; init; }
    
    /// 响应体
    public byte[] Body { get; init; }
}
```

### 6.4 ServerException (服务异常)

```csharp
public class ServerException
{
    /// 错误码
    public string Code { get; set; }
    
    /// 错误位置
    public string Locator { get; set; }
    
    /// 错误文本
    public string Text { get; set; }
}
```

---

## 7. 异常与错误处理

### 7.1 错误码清单

| 错误码 | 说明 | 可能原因 |
|--------|------|----------|
| 0 | 成功 | 正常 |
| -1 | WKT 解析失败 | WKT 格式不正确 |
| 404 | 疑似非标准 CRS | 坐标系不被识别 |
| 403 | 权限不足 | 未授权访问 |
| 500 | 系统内部错误 | 服务器异常 |

### 7.2 异常场景

#### 7.2.1 WMS 异常

| Code | Locator | 说明 |
|------|---------|------|
| `LayerNotDefined` | layers | 请求的图层不存在 |
| `StyleNotDefined` | styles | 请求的样式不存在 |
| `InvalidCRS` | srs | 不支持的空间参考 |
| `InvalidBBox` | bbox | 无效的地理范围 |
| `InvalidDimension` | width/height | 无效的图像尺寸 |

#### 7.2.2 WMTS 异常

| Code | 说明 |
|------|------|
| `InvalidTileMatrix` | 无效的瓦片矩阵 |
| `InvalidTileRow` | 无效的瓦片行号 |
| `InvalidTileCol` | 无效的瓦片列号 |
| `LayerNotDefined` | 图层不存在 |

### 7.3 错误响应格式

```json
{
  "success": false,
  "msg": "错误信息",
  "code": 500
}
```

### 7.4 排查方案

| 症状 | 可能原因 | 排查方法 |
|------|----------|----------|
| 返回 500 错误 | 服务内部异常 | 检查日志文件 |
| 图层不显示 | 数据源配置错误 | 验证数据库连接 |
| 瓦片请求失败 | 图层名称错误 | 确认图层名称和 tileMatrixSet |
| 样式不生效 | 样式配置错误 | 检查 zserver.json 中 styleGroups |
| 坐标系转换错误 | SRS 不匹配 | 确认 sourceSRID 与数据一致 |

---

## 8. 权限与安全说明

### 8.1 鉴权方式

ZServer 支持两种认证方式：

1. **JWT Bearer Token 认证**
2. **Simple Token 认证**

#### 8.1.1 认证配置

在 `appsettings.json` 中配置：

```json5
{
  "EnableAuthorization": "true",  // 启用认证
  "jwtBearer": {
    "authority": "https://auth.example.com",
    "requireHttpsMetadata": true,
    "validateAudience": true,
    "validateIssuer": true
  },
  "tokens": ["your-token-here"],
  "apiName": "zserver-api"
}
```

#### 8.1.2 认证流程

```
客户端请求 → UseAuthentication() → 验证 Token → UseAuthorization() → 控制器
```

### 8.2 权限控制

#### 8.2.1 默认策略

```csharp
options.AddPolicy("default", policy =>
{
    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, "Token");
    policy.RequireAuthenticatedUser();
    policy.RequireClaim("scope", apiName);
});
```

#### 8.2.2 API 文档策略

```csharp
options.AddPolicy("api-document", policy =>
{
    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, "Token");
    policy.RequireAuthenticatedUser();
    policy.RequireRole("zserver-api-document");
});
```

### 8.3 安全建议

| 项目 | 建议 |
|------|------|
| Token 安全 | 定期更换 tokens 列表中的 Token |
| JWT 验证 | 生产环境启用 `validateAudience` 和 `validateIssuer` |
| CORS | 生产环境配置具体的允许来源 |
| 日志 | 启用日志记录，监控异常访问 |
| 连接字符串 | 不要硬编码，使用环境变量或密钥管理服务 |

---

## 9. 常见问题 FAQ

### 9.1 部署常见问题

#### Q1: 如何启动单机模式？

```bash
dotnet run --project src/ZServer.API/ZServer.API.csproj -- --Standalone true --Port 8200
```

#### Q2: 如何配置集群模式？

1. 修改 `appsettings.json`：`"standalone": false`
2. 配置 Orleans 连接字符串
3. 启动多个节点，使用不同端口

#### Q3: PostgreSQL 连接失败？

- 检查 `connectionString` 是否正确
- 确认数据库存在且 PostGIS 扩展已安装
- 检查防火墙和网络配置

#### Q4: 瓦片缓存路径？

瓦片缓存默认存储在 `cache/` 目录下。

### 9.2 调用常见问题

#### Q5: WMS 请求返回空图像？

- 检查图层名称是否正确
- 确认 bbox 范围在数据范围内
- 验证 SRS 与数据源 SRID 一致

#### Q6: WMTS 瓦片不显示？

- 确认 tileMatrixSet 与图层 SRS 匹配
- 检查 tileRow/tileCol 是否在有效范围内
- 验证图层是否启用

#### Q7: 如何过滤特定要素？

使用 `Z_FILTER` 参数：

```
Z_FILTER=name='特定名称'
```

### 9.3 配置常见问题

#### Q8: 如何添加新的数据源？

在 `zserver.json` 的 `sources` 节点添加：

```json
{
  "new_source": {
    "provider": "ZMap.Source.Postgre.PostgreSource, ZMap.Source.Postgre",
    "connectionString": "..."
  }
}
```

#### Q9: 如何配置多图层叠加？

在请求 `layers` 参数中使用逗号分隔：

```
layers=layer1,layer2
```

或使用资源组格式：

```
layers=resourceGroup1:layer1,resourceGroup2:layer2
```

#### Q10: 如何启用 HTTPS？

1. 配置 Kestrel 证书
2. 设置 `requireHttpsMetadata: true`
3. 使用反向代理（Nginx/Apache）

---

## 10. 版本更新记录

### v1.0.0 (2026-03-10)

**新增功能**：
- 完整 OGC WMS 服务支持
- 完整 OGC WMTS 服务支持
- SLD 样式配置支持
- Orleans 分布式集群支持
- 多数据源支持（PostgreSQL、ShapeFile、COG、远程WMTS）

**初始版本发布**
