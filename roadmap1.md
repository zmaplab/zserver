# Roadmap 1 (202108-202110)

OWS （OGC Web Service Common Implementation Specification）
OWS 描述了 Web 服务通用的一些接口规范，包括请求和响应的内容、请求的参数和编码等。目前，OWS 包括 WFS（要素 Web 服务）、WMS（地图 Web 服务）、WCS（栅格 Web 服务）。

## 完整的 WMS 协议

1. GetCapabilities

| 参数 | 名称 | 是否必须 | 备注 |
| --- | --- | --- | --- |
| SERVICE=WMS | 服务名称 | 是 |
|   REQUEST=GetCapabilities   |  请求接口    |   是    |      |
|   VERSION   |  版本号    |   否    |  若为空则反回当前服务器支持的版本    |
|   FORMAT   |  返回类型   |   否    |   默认为 XML   |

2. GetMap

| 参数 | 名称 | 是否必须 | 备注 |
| --- | --- | --- | --- |
| SERVICE=WMS | 服务名称 | 是 |
| REQUEST=GetMap | 请求接口 | 是 |
|   VERSION   |  版本号    |   是    |  若为空则反回当前服务器支持的版本    |
|   LAYERS   |  图层名称   |   是    |   可以一次请求多个图层， 格式： workspace1:layer1,workspace1:layer2   |
|   FORMAT   |  返回类型   |   否    |   默认是 PNG、JPEG、MVT    |
|  STYLES     |  样式类型    |   否     |  和 LAYERS 对应的样式: style1,style2    | 
|  BBOX     |   边界框值   |    是    |      | 
|  SRS OR CRS     |  投影坐标系    |     是    |  1.3 为 CRS， 其它使用 SRS    |
|  WIDTH     |   图片宽度   |    是    |      | 
|   HEIGHT    |  图片高度    |    是    |      | 
|  TRANSPARENT     |  图片是否透明    |  否      |  默认false，不透明    | 
|   BGCOLOR    |   背景颜色   |    否    |      | 
|  TIME     |  请求时间    |    否    |      | 
|  ELEVATION     |  高程，若支持高程    |   否     |      | 
|  EXCEPTIONS     |  异常返回格式    |   否     |   XML, JSON, INIMAGE, BLANK   | 

3. GetFeatureInfo

| 参数 | 名称 | 是否必须 | 备注 |
| --- | --- | --- | --- |
| SERVICE=WMS | 服务名称 | 是 |
| REQUEST=GetFeatureInfo | 请求接口 | 是 |
|   VERSION   |  版本号    |   是    |  若为空则反回当前服务器支持的版本    |
|   QUERY_LAYERS   |  图层名称   |   是    |   可以一次请求多个图层， 格式： workspace1:layer1,workspace1:layer2   |
|   INFO_FORMAT   |  返回类型   |  是    |    XML, JSON    |
|  FEATURE_COUNT     |  结果数量    |   否     |  默认是 1    | 
|  I     |  pixel_column     |    是    |      | 
|  J     |  pixel_row     |     是    |     |
|  EXCEPTIONS     |  异常返回格式    |   否     |   XML, JSON, INIMAGE, BLANK   | 

 feature_count 若是查询多个图层时， 结果如何聚合？ 

## 完整的 WMTS 协议

1. GetCapabilities

| 参数 | 名称 | 是否必须 | 备注 |
| --- | --- | --- | --- |
| SERVICE=WMTS | 服务名称 | 是 |
|   REQUEST=GetCapabilities   |  请求接口    |   是    |      |
|   FORMAT   |  返回类型   |   否    |   默认为 XML   |


2. GetTile

