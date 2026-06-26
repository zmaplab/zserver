# ZServer 整体架构重构方案

**日期**: 2026-06-11 | **状态**: 草稿 | **方案**: B — 模块化单体

## 目标

1. **代码组织** — 拆分 ZMap（204 文件）为职责清晰的独立包
2. **性能/扩展准备** — 渲染器和数据源实现真正的插件化，为未来的 Orleans 分布式做准备
3. **技术债务** — 启用可空检查、修复同步异步混用、统一日志、清理 Orleans 配置

## 第一节：包拆分

### 当前状态

16 个项目，ZMap 单个项目 204 个文件，混入了 8 种职责。

### 目标结构

```
src/
├── ZMap.Core/                    # Layer, Map, Feature, Envelope, Zoom, ResourceGroup
│                                 # ~15 文件 — 纯领域模型，无基础设施依赖
├── ZMap.Ogc/                     # WMS/WMTS 协议解析、请求参数校验
│   ├── Wms/                      # WmsService, ParameterValidator
│   └── Wmts/                     # WmtsService, 瓦片矩阵计算
│                                 # 依赖: ZMap.Core, ZMap.TileGrid
├── ZMap.Style/                   # StyleGroup, SldStyleVisitor, IStyleVisitor, 样式定义
│                                 # 依赖: ZMap.Core, ZMap.DynamicCompiler
├── ZMap.Rendering.Abstractions/  # IGraphicsService, IGraphicsServiceProvider, Viewport
│                                 # 依赖: ZMap.Core
├── ZMap.SLD/                     # 自动生成的 XSD 类（112 文件，从 ZMap 移出）
│                                 # 依赖: 无（纯数据模型）
├── ZMap.Source.Abstractions/     # IVectorSource, IRasterSource, ITiledSource, ISource
│                                 # 依赖: ZMap.Core
├── ZMap.TileGrid/                # 保持不变 — GridSet, GridSetFactory
├── ZMap.DynamicCompiler/         # 保持不变 — Natasha 运行时编译
│
├── ZMap.Renderer.SkiaSharp/      # 保持不变 — SkiaSharp 实现
├── ZMap.Source.Postgre/          # 保持不变
├── ZMap.Source.ShapeFile/        # 保持不变
├── ZMap.Source.GDAL/             # 保持不变
├── ZMap.Source.CloudOptimizedGeoTIFF/  # 保持不变
│
├── ZServer.Core/                 # Store 层 + DI 组合根
│   ├── Store/                    # LayerStore, SourceStore, GridSetStore, LayerQueryService
│   └── Extensions/               # 服务注册、插件编排
│                                 # 依赖: ZMap.Core, ZMap.Ogc, ZMap.Style,
│                                 #   ZMap.Rendering.Abstractions, ZMap.Source.Abstractions
├── ZServer.Interfaces/           # 保持不变 — Orleans 契约接口
├── ZServer.Grains/               # 保持不变 — Orleans 实现
├── ZServer.Silo/                 # 保持不变（Orleans 配置在第三节 d 清理）
├── ZServer.SiloHost/             # 保持不变
├── ZServer.API/                  # Web 主机 — 只依赖 ZServer.Core（不直接引用原始 Source）
└── ZServer.Tests/                # 按包划分的单元测试
```

### 迁移顺序

1. 先抽 `ZMap.SLD` — 纯移动，无逻辑变更，零风险
2. 再抽 `ZMap.Rendering.Abstractions` — IGraphicsService, IGraphicsServiceProvider, Viewport
3. 再抽 `ZMap.Source.Abstractions` — IVectorSource, IRasterSource, ISource
4. 再抽 `ZMap.Core` — Layer, Map, Feature, Envelope, Zoom, ResourceGroup
5. 再抽 `ZMap.Ogc` — WmsService, WmtsService, ParameterValidator
6. 再抽 `ZMap.Style` — StyleGroup, SldStyleVisitor, IStyleVisitor
7. 剩余文件（Extensions, Indexing, Infrastructure, Permission）留在旧 ZMap。旧 ZMap 通过 `[assembly: TypeForwardedTo]` 重导出已移走的类型，保持现有引用不中断
8. 所有消费者迁移到新包后，旧 ZMap 项目归档
9. `ZServer` 更名为 `ZServer.Core`，Store 层合并

## 第二节：插件架构

### 问题

