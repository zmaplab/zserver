# API 文档

**项目名称**：ZServer - 分布式地图瓦片服务器
**作者**：Lewis Zou
**日期**：2026-07-27
**版本**：1.26.727.236

## 变更日志

| 版本 | 日期 | 作者 | 变更内容 |
|------|------|------|----------|
| 1.26.727.236 | 2026-07-27 | Lewis Zou | 初始版本 |

---

## 一、接口总览

| 接口路径 | 方法 | 说明 | 认证 |
|----------|------|------|------|
| `/wms` | GET | WMS 服务（GetMap / GetFeatureInfo） | 默认策略 |
| `/wmts` | GET | WMTS 瓦片服务（GetTile） | 默认策略 |
| `/xyz/{layers}` | GET | XYZ 瓦片服务 | 默认策略 |
| `/api/v1.0/tools/crs_authority` | POST | CRS Authority 查询工具 | 默认策略 |
| `/healthz` | GET | 健康检查 | 匿名 |
| `/dashboard` | GET | Orleans Dashboard | 匿名 |

> **认证说明**：默认策略为 JWT Bearer Token，scope 需包含 `apiName`（默认 `zserver-api`）。可通过 `EnableAuthorization=false` 关闭认证。

---

## 二、WMS 服务

### 2.1 接口地址

```
GET /wms
```

### 2.2 GetMap — 地图渲染

**请求参数**：

| 参数名 | 类型 | 必填 | 说明 | 取值说明 |
|--------|------|------|------|----------|
| `request` | string | 是 | 请求类型 | 固定值：`GetMap` |
| `layers` | string | 是 | 图层名称 | 支持两种格式：<br>1. 资源组图层：`resourceGroup:layer`<br>2. 直接访问：`layer`<br>多图层用逗号分隔 |
| `styles` | string | 否 | 样式名称 | 逗号分隔，与 layers 顺序对应 |
| `srs` | string | 是 | 空间参考系统 | `EPSG:4326`、`EPSG:3857`、`EPSG:900913` 等 |
| `bbox` | string | 是 | 地理范围 | 格式：`minX,minY,maxX,maxY` |
| `width` | int | 是 | 输出图像宽度 | 像素值 |
| `height` | int | 是 | 输出图像高度 | 像素值 |
| `format` | string | 否 | 输出格式 | 默认 `image/png`，支持 `image/png`、`image/jpeg`、`image/webp` |
| `transparent` | bool | 否 | 是否透明背景 | 默认 `false` |
| `bgColor` | string | 否 | 背景色 | 十六进制，如 `0xFFFFFF` |
| `time` | int | 否 | 时间戳 | 预留参数，暂未实现 |
| `Z_FILTER` | string | 否 | 过滤条件 | SQL WHERE 条件，如 `name='测试'` |
| `FORMAT_OPTIONS` | string | 否 | 格式选项 | WMS 格式参数，如 `dpi:96` |
| `buffer` | int | 否 | 缓冲区大小 | 默认 0 |
| `bordered` | bool | 否 | 是否添加边框 | 默认 `false` |

**请求示例**：

```http
GET /wms?request=GetMap&layers=polygon&srs=EPSG:4326&bbox=123.3984375,27.7734375,126.9140625,31.2890625&width=800&height=600&format=image/png&transparent=true
Authorization: Bearer <token>
```

**成功响应**：
```
HTTP/1.1 200 OK
Content-Type: image/png
Content-Length: <length>

<二进制图片数据>
```

**异常响应**（WMS XSD 规范格式）：
```json
{
  "success": false,
  "msg": "错误信息",
  "code": "错误码",
  "locator": "出参参数名"
}
```

**常见错误码**：

| Code | 说明 |
|------|------|
| `LayerNotDefined` | 请求的图层不存在 |
| `StyleNotDefined` | 请求的样式不存在 |
| `InvalidCRS` | 不支持的空间参考 |
| `InvalidBBox` | 无效的地理范围 |
| `InvalidDimension` | 无效的图像尺寸 |
| `QueryLayerError` | 图层查询失败 |
| `403` | 权限不足 |
| `InternalError` | 系统内部错误 |

