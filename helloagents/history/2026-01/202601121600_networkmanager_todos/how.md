# 技术设计: NetworkManager TODO 清理与补齐

## 技术方案

### 核心技术
- C# / .NET（现有解决方案 `simplesolution.sln`）
- `System.Text.Json`（复用 `NetworkManager` 内既有解析辅助方法）
- 线程安全：复用 `_playersLock` + “快照脏标记”机制

### 实现要点
- 以“当前架构”为准：玩家身份与玩家列表来源由 `NetworkIdentityTracker`（从 `Welcome/PlayerListUpdate/...` 提取）驱动；`NetworkManager` 在 `OnGameEventReceived` 中同步并按 payload 更新玩家信息。
- 对 `TODO` 分两类处理：
  1. **已实现但未清理的 TODO**：移除/改写注释与 XML 文档（不改变逻辑）。
  2. **仍缺失的 TODO**：补齐实现，尽量复用现有私有方法，避免引入新的同步路径。

### TODO 对齐策略（逐项）
1. `GetPlayer(string id)`
   - 现状：已有 `id` 校验 + `SyncPlayersFromIdentityTracker()` + `_players.TryGetValue`。
   - 动作：删除不可达的 TODO 占位注释；将 XML 文档中 “NotImplementedException” 描述改为实际行为说明。
2. `GetSelf()`
   - 现状：直接返回 `_selfPlayer`；本地玩家的 `userName` 已优先使用 `NetworkIdentityTracker.GetSelfPlayerId()`（见 `LocalNetworkPlayer.userName`）。
   - 动作：移除/改写 TODO 注释与“旧实现思路（ClientData.username）”的误导性描述；（可选）在返回前调用 `SyncPlayersFromIdentityTracker()` 以确保 `_selfKey` 在刚连上时完成从 `"self"` 到 `PlayerId` 的切换。
3. `RegisterPlayer(INetworkPlayer player)`
   - 现状：已有 `null` 校验、id 兜底、字典写入与 `MarkPlayersDirty_NoLock()`。
   - 动作：将 TODO 注释改为“当前实现说明”；确认不会因为重复注册导致不必要的快照重建（已通过 `changed` 判断）。
4. `RemovePlayer(string id)`
   - 现状：已有输入校验、禁止移除 self、移除并标记 dirty。
   - 动作：将 TODO 注释改为“当前实现说明”；补充注释解释“资源清理/事件通知”由上层补丁或网络事件驱动处理（本类保持轻量）。
5. `UpdatePlayerInfo(object playerInfo)`
   - 现状：未实现且无引用点。
   - 推荐实现：作为兼容入口，支持以下输入：
     - `JsonElement`：直接处理
     - `string`：按 JSON 解析
     - 其他：尝试 `ToString()` 作为 JSON 再解析，失败则 no-op
   - 处理策略：
     - 若为单个玩家对象：调用 `UpdateSinglePlayer(JsonElement)`
     - 若包含 `Players` 数组或本身为数组：调用 `UpdatePlayersFromArray(JsonElement)`
   - 失败策略：捕获异常并忽略，避免影响网络事件线程。

## 架构设计
不新增模块、不改动网络协议。`NetworkManager` 继续作为“玩家列表/数量”轻量维护层，真实同步由各 `*SyncPatch` 与服务器事件驱动完成。

## 架构决策 ADR
### ADR-001: 以现有事件驱动同步为准，清理陈旧 TODO 并提供兼容入口
**上下文:** `NetworkManager.cs` 中多处 TODO 与现状不一致；旧注释提到的 `ClientData.username`/请求式获取已不存在。  
**决策:** 清理已完成 TODO；`UpdatePlayerInfo` 实现为薄封装并复用 `OnGameEventReceived` 既有解析/更新逻辑。  
**替代方案:** 重新引入请求式 `GetSelf`/独立玩家更新通道 → 拒绝原因：与当前事件驱动同步重复且易造成状态分叉。  
**影响:** 维护成本下降；兼容性更好；风险集中在注释清理与输入解析边界。

## API设计
无

## 数据模型
无

## 安全与性能
- **安全:**
  - 对外输入（`id`、`playerInfo`）保持输入校验与异常吞噬策略，防止恶意 payload 触发崩溃。
  - JSON 解析使用 `System.Text.Json`，不引入反射式动态绑定。
- **性能:**
  - 不引入额外的全量扫描；复用现有 `NetworkIdentityTracker` 与 `GetPlayersSnapshot()` 快照缓存机制。
  - `UpdatePlayerInfo` 仅在被调用时解析，失败快速返回。

## 测试与部署
- **验证建议:**
  - `dotnet build simplesolution.sln`
  - 在联机/断线重连场景下观察 `GetPlayerCount/GetAllPlayers/GetSelf` 行为保持一致（手动验证即可）。
