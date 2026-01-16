# NetworkPlayer

## 分层约定

- `NetworkPlugin.Network.NetworkPlayer.dto.*`：DTO（传输/序列化载体）
  - 只承载可序列化字段（例如 `System.Text.Json` 的 `JsonPropertyName`）
  - 不承担运行时同步机制，不引入 `SyncVar<T>`

- `NetworkPlugin.Network.NetworkPlayer.*`：运行时玩家模型/适配层（供补丁/逻辑读取）
  - 可封装运行时引用（例如 `VisitingNode`）
  - 如需“值变更触发同步/事件”，应通过 `PlayerEntity` + `SyncVar<T>` 或快照同步路径实现

## 2026-01-16 修复：VisitingNode 空引用

问题：
- `NetWorkPlayer` 构造函数中直接读取 `VisitingNode.X/Y`，当 `VisitingNode` 为 `null` 时会触发 `NullReferenceException`。

处理：
- 在初始化 `location_X/location_Y` 时，对 `VisitingNode` 做判空。
- 当节点不可用时回退为 `0`，保持 JSON 协议字段类型不变。

涉及文件：
- `networkplugin/Network/NetworkPlayer/dto/NetWorkPlayer.cs`
- `networkplugin/Network/NetworkPlayer/NetWorkPlayer.cs`

## 2026-01-16 文档：INetworkPlayer 注释补齐

目的：
- 完善 `INetworkPlayer` 的 XML 文档注释，降低联机字段误用与后续重构风险。

要点：
- 解释 `updateServer` 参数：控制本地更新后是否向服务器提交/广播（以实现为准）。
- 说明 `mana` 为“协议侧简化视图”，与原游戏 `ManaColor/ManaGroup` 的完整表达存在差异。
- 明确 `ultimatePower` 以 bool 表示“终极技能可用/充能完成状态”。
- 明确 `UpdateLocation(MapNode ...)` 的 `MapNode` 来自 `LBoL.Core`，用于同步“正在访问节点”。
- 对 `GetMyself` 标注预期弃用方向与建议替代方式（由上层上下文获取）。

涉及文件：
- `networkplugin/Network/NetworkPlayer/INetworkPlayer.cs`