### 2.3 GetFeatureInfo — 要素查询

**请求参数**：

| 参数名 | 类型 | 必填 | 说明 | 取值说明 |
|--------|------|------|------|----------|
| `request` | string | 是 | 请求类型 | 固定值：`GetFeatureInfo` |
| `layers` | string | 是 | 查询图层 | 逗号分隔 |
| `srs` | string | 是 | 空间参考系统 | `EPSG:4326` 等 |
| `bbox` | string | 是 | 父地图范围 | `minX,minY,maxX,maxY` |
| `width` | int | 是 | 父地图宽度 | 像素值 |
| `height` | int | 是 | 父地图高度 | 像素值 |
| `x` | float | 是 | 查询点 X 坐标 | 像素坐标（左上角为原点） |
| `y` | float | 是 | 查询点 Y 坐标 | 像素坐标（左上角为原点） |
| `featureCount` | int | 否 | 最大返回要素数 | 默认 1 |
| `format` | string | 否 | 输出格式 | 默认 JSON |
| `Z_FILTER` | string | 否 | 过滤条件 | SQL WHERE 条件 |

**请求示例**：

```http
GET /wms?request=GetFeatureInfo&layers=polygon&srs=EPSG:4326&bbox=123.3984375,27.7734375,126.9140625,31.2890625&width=800&height=600&x=400&y=300&featureCount=5
Authorization: Bearer <token>
```

**成功响应**：
```json
{
  "type": "FeatureCollection",
  "features": [
    {
      "type": "Feature",
      "geometry": {
        "type": "Polygon",
        "coordinates": [[[...]]]
      },
      "properties": {
        "___layer": "polygon",
        "name": "地块A",
        "type": "耕地",
        "area": 1234.56
      }
    }
  ]
}
```

> 注意：每个返回的 Feature 在属性中包含 `___layer` 字段，标识该要素来源于哪个图层。

---

## 三、WMTS 服务

### 3.1 接口地址

```
GET /wmts
```

### 3.2 GetTile — 瓦片获取

**请求参数**：

| 参数名 | 类型 | 必填 | 说明 | 取值说明 |
|--------|------|------|------|----------|
| `layer` | string | 是 | 图层名称 | 支持多图层逗号分隔，如 `layer1,layer2` |
| `style` | string | 否 | 样式名称 | 默认 `default` |
| `tileMatrix` | string | 是 | 瓦片矩阵级别 | 缩放级别标识，如 `5` |
| `tileRow` | int | 是 | 瓦片行号 |  |
| `tileCol` | int | 是 | 瓦片列号 |  |
| `format` | string | 否 | 瓦片格式 | 默认 `image/png` |
| `tileMatrixSet` | string | 是 | 瓦片矩阵集 | `EPSG:4326`、`EPSG:3857` 等 |
| `Z_FILTER` | string | 否 | 过滤条件 | SQL WHERE 条件，多图层用分号分隔 |
| `bordered` | bool | 否 | 是否添加边框 | 默认 `false` |

**请求示例**：

```http
GET /wmts?layer=polygon&style=default&tileMatrix=5&tileRow=10&tileCol=20&format=image/png&tileMatrixSet=EPSG:4326
Authorization: Bearer <token>
```

**成功响应**：
```
HTTP/1.1 200 OK
Content-Type: image/png
Content-Length: <length>

<瓦片图片二进制数据>
```

**异常响应**：
```json
{
  "success": false,
  "msg": "错误信息",
  "code": "错误码"
}
```

**常见错误码**：

| Code | 说明 |
|------|------|
| `InvalidTileMatrix` | 无效的瓦片矩阵级别 |
| `InvalidTileRow` | 无效的瓦片行号 |
| `InvalidTileCol` | 无效的瓦片列号 |
| `LayerNotDefined` | 图层不存在 |
| `TileMatrixSetNotDefined` | 瓦片矩阵集不存在 |
| `FilterDefinedError` | 过滤器数量与图层数量不匹配 |
| `StyleDefinedError` | 样式数量与图层数量不匹配 |
| `WMTSKeyIsEmpty` | WMTS 缓存的 Key 为空 |

