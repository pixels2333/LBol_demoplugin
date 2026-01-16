# task

目标：将 `stance` 相关实现迁移为 `mood`（原游戏语义），并保持联机协议可用。

## 任务清单

### 1. 接口层迁移
- [ ] 在 `networkplugin/Network/NetworkPlayer/INetworkPlayer.cs` 新增 `mood` 与 `UpdateMood(bool)`。
- [ ] 保留 `stance`/`UpdateStance` 作为兼容入口，并在注释中说明后续弃用计划（不强制删除）。

### 2. 实现层迁移
- [ ] 在 `networkplugin/Network/NetworkPlayer/LocalNetworkPlayer.cs`：新增 `mood` 属性；将 `stance` 映射为同一字段（避免双存储）。
- [ ] 在 `networkplugin/Network/NetworkPlayer/RemoteNetworkPlayer.cs`：同上。
- [ ] 确保 `UpdateStance` 与 `UpdateMood` 行为一致（可互相转调）。

### 3. DTO/协议迁移
- [ ] 在 `networkplugin/Network/NetworkPlayer/dto/NetWorkPlayer.cs`：将 `JsonPropertyName("stance")` 改为 `JsonPropertyName("mood")`。
- [ ] 增加对旧字段 `"stance"` 的兼容反序列化（例如新增 `legacy_stance` 并在 setter 写入 `mood`；或使用 `JsonExtensionData`）。
- [ ] 明确优先级：同时出现时以 `mood` 为准。

### 4. 构建验证
- [ ] 运行 `dotnet build networkplugin/NetWorkPlugin.csproj -c Release`。

## 验收标准
- [ ] 代码主语义统一为 `mood`。
- [ ] DTO/协议以 `"mood"` 为主字段，仍可读取旧 `"stance"`。
- [ ] 编译通过。

## 执行
- 使用 `~exec` 执行本方案包：`helloagents/plan/202601161657_networkplayer_stance_to_mood/`
