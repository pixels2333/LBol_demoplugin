# how

## 推荐方案（最小破坏 + 兼容迁移）
采用“新增 mood / stance 作为兼容别名”的迁移策略：

1) 接口层：
- 在 `INetworkPlayer` 新增 `string mood { get; set; }`。
- 新增 `void UpdateMood(bool updateServer)`。
- 保留 `stance` 与 `UpdateStance` 作为兼容入口（可在注释中标注后续弃用），其实现应与 `mood` 互通。

2) 实现层：
- `LocalNetworkPlayer` / `RemoteNetworkPlayer` 新增 `mood` 属性。
- 将原 `stance` 的 getter/setter 映射到同一底层字段（避免双存储）。
- `UpdateStance` 内部调用 `UpdateMood`（或反向），保证行为一致。

3) DTO/协议层：
- 将 `dto/NetWorkPlayer.cs` 的 `JsonPropertyName("stance")` 改为 `JsonPropertyName("mood")`。
- 为兼容旧字段：增加一个仅用于反序列化的兼容属性/字段（例如 `legacy_stance`），在 setter 中写入 `mood`。
  - 说明：System.Text.Json 可通过 `set` 访问器或 `[JsonExtensionData]` 做兼容解析；优先选择对项目现状改动最小的方式。
  - 优先级建议：若同时收到 `mood` 与 `stance`，以 `mood` 为准。

## 不推荐方案
- 直接全仓重命名并删除 `stance`（破坏性大，旧版本混用会失败）。
- 仅改代码命名为 `mood` 但协议字段仍为 `stance`（未来继续困惑）。

## 风险与规避
- 版本混用：旧客户端仍发 `stance` → 新版本需能解析到 `mood`。
- 字段双写冲突：同一消息同时含 `mood` 与 `stance` → 明确优先级。
- 迁移窗口：在完成一次稳定版本发布前，保留 `stance` 作为兼容入口；等所有调用点迁移后再移除。

## 验证
- 编译验证：`dotnet build networkplugin/NetWorkPlugin.csproj -c Release`。
- 最小联机验证（人工）：两端加入同局，触发 Koishi 心境切换，确认远端循环 VFX 正常。
