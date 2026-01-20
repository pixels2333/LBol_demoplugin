# How - 方案设计

## A. OnTurnEnd 接收落地

### A1. 事件来源与通道
- `OnTurnEnd` 属于 GameEvent（`NetworkClient.IsGameEvent` 规则：`On*`）。
- 发送端当前使用 `INetworkClient.SendRequest(NetworkMessageTypes.OnTurnEnd, json)`，服务端会将 messageType 识别为 GameEvent 并广播给其他玩家（不含发送方）。

### A2. 接收端挂载点
推荐实现一个“轻量接收补丁/管理器”，模式参考：
- `networkplugin/Patch/Network/EndTurnSyncPatch.cs` 的订阅方式：在 `GameDirector.Update` 中 EnsureSubscribed 到 `INetworkClient.OnGameEventReceived`。

拟新增文件：
- `networkplugin/Patch/Network/TurnEndSnapshotReceivePatch.cs`

核心职责：
- 订阅 `INetworkClient.OnGameEventReceived`。
- 过滤 `eventType == NetworkMessageTypes.OnTurnEnd`。
- 将 payload 解析为 `TurnEndStateSnapshot`（考虑 payload 可能是 `Dictionary<string,object>` 或 `JsonElement` 的情况）。
- 根据 payload 中的 `UserName` / `PlayerId`（如有）定位远端玩家对象。
- 将关键字段写入远端玩家状态缓存（优先写入已有结构，若没有则记录到 `NetworkManager` 的扩展缓存字典）。

### A3. 状态落地策略（不侵入）
优先级从“最小侵入”到“更强一致性”：
1) **最小侵入（推荐）**：维护一个 `Dictionary<string, TurnEndStateSnapshot>` 缓存（key = playerId 或 userName），供 UI/调试读取。
2) **中等侵入**：如果 `RemoteNetworkPlayer` 有可写的状态字段（例如 `StateSnapshot`/`LastKnownHp` 等），则同步写入。
3) **高侵入（本次不做）**：直接对 LBoL 的本地 Battle 状态施加变更。

### A4. 与 EndTurnSyncPatch 的边界
- `EndTurnSyncPatch`：负责“什么时候允许结束回合”（协商与 UI 锁定）。
- `OnTurnEnd`：负责“回合已结束后的状态快照”（供远端对齐/展示/核对）。
二者互不依赖：接收端不应根据 `_localEndedTurn` 做过滤。

---

## B. 修复 INetworkPlayer.mana 编译错误

### B1. 问题定位
编译错误表明：`INetworkPlayer` 接口不包含 `mana`，但以下位置直接访问了该属性：
- `networkplugin/Patch/Actions/PlayCardAction_Patch.cs`
- `networkplugin/Network/Reconnection/ReconnectionManager.cs`

### B2. 修复策略选项

**方案 B-1（推荐）：引入扩展方法/兼容层**
- 新增 `NetworkPlugin/Utils/NetworkPlayerManaCompat.cs`（或类似）
- 通过 `as` / `dynamic` / 反射尝试读取实际实现类上的 mana 字段/属性；不存在则返回默认 `ManaGroup.Empty` 或 int[] 全 0。
- 替换所有直接 `.mana` 访问为 `GetManaOrDefault()`。
优点：不修改接口，兼容历史实现；风险较小。

**方案 B-2：扩展 INetworkPlayer 接口**
- 直接给 `INetworkPlayer` 增加 `ManaGroup`/`int[]` 属性。
优点：类型安全；缺点：需要修改所有实现类，破坏面较大。

本次建议采用 **B-1**。

---

## 验证计划
- `dotnet build networkplugin/NetWorkPlugin.csproj -c Release` 通过（允许已有 warning）。
- 手动联机验证：两客户端同一战斗中结束回合，非发送方能收到 `OnTurnEnd` 并更新缓存（日志中可观察）。