| 参数 | 名称 | 是否必须 | 备注 |
| --- | --- | --- | --- |
| SERVICE=WMTS | 服务名称 | 是 |
| REQUEST=GetTile | 请求接口 | 是 |
|   VERSION   |  版本号    |   是    |  若为空则反回当前服务器支持的版本    |
|   LAYER   |  图层名称   |   是    |   可以一次请求多个图层， 格式： workspace1:layer1,workspace1:layer2   |
|   FORMAT   |  返回类型   |   是    |   默认是 PNG、JPEG、MVT  |
|  STYLE     |  样式类型    |   否     |  和 LAYERS 对应的样式: style1,style2    |
|  TILEMATRIXSET     |   瓦片矩形设置  |    是    |      |
|  TILEMATRIX     |  瓦片矩形    |     是    |     |
|  TILEROW     |   瓦片的行索引   |    是    |      |
|   TILECOL    |  瓦片的列索引    |    是    |      |

3. GetFeatureInfo

| 参数 | 名称 | 是否必须 | 备注 |
| --- | --- | --- | --- |
| SERVICE=WMTS | 服务名称 | 是 |
| REQUEST=GetFeatureInfo | 请求接口 | 是 |
|   VERSION   |  版本号    |   是    |  若为空则反回当前服务器支持的版本    |
|   QUERY_LAYERS   |  图层名称   |   是    |   可以一次请求多个图层， 格式： workspace1:layer1,workspace1:layer2   |
|   INFO_FORMAT   |  返回类型   |  是    |    XML, JSON    |
|  FEATURE_COUNT     |  结果数量    |   否     |  默认是 1    | 
|  I     |  pixel_column     |    是    |      | 
|  J     |  pixel_row     |     是    |     |
|  EXCEPTIONS     |  异常返回格式    |   否     |   XML, JSON, INIMAGE, BLANK   | 

feature_count 若是查询多个图层时， 结果如何聚合？

## 相关配置

### 画刷

1. 可以配置显示比例尺
2. 可以配置生效表达式， 比如当图斑的某个属性值在于 10 时用一种画刷
``` 
feature["height"] > 10
```
3. 画刷可以配置： 轮廓宽度、笔冒样式、字体大小、颜色、画刷样式、字体、文字对齐方式、文字比例因子、文字倾斜因子、特效（暂时只支持 2 种）

画刷的完整配置如下

``` 
 {
    "MinVisible": 100,
    "MaxVisible": 100000,
    "AvailableExpression": "feature[\"height\"] >= 40 "
    "TextSize": 12,
    "Color": "#EA6B66",
    "Style": "Fill",
    "Font": "宋体",
    "StrokeWidth": 4,
    "StrokeCap": "BUTT",
    "TextAlign": "Left",
    "TextScaleX": 0,
    "TextSkewX": 0,
    "PathEffects": [
    {
        "Type": "Dash",
        "Intervals": [4, 4],
        "Phase": 0
    },
    {
        "Type": "Hatch",
        "ScaleX": 1,
        "ScaleY": 16,
         "Width": 4
    }]
 }
```
### 画刷组

1. 一个画刷组包含多个画刷
2. 所有画刷都会根据其比例尺、生效表达式独立渲染
3. 画刷组可以指定工作区也可以不指定工作区， 若指定工作区则其名称为 workspace:name

### 数据源

数据源必须使用 Workspace

#### Shape

1. 配置文件路径

不需要独立配置 SRID， 需要从 SHP 的投影文件中读取。完整配置如下

``` 
{
    "Type": "ZServer.Source.ShapeFileSource, ZServer",
    "Path": "shapes/osmbuildings.shp"
}
```

#### PostGIS

1. 配置连接字符串
2. 配置数据库名

完整配置如下

``` 
{
    "Type": "ZServer.Postgre.PostgreSource, ZServer.Postgre",
    "Database": "berlin",
    "ConnectionString": "User ID=postgres;Password=1qazZAQ!;Host=localhost;Port=5432;Database=postgres;Pooling=true;"
}
```

### 图层

1. 配置显示比例尺
2. 配置图层数据源： shape、gdb、postgis，
3. 图层的 workspace 继承于 datasource

```
a. 若是 SHP 则可以额外配置需要读取的属性名（有于各生效表达式或者标签值计算）
b. 若是空间数据库， 需要配置 SRID、表、ID 列名、图形列名以及需要读取的属性名
```