---

## 四、XYZ 服务

### 4.1 接口地址

```
GET /xyz/{layers}?x={x}&y={y}&z={z}
```

### 4.2 请求参数

| 参数名 | 类型 | 位置 | 必填 | 说明 |
|--------|------|------|------|------|
| `layers` | string | 路由参数 | 是 | 图层名称 |
| `x` | int | 查询参数 | 是 | 瓦片列号 |
| `y` | int | 查询参数 | 是 | 瓦片行号 |
| `z` | string | 查询参数 | 是 | 缩放级别 |
| `format` | string | 查询参数 | 否 | 瓦片格式，默认 `image/png` |
| `tileMatrixSet` | string | 查询参数 | 否 | 瓦片矩阵集，默认 `3857`（会自动补全为 EPSG:3857） |
| `style` | string | 查询参数 | 否 | 样式名称 |
| `Z_FILTER` | string | 查询参数 | 否 | 过滤条件 |
| `bordered` | bool | 查询参数 | 否 | 是否添加边框 |

**请求示例**：

```http
GET /xyz/polygon?x=123&y=456&z=15&format=image/png
Authorization: Bearer <token>
```

**成功响应**：
```
HTTP/1.1 200 OK
Content-Type: image/png

<瓦片图片二进制数据>
```

> **实现说明**：XYZ 服务默认使用 EPSG:3857 坐标系（90% 的 XYZ 服务使用此投影系），内部通过参数映射复用 WMTS 的处理逻辑和缓存。

---

## 五、工具服务

### 5.1 CRS Authority 查询

**接口地址**：

```
POST /api/v1.0/tools/crs_authority
```

**请求头**：
```
Content-Type: text/plain
Authorization: Bearer <token>
```

**请求体**：WKT 格式的坐标参考系统定义

**请求示例**：

```http
POST /api/v1.0/tools/crs_authority
Content-Type: text/plain
Authorization: Bearer <token>

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

**实现说明**：
- 使用 ProjNET 的 `CoordinateSystemWktReader` 解析 WKT
- 结果缓存 5 分钟（IMemoryCache）
- 首先尝试获取 WKT 中的 Authority 信息
- 若无权威信息，遍历内部 SRID 缓存进行坐标系统比较

---

## 六、健康检查

### 6.1 接口地址

```
GET /healthz
```

**匿名访问**，无需认证。

**响应示例**：HTTP 200，表示服务正常运行。

可通过环境变量 `HEALTH_CHECK_PATH` 自定义健康检查路径。

---

## 七、Dashboard

### 7.1 接口地址

```
GET /dashboard
```

**匿名访问**，无需认证。

Orleans Dashboard 提供集群运行时监控信息：
- Silo 节点列表与状态
- Grain 激活统计
- 请求处理性能指标
- 集群网络延迟

---

## 八、全局异常响应格式

ZServer 使用统一的异常响应格式：

```json
{
  "success": false,
  "msg": "错误描述",
  "code": "错误码",
  "locator": "导致错误的参数名（可选）"
}
```

| 错误码 | HTTP 状态码 | 说明 |
|--------|-------------|------|
| `LayerNotDefined` | 404 | 图层不存在 |
| `StyleNotDefined` | 404 | 样式不存在 |
| `InvalidCRS` | 400 | 坐标系无效 |
| `InvalidBBox` | 400 | 地理范围无效 |
| `InvalidDimension` | 400 | 图像尺寸无效 |
| `InvalidTileMatrixSet` | 400 | 瓦片矩阵集无效 |
| `403` | 403 | 权限不足 |
| `InternalError` | 500 | 系统内部错误 |
| `-1` | 400 | WKT 解析失败 |
| `404` | 404 | 疑似非标准 CRS |
