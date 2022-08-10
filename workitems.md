# Work items

| 任务 | 负责人 | 是否完成 | 备注 |
| --- | --- | --- | --- |
| Workspace 级别不能配置服务的开放， Layer 级别可以配置服务开放的类型(WMS, WMTS), Layer 可以配置 GridSet 类型组（可以是多个）  |  |  [ ] |
| LayerGroup 调整为和 Layer 同级别的配置， 而不是通过 type 来区分 | zlzforever |  [ ] |
| WMS GetMap 如异常格式、STYLES、BGCOLOR 等完整支持 | zlzforever |  [ ] |
| WMS GetCapabilities |  |  [ ] |
| WMTS GetCapabilities |  |  [ ] |
| WMTS GetTile 如 STYLE 等完整支持 | zlzforever |  [ ] |
| SLD 解析成画刷 |  |  [ ] |
| 缓存接口以及本地文件、OSS、Minio、MongoDB 支持 | OSS、Minio |  [ ] |
| 添加图形 |  计算影响到的 LayerGroup, Layer 通过是否支持 WMTS， 是否缓存， GridSets 等后重新渲染完成后接口返回 |  [ ] |
| 删除图形 |  计算影响到的 LayerGroup, Layer 通过是否支持 WMTS， 是否缓存， GridSets 等后重新渲染完成后接口返回 |  [ ] |
| 更新图斑数据 | 计算影响到的 LayerGroup, Layer 通过是否支持 WMTS， 是否缓存， GridSets 等后重新渲染完成后接口返回 |  [ ] |
| 重新渲染图片的接口，提供图层，以及影响的 ID | |  [ ] |
| 重新渲染图片的接口，提供 BBOX | |  [ ] |
| 重新渲染图片的接口，提供 XYZ/WMTS | |  [ ] |
| WMS GetFeatureInfo |  |  [ ] |
| WMTS GetFeatureInfo |  |  [ ] |
