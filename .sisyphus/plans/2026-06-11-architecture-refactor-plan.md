# ZServer 架构重构实施计划

基于设计文档 `docs/superpowers/specs/2026-06-11-architecture-refactor-design.md`

## 工作单元

### Phase 1: 基础设施（无风险，可并行）

| # | 任务 | 依赖 | 验证 |
|---|------|------|------|
| 1a | 抽 `ZMap.SLD` — 将 112 个自动生成 XSD 类移到独立项目 `ZMap.SLD/ZMap.SLD.csproj`，旧 ZMap 通过 `TypeForwardedTo` 重导出 | 无 | `dotnet build` |
| 1b | 抽 `ZMap.Rendering.Abstractions` — IGraphicsService, IGraphicsServiceProvider, Viewport, GridBuffer | 无 | `dotnet build` |
| 1c | 抽 `ZMap.Source.Abstractions` — IVectorSource, IRasterSource, ITiledSource, ISource | 无 | `dotnet build` |
| 1d | 清理日志 — 7 个 `Console.WriteLine` 替换为 `Log.CreateLogger<T>()` | 无 | `dotnet build` |
| 1e | 修复同步异步 — 3 个 `.Result`/`.Wait()` 替换为 `await` | 无 | `dotnet build && dotnet test` |

### Phase 2: 核心拆分（依赖 Phase 1）

| # | 任务 | 依赖 | 验证 |
|---|------|------|------|
| 2a | 抽 `ZMap.Core` — Layer, Map, Feature, Envelope, Zoom, ResourceGroup, LayerGroup, Tile | 1b, 1c | `dotnet build` |
| 2b | 抽 `ZMap.Ogc` — WmsService, WmtsService, ParameterValidator, GetFeatureInfo | 2a | `dotnet build` |
| 2c | 抽 `ZMap.Style` — StyleGroup, SldStyleVisitor, IStyleVisitor, style 定义 | 2a | `dotnet build` |

### Phase 3: Store 合并（依赖 Phase 2）

| # | 任务 | 依赖 | 验证 |
|---|------|------|------|
| 3a | `ZServer` 更名 `ZServer.Core`，合并 Store 层：LayerQueryService 单一外观，内部化 LayerGroupStore/LayerStore/StyleGroupStore | 2a, 2b, 2c | `dotnet build && dotnet test` |
| 3b | 插件架构：每个 Source/Renderer 提供 `AddXxx()` 扩展方法，ZServer.Core 统一编排，移除 ZServer.API 对具体 Source 的硬引用 | 3a | `dotnet build` |

### Phase 4: Orleans 配置清理（依赖 Phase 3）

| # | 任务 | 依赖 | 验证 |
|---|------|------|------|
| 4a | `ConfigureSilo` 拆分为 `ConfigureStandalone()``ConfigureClustered()` | 3a | `dotnet build` |
| 4b | `Assembly.Load($"{invariant}")` 替换为类型化注册，SQL 预配抽到 `ClusterProvisioner` | 4a | `dotnet build` |
| 4c | 仪表盘端口合并：单机模式用 `app.UseOrleansDashboard('/dashboard')` 中间件共享 API 端口 | 4a | 手动验证 `/dashboard` 可达 |

### Phase 5: 可空启用（可并行与其他 Phase）

| # | 任务 | 依赖 | 验证 |
|---|------|------|------|
| 5a | 移除 `package.props` 的 `<Nullable>disable</Nullable>`，各新包自行 `<Nullable>enable</Nullable>` | 各包创建完成后 | `dotnet build` |
| 5b | JSON 反序列化等确实需要禁用可空的文件加 `#nullable disable` | 5a | `dotnet build` |

### Phase 6: 收尾

| # | 任务 | 依赖 | 验证 |
|---|------|------|------|
| 6a | 旧 `ZMap` 项目归档，所有消费者已迁移 | 所有 Phase 2-3 | `dotnet build ZServer.sln` |
| 6b | 更新 AGENTS.md 反映新项目结构 | 6a | 阅读检查 |

## 执行顺序

```
Phase 1 ──► Phase 2 ──► Phase 3 ──► Phase 4 ──► Phase 6
                │                     │
                └── Phase 5 (并行) ───┘
```

Phase 1 内部可并行（1a~1e 互不依赖）。
Phase 5 可与 Phase 2~4 并行推进。
每个步骤完成后执行 `dotnet build` 验证，`dotnet test` 检查回归。
