# 服务标准
* [WMTS](https://www.ogc.org/standards/wmts)
* [WMS](https://www.ogc.org/standards/wms)
* [xyz](https://gist.github.com/tmcw/4954720)
* [tms](Tile_Map_Service_Specification)
# 样式定义标准
* [mapbox style](https://docs.mapbox.com/mapbox-gl-js/style-spec/)
* [mapnik configuration xml](https://github.com/mapnik/mapnik/wiki/XMLConfigReference)
* [sld](https://docs.geoserver.org/latest/en/user/styling/sld/reference/index.html)
* [css](https://docs.geoserver.org/latest/en/user/styling/css/index.html)，目前只在GeoServer里见过对它的支持
* [ysld](https://docs.geoserver.org/latest/en/user/styling/workshop/ysld/ysld.html)，似乎只是SLD的YAML格式表达,支持与SLD的无损转换
* mxd，esri公司的样式定义，比较常用，[似乎不能无损转换到sld](https://arcmap2sld.i3mainz.hs-mainz.de/ArcMap2SLDConverter_Eng.htm)
# 常用瓦片格式-矢量
* [.mvt](https://github.com/mapbox/vector-tile-spec),mapbox vector tile.
* [.pbf是gzip压缩后的的mvt](https://github.com/mapbox/mbtiles-spec/blob/master/1.3/spec.md)
* [geojson](https://datatracker.ietf.org/doc/html/rfc7946)
* [topojson](https://github.com/topojson/topojson)


# 常用瓦片格式-栅格
.jpg   .png ....略

# 渲染工具库
* [mapnik](https://mapnik.org/)
* [mapnik-node](https://github.com/mapnik/node-mapnik)
* [mapnik-python](https://github.com/mapnik/python-mapnik)
* [mapnik-java](https://mapnik.org/)
* [VectorTileRenderer](https://github.com/AliFlux/VectorTileRenderer)，一个.NET矢量瓦片渲染库，支持根据mapbox样式定义渲染MVT
* [net-mapnik](https://github.com/jbrwn/NET-Mapnik)，mapnik 的.NET第三方移植库，是否支持跨平台未知，很久未更新
* [geotools](https://github.com/geotools/geotools)

# 矢量数据源
* [shapefile](https://zh.wikipedia.org/zh-hans/Shapefile)
* esri gdb
* [geopackage](https://www.geopackage.org/)
* [geojson](https://datatracker.ietf.org/doc/html/rfc7946)
* [topojson](https://github.com/topojson/topojson)
* [.mvt](https://github.com/mapbox/vector-tile-spec)
* [pbf,gzip压缩后的的mvt](https://github.com/mapbox/mbtiles-spec/blob/master/1.3/spec.md)
* [mbtiles](https://github.com/mapbox/mbtiles-spec)
* [postgis](https://github.com/postgis/postgis)
* [oracle spatial](https://www.oracle.com/database/spatial/)
* mongodb
* mysql
* spatial lite，基于sqlite的空间数据库
https://my.oschina.net/u/3185947/blog/4819218