3. 配置画刷组， 不能独立配置一个画刷
4. 配置标签（多个）
5. 配置生效的服务： WMS, WMTS, XYZ, RESTFUL

#### SHP 完整配置

```
{
    "Type": "Layer",
    "MinVisible": 10,
    "MaxVisible": 128000,
    "Source": {
      "Name": "berlin_shp",
      "Columns": [
        "height",
        "country",
        "city"
      ]
    },
    "PaintGroup": "style",
    "Labels": [
      {
        "Property": "height",
        "PaintGroup": "label_style",
        "AvailableExpression": "feature[\"height\"] > 25"
      },
      {
        "EvalExpression": "feature[\"city\"]",
        "PaintGroup": "label_style"
      }
    ]
}
```

#### PostGIS 完整配置

```
"berlin_db": {
    "Type": "Layer",
    "MinVisible": 500,
    "MaxVisible": 128000,
    "Source": {
      "Name": "berlin_db",
      "Table": "osmbuildings",
      "Geometry": "geom",
      "SRID": 4326,
      "Columns": [
        "height",
        "country",
        "city"
      ]
    },
    "PaintGroup": "style",
    "Labels": [
      {
        "Property": "height",
        "PaintGroup": "label_style",
        "AvailableExpression": "feature[\"height\"] > 25"
      },
      {
        "EvalExpression": "feature[\"city\"]",
        "PaintGroup": "label_style"
      }
    ]
}
```

## 图层组

图层组是个额外的概念， 它并不是一个图层。各种显示等也是由其内容的图层独立控制。

1. 配置多个图层
2. 图层组不独立设置显示比例尺、生效表达式， 完全依赖各图层自已的配置
3. 图层组不独立设置支持的服务（WMS， WMTS）
4. 图层组可以选择工作区也可以不选择工作区， 若指定工作区则其名称为 workspace:name

```
"berlin_group": {
  "Type": "LayerGroup",
  "Layers": [
    "berlin_db"
  ]
},
```

### 标签

1. 配置显示比例尺
2. 配置生效表达式
``` 
feature["height"] > 25
```
3. 配置画刷组， 只能配置画刷组不能配置单独的画刷
4. 配置显示的属性
5. 配置值表达式， 只有属性配置为空是值表达式才会生效。值表达式是 C# 脚本， 如

``` 
feature["heigth"] * feature["width"]
```

完整的标签配置样例如下

```
 {
    "MinVisible": 100,
    "MaxVisible": 100000,
    "Property": "height",
    "PaintGroup": "label_style",
    "AvailableExpression": "feature[\"height\"] > 25",
    "EvalExpression": "feature[\"city\"]"
}
```
6. Label 应该移到 Style 中， 是其中的 TextSymbolizer

##  SLD & mapbox style 支持

支持将 SLD 的渲染

## 缓存

此配置生效在各个 TILED 服务中， 如 WMTSGrain 中。

1. 配置缓存组件（本地文件、OSS、Minio、MongoDB 等）

```
"Cache": {
  "Enabled": true,
  "Type": "File",
  "Directory": ""
},

"Cache": {
  "Enabled": true,
  "Type": "AliOSS",
  "Bucket": "",
  "EndPoint": "oss-cn-shanghai.aliyuncs.com",
  "AccessKeyId": "",
  "AccessKeySecret": "",
  "BaseUrl": "xxx.oss-cn-shanghai.aliyuncs.com"
}
```

## 数据操作接口

### 属性数据操作

1. 修改属性

```
a. 修改某个 feature 的属性
b. 计算其影响的瓦片并进行重新渲染， 缓存

计算影响到的 LayerGroup, Layer 通过是否支持 WMTS， 是否启用缓存， GridSets 等后重新渲染完成后接口返回

```

### 图形操作

1. 添加图形

```
a. 添加 feature， 图形和属性数据都由业务方计算好了， 完全不依赖
b. 计算其影响的瓦片并进行重新渲染， 缓存
```

3. 删除图形

4. 合并图形

5. 相交图形