`ZServer.csproj` 硬引用了 `ZMap.Renderer.SkiaSharp` 和 `ZMap.Source.ShapeFile`。
`ZServer.API.csproj` 硬引用了各种 Source 实现。
新增一个渲染器或数据源要改多个项目。

### 方案

每个插件提供独立的 DI 注册扩展方法，`ZServer.Core` 统一编排。

**插件模式**：
```csharp
// 每个插件暴露：
public static IServiceCollection AddXxx(this IServiceCollection services) { ... }

// ZServer.Core 统一注册：
services.AddSkiaSharpRenderer();
services.AddPostgreSource();
services.AddShapeFileSource();
services.AddCloudOptimizedGeoTIFFSource();
```

**依赖流向**：
```
ZServer.API → ZServer.Core → ZMap.*.Abstractions（编译期）
ZServer.Core → ZMap.Renderer.SkiaSharp, ZMap.Source.*（运行时，通过 DI）
```

**优点**：
- 新渲染器（如 ImageSharp）？加一个包 + 一个扩展方法，搞定
- 新数据源？同一模式
- API/ZServer 不再编译期依赖具体实现
- 可测试性：单元测试通过 DI 替换渲染器为桩

## 第三节：技术债务

### 3a. 启用可空检查 — 渐进式

- 移除 `package.props` 中的 `<Nullable>disable</Nullable>`
- 每个新包在自己的 `.csproj` 中启用 `<Nullable>enable</Nullable>`
- 确实需要禁用可空的文件（如 JSON 配置反序列化）用 `#nullable disable` 按文件关闭
- 旧 ZMap 在迁移完成前保持 `#nullable disable`

### 3b. 修复同步异步混用

3 个文件使用了 `.Result`/`.Wait()` — 替换为 `await` 上溯调用链。
可能位置：Orleans 引导（同步 silo 配置）、GDAL 互操作。

### 3c. 统一日志

7 个文件使用了 `Console.WriteLine`（主要在 Console/Client 示例项目）。
替换为 `Log.CreateLogger<T>()`，使用已有 Serilog 基础设施。

### 3d. 清理 Orleans 配置（`OrleansExtensions.cs`，166 行）

- 将 `Assembly.Load($"{invariant}")` 替换为类型化的 ADO.NET 集群注册
- 将 SQL 预配逻辑抽到独立 `ClusterProvisioner` 服务
- 将 `ConfigureSilo` 拆分为 `ConfigureStandalone()` / `ConfigureClustered()`
- **仪表盘端口合并**：将 `ISiloBuilder.UseDashboard()`（独立 Kestrel 端口）替换为 `app.UseOrleansDashboard()` 中间件，挂在 API 同一端口的 `/dashboard` 路径下。前置网关无需再配两个端口转发
  - 单机模式：从 silo 配置中移除 `UseDashboard()`，在 `Program.cs` API 管道中添加中间件
  - 集群模式：SiloHost 仍使用 `UseDashboard()` 独立端口（独立进程，无 ASP.NET Core 主机）

## 第四节：Store 层合并

### 问题

`LayerQueryService` 委托给 `LayerGroupStore` + `LayerStore` + `StyleGroupStore`。
"按 resourceGroup:layerName 查找"的逻辑在 3 个类中重复。
样式设置与图层解析交织在一起。

### 方案

`LayerQueryService` 成为唯一的公开入口。
`LayerGroupStore`、`LayerStore`、`StyleGroupStore` 改为内部实现。
从 `ILayerGroupStore` 移除 `IRefresher` — 刷新由 `RefreshConfigService` 统一处理。

```
LayerQueryService ──► WmsService/WmtsService（单一外观）
  └── 内部: LayerGroupStore, LayerStore, StyleGroupStore
```

## 不纳入范围

- Orleans 粒度分布式（方案 C — 延后）
- 瓦片缓存策略变更
- 前端（Web/）变更
- Client/Console 示例更新（尽力而为）

## 风险

| 风险 | 应对 |
|------|------|
| 包拆分导致现有导入路径损坏 | 渐进抽取；旧 ZMap 通过 `[assembly: TypeForwardedTo]` 重导出已移类型 |
| 插件 DI 导致启动失败 | 每一步抽取后用 `dotnet build && dotnet test` 验证 |
| 启用可空发现隐藏缺陷 | 按包启用，非全量开启；已有测试覆盖回归 |
| Store 合并改变行为 | `LayerQueryService` 接口不变，仅内部重构 |
