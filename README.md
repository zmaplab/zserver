# zserver

基于 Actor 模型实现的分布式计算地图服务器，符合 OpenGIS Web 的服务器规范

[![Docker Image CI](https://github.com/zmaplab/zserver/actions/workflows/docker-image.yml/badge.svg)](https://github.com/zmaplab/zserver/actions/workflows/docker-image.yml)

![DESIGN IMAGE](https://github.com/zmaplab/zserver/blob/main/img/design.jpg?raw=true)

## 开发说明

### 环境依赖

+ Docker

```
https://docs.docker.com/engine/install/
```

+ PostgreSQL + PostGIS

```
docker run --name postgis -p 5432:5432 -e POSTGRES_PASSWORD=1qazZAQ! -d postgis/postgis
```

+ dotnet sdk 5.0 或以上

```
https://dotnet.microsoft.com/download
```

+ nodejs

```
https://nodejs.org/zh-cn/
```

+ Parcel

```
yarn global add parcel-bundler 或者 npm install -g parcel-bundler
```

### 启动步骤

+ 安装 PostgreSQL
```
docker run --name postgis -p 5432:5432 -e POSTGRES_PASSWORD=1qazZAQ! -d postgis/postgis
```
+ 创建数据库并添加 postgis 扩展

```
create database zserver_dev;
create extension postgis;
```
+ 上传 Demo 数据

通过 QGIS/ArcGIS 打开 ZServer.SiloHost/shapes/polygon.shp 并上传到 zserver_dev 数据库中， 表名为 polygon

+ 启动 ZServer.API
+ 启动 Web

```
1. 用 VSC 打开 src/Web 后
2. 在终端 yarn install
3. 在终端 yarn dev
```

+ 访问 localhost:3000 切换 wmts wms 测试查看效果

## Roadmap

### 1.0 

1. 完整 OGC 规范的 WMS 服务
2. 完整 OGC 规范的 WMTS 服务（实时渲染）
3. SLD 的支持

### 2.0 

1. 管理界面
2. 支持影像数据的实时渲染/切片
3. 数据基本操作接口(CRUD）
4. 图形操作（切割、合并、相交）等

 
