# ZServer — 分布式地图瓦片服务器

[![Docker Image CI](https://github.com/zmaplab/zserver/actions/workflows/docker-image.yml/badge.svg)](https://github.com/zmaplab/zserver/actions/workflows/docker-image.yml)
[![GitHub](https://img.shields.io/badge/GitHub-zmaplab/zserver-181717?logo=github)](https://github.com/zmaplab/zserver)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://github.com/zmaplab/zserver/blob/main/LICENSE.txt)

---

## 项目简介

**ZServer** 是一个基于 **Actor 模型**（[Microsoft Orleans](https://learn.microsoft.com/en-us/dotnet/orleans/) 框架）实现的**分布式计算地图服务器**，完全符合 **OGC（Open Geospatial Consortium）WMS / WMTS** 国际标准。

> 每个地图瓦片天然映射为一个 Orleans Actor，实现无锁并发渲染和水平扩展。

| 属性 | 值 |
|------|-----|
| **版本** | v1.5.4 |
| **目标框架** | .NET 10 |
| **分布式框架** | Orleans 10 |
| **渲染引擎** | SkiaSharp |
| **空间数据库** | PostgreSQL / PostGIS |
| **源码仓库** | [github.com/zmaplab/zserver](https://github.com/zmaplab/zserver) |
| **在线文档** | [docs.zmap.xyz](https://docs.zmap.xyz) |
| **许可证** | Apache License 2.0 |

---

## 核心特性

- **OGC 标准兼容** — 完整支持 WMS（GetMap / GetFeatureInfo）和 WMTS 协议，兼容 QGIS、Leaflet、OpenLayers 等主流 GIS 客户端
- **Actor 模型架构** — 基于 Orleans 实现，每个瓦片对应一个 Actor，天然无锁并发；水平扩展只需添加 Silo 节点
- **多数据源支持** — PostgreSQL/PostGIS、ShapeFile、COG GeoTIFF、GDAL、远程 WMTS，插件式架构可扩展
- **SLD 样式渲染** — 支持 SLD（Styled Layer Descriptor）样式定义，灵活的渲染管线（Style → Visitor → Renderer）
- **SkiaSharp 渲染管线** — 高性能 2D 图形渲染引擎，支持 Fill、Line、Text、Symbol、Raster 等多种渲染类型
- **JWT 认证授权** — 内置 JWT Bearer Token + Simple Token 双认证模式，Scope 级别权限控制
- **文件瓦片缓存** — 本地文件系统缓存 + 实时渲染，兼顾首次加载速度与重复访问性能
- **动态编译过滤** — 基于 Natasha 的 C# 动态编译，支持 CQL 运行时过滤表达式
- **多部署模式** — 支持单机模式（Standalone）和集群模式，灵活的 Docker Compose 部署

---

## 文档导航

### 中文文档 `[cn/](cn/)`

| 文档 | 说明 |
|------|------|
| [01-需求规格](cn/01-%E9%9C%80%E6%B1%82%E8%A7%84%E6%A0%BC.md) | 功能概述、功能需求清单、业务规则、非功能需求 |
| [02-概要设计](cn/02-%E6%A6%82%E8%A6%81%E8%AE%BE%E8%AE%A1.md) | 系统架构、模块划分、核心流程概要、依赖关系 |
| [03-详细设计](cn/03-%E8%AF%A6%E7%BB%86%E8%AE%BE%E8%AE%A1.md) | 核心类/方法的算法流程、分支逻辑、关键实现细节 |
| [04-数据库设计](cn/04-%E6%95%B0%E6%8D%AE%E5%BA%93%E8%AE%BE%E8%AE%A1.md) | 数据表结构、字段定义、表间关系、索引建议 |
| [05-API文档](cn/05-API%E6%96%87%E6%A1%A3.md) | 接口清单、入参出参、调用示例、异常场景 |
| [06-测试计划](cn/06-%E6%B5%8B%E8%AF%95%E8%AE%A1%E5%88%92.md) | 单元测试用例、边界条件、并发测试、测试数据准备 |
| [07-部署手册与用户手册](cn/07-%E9%83%A8%E7%BD%B2%E6%89%8B%E5%86%8C%E4%B8%8E%E7%94%A8%E6%88%B7%E6%89%8B%E5%86%8C.md) | 环境要求、配置项、部署步骤、操作指南、常见问题 |

### English Documentation `[en/](en/)`

| Document | Description |
|----------|-------------|
| [01-Requirement Specification](en/01-Requirement%20Specification.md) | Feature overview, requirement list, business rules, non-functional requirements |
| [02-Overview Design](en/02-Overview%20Design.md) | System architecture, module division, core process overview, dependency graph |
| [03-Detailed Design](en/03-Detailed%20Design.md) | Algorithm flow, branch logic, key implementation details |
| [04-Database Design](en/04-Database%20Design.md) | Table structure, field definitions, relationships, index recommendations |
| [05-API Documentation](en/05-API%20Documentation.md) | API list, parameters, response examples, error scenarios |
| [06-Test Plan](en/06-Test%20Plan.md) | Unit test cases, boundary conditions, concurrency tests, test data |
| [07-Deployment & User Manual](en/07-Deployment%20&%20User%20Manual.md) | Environment requirements, configuration, deployment steps, operation guide |

---

## 架构概览

```
┌──────────────────────────────────────────────────────────────────────────┐
│                          HTTP Layer (ASP.NET Core)                       │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │ WMSController │  │ WMTSController│  │ XYZController│  │ToolController│  │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘  └──────────────┘  │
│         │                 │                 │                            │
│  ┌──────┴─────────────────┴─────────────────┴──────────────────────┐    │
│  │                   Orleans Cluster Client                        │    │
│  │              (JWT Auth / Token Auth Middleware)                  │    │
│  └──────┬─────────────────┬─────────────────┬──────────────────────┘    │
├─────────┼─────────────────┼─────────────────┼────────────────────────────┤
│  ┌──────┴──────┐  ┌──────┴──────┐  ┌──────┴──────┐                     │
│  │  WMSGrain   │  │ WMTSGrain   │  │  XyzGrain   │   ← Orleans Grains  │
│  │ (GetMap/    │  │ (GetTile/   │  │ (XYZ Tiles) │     (Actor 模型)    │
│  │  GetFeature │  │  GetCapa    │  │             │                     │
│  │  Info)      │  │  bilities)  │  │             │                     │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘                     │
├─────────┼─────────────────┼─────────────────┼────────────────────────────┤
│  ┌──────┴─────────────────┴─────────────────┴──────────────────────┐    │
│  │                      ZServer Store Layer                         │    │
│  │  LayerStore │ SourceStore │ StyleGroupStore │ GridSetStore       │    │
│  │  ResourceGroupStore │ LayerGroupStore │ SldStore                 │    │
│  └──────────────────────────────────────────────────────────────────┘    │
├────────────────────────────────────────────────────────────────────────┤
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │                     ZMap Core Library                             │  │
│  │  ┌───────────┐  ┌────────────┐  ┌───────────┐  ┌───────────┐   │  │
│  │  │  Layer    │  │ StyleGroup │  │   Map     │  │  Feature  │   │  │
│  │  │ RasterVec │  │ Fill/Line/ │  │ Definition│  │ (GeoJSON) │   │  │
│  │  │ tor/Tiled │  │ Text/Raster│  │           │  │           │   │  │
│  │  └───────────┘  └────────────┘  └───────────┘  └───────────┘   │  │
│  └──────────────────────────────────────────────────────────────────┘  │
├────────────────────────────────────────────────────────────────────────┤
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │              Rendering Pipeline (SkiaSharp)                       │  │
│  │   Style → Visitor → Renderer (IFill/ILine/IText/ISymbol/IRaster) │  │
│  └──────────────────────────────────────────────────────────────────┘  │
├────────────────────────────────────────────────────────────────────────┤
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌────────┐ ┌──────────────┐  │
│  │PostgreSQL│ │ShapeFile │ │COG TIFF  │ │ GDAL   │ │ Remote WMTS  │  │
│  │ /PostGIS │ │          │ │          │ │        │ │              │  │
│  └──────────┘ └──────────┘ └──────────┘ └────────┘ └──────────────┘  │
└────────────────────────────────────────────────────────────────────────┘
```

### 模块说明

| 模块 | 路径 | 职责 |
|------|------|------|
| **ZMap** | `src/ZMap/` | 核心地图库 — Layer、Map、Style、Source、Renderer、OGC 协议逻辑 |
| **ZServer.API** | `src/ZServer.API/` | ASP.NET Core Web API 主机 — Controllers、Middleware、Auth、配置 |
| **ZServer.Interfaces** | `src/ZServer.Interfaces/` | Orleans Grain 契约接口 — IWMSGrain、IWMTSGrain、IXyzGrain |
| **ZServer.Grains** | `src/ZServer.Grains/` | Orleans Grain 实现 — WMS/WMTS/XYZ 分布式渲染 |
| **ZServer.Silo** | `src/ZServer.Silo/` | Orleans Silo 配置扩展 — ADO.NET 集群管理 |
| **ZServer.SiloHost** | `src/ZServer.SiloHost/` | 独立 Silo 主机入口 |
| **ZServer** | `src/ZServer/` | 服务配置与 Store — LayerStore、SourceStore、GridSetStore 等 |
| **ZMap.Renderer.SkiaSharp** | `src/ZMap.Renderer.SkiaSharp/` | SkiaSharp 渲染引擎实现 |
| **ZMap.TileGrid** | `src/ZMap.TileGrid/` | 瓦片网格数学 — GridSets、CRS 转换 |
| **ZMap.Source.Postgre** | `src/ZMap.Source.Postgre/` | PostgreSQL/PostGIS 矢量数据源 |
| **ZMap.Source.ShapeFile** | `src/ZMap.Source.ShapeFile/` | ShapeFile 矢量数据源 |
| **ZMap.Source.CloudOptimizedGeoTIFF** | `src/ZMap.Source.CloudOptimizedGeoTIFF/` | COG GeoTIFF 栅格数据源 |
| **ZMap.Source.GDAL** | `src/ZMap.Source.GDAL/` | GDAL 栅格/矢量数据源 |
| **ZMap.SLD** | `src/ZMap.SLD/` | SLD 样式支持 |
| **ZMap.DynamicCompiler** | `src/ZMap.DynamicCompiler/` | Natasha 动态 C# 编译（CQL 过滤） |
| **ZServer.Tests** | `src/ZServer.Tests/` | xUnit 单元测试 |

---

## 快速开始

### 前置依赖

- Docker（运行 PostgreSQL）
- .NET SDK 10.0
- Node.js 16+（前端开发）

### 第 1 步：启动 PostgreSQL

```bash
docker run --name postgis -p 5432:5432 -e POSTGRES_PASSWORD=1qazZAQ! -d postgis/postgis
```

### 第 2 步：创建数据库

```bash
docker exec -it postgis psql -U postgres -c "CREATE DATABASE zserver_dev;"
docker exec -it postgis psql -U postgres -d zserver_dev -c "CREATE EXTENSION postgis;"
```

### 第 3 步：编译项目

```bash
git clone https://github.com/zmaplab/zserver.git
cd zserver
dotnet build ZServer.sln
```

### 第 4 步：启动服务（单机模式）

```bash
dotnet run --project src/ZServer.API \
  --Standalone true \
  --ClusterDashboard true \
  --ClusterDashboardPort 8182 \
  --Port 8200
```

### 第 5 步：验证服务

```bash
# 获取 WMS 能力文档
curl "http://localhost:8200/wms?request=GetCapabilities"

# 打开 Orleans 仪表盘
open http://localhost:8182
```

### Docker Compose 部署

```bash
docker-compose up -d
```

---

## 数据源支持

| 数据源 | 类型 | 实现 |
|--------|------|------|
| **PostgreSQL / PostGIS** | 矢量 | `ZMap.Source.Postgre.PostgreSource` |
| **ShapeFile** | 矢量 | `ZMap.Source.ShapeFile.ShapeFileSource` |
| **COG GeoTIFF** | 栅格 | `ZMap.Source.CloudOptimizedGeoTIFF.COGGeoTiffSource` |
| **GDAL** | 栅格/矢量 | `ZMap.Source.GDAL.GdalSource` |
| **远程 WMTS** | 瓦片 | `ZMap.Source.RemoteWmtsSource` |

---

## 样式系统

ZServer 支持多种样式定义方式：

| 样式类型 | 说明 |
|----------|------|
| **FillStyle** | 面填充样式（颜色、透明度） |
| **LineStyle** | 线样式（颜色、宽度、透明度） |
| **TextStyle** | 文字标注样式（字体、大小、颜色、偏移） |
| **SymbolStyle** | 符号样式（图标、大小） |
| **RasterStyle** | 栅格渲染样式 |
| **SLD** | OGC Styled Layer Descriptor 标准样式 |

渲染管线：**Style → IZMapStyleVisitor → IRenderer**（IFillRenderer、ILineRenderer、ITextRenderer 等）

---

## API 接口一览

| 接口 | 方法 | 路径 | 说明 |
|------|------|------|------|
| WMS GetMap | GET | `/wms?request=GetMap&...` | 动态地图渲染 |
| WMS GetFeatureInfo | GET | `/wms?request=GetFeatureInfo&...` | 要素属性查询 |
| WMS GetCapabilities | GET | `/wms?request=GetCapabilities` | 服务能力文档 |
| WMTS GetTile | GET | `/wmts?layer=...&tileMatrix=...&tileRow=...&tileCol=...` | 瓦片获取 |
| WMTS GetCapabilities | GET | `/wmts?request=GetCapabilities` | 服务能力文档 |
| XYZ Tiles | GET | `/xyz/{z}/{x}/{y}.{format}` | XYZ 格式瓦片 |
| CRS Authority | POST | `/api/v1.0/tools/crs_authority` | CRS/WKT 查询 |

---

## 技术栈

| 层级 | 技术 | 用途 |
|------|------|------|
| **运行时** | .NET 10 | 跨平台运行时 |
| **分布式框架** | Microsoft Orleans 10 | Actor 模型、集群管理、分布式计算 |
| **Web 框架** | ASP.NET Core | HTTP 服务、中间件、认证授权 |
| **渲染引擎** | SkiaSharp | 跨平台 2D 图形渲染 |
| **空间数据库** | PostgreSQL + PostGIS | 矢量数据存储与空间查询 |
| **瓦片缓存** | 本地文件系统 | 瓦片缓存加速 |
| **动态编译** | Natasha | C# 运行时表达式编译（CQL 过滤） |
| **坐标系** | ProjNET | 坐标参考系（CRS）转换 |
| **GIS 格式** | NetTopologySuite | GeoJSON、几何对象处理 |
| **日志** | Serilog | 结构化日志 |
| **配置** | SocoDB / JSON 文件 | 灵活的配置管理 |

---

## 相关链接

- **源码仓库**: [github.com/zmaplab/zserver](https://github.com/zmaplab/zserver)
- **在线文档**: [docs.zmap.xyz](https://docs.zmap.xyz)
- **问题反馈**: [Issues](https://github.com/zmaplab/zserver/issues)
- **发布版本**: [Releases](https://github.com/zmaplab/zserver/releases)
- **设计思路**: [design.md](https://github.com/zmaplab/zserver/blob/main/design.md)
- **技术引用**: [references.md](https://github.com/zmaplab/zserver/blob/main/references.md)

---

## 许可证

Apache License 2.0. Copyright © 2023-2026 zmap lab.

*ZServer 由 [zmap lab](https://github.com/zmaplab) 开发维护*
