# Why - TurnEnd 接收落地 + 编译错误修复

## 背景
当前已实现“回合边界发送端”的基础能力：
- 在 `EndPlayerTurnAction.Execute` 的 Harmony Postfix 中发送 `OnTurnEnd`（payload 为 `TurnEndStateSnapshot`）。

但仍存在两个缺口：
1. `OnTurnEnd` 事件的接收端尚未落地（订阅/解析/写入远端玩家状态）。
2. `networkplugin` 当前 `dotnet build` 失败（多处引用 `INetworkPlayer.mana`，接口中不存在该成员），导致无法进行基础编译验证与迭代。

## 目标
- 让 `OnTurnEnd` 成为“可用的同步链路”：发送端 -> 网络广播 -> 接收端消费并更新远端玩家状态缓存（至少用于 UI/调试/一致性验证）。
- 修复 `INetworkPlayer.mana` 相关编译错误，使 `networkplugin/NetWorkPlugin.csproj` 可以通过编译（不要求清零历史 warning）。

## 非目标
- 不在本次方案中实现完整的“远端状态强制回放/权威回滚”。
- 不改变 `EndTurnSyncPatch` 的协商策略（EndTurnRequest/Confirm）与 UI 锁定行为。

## 成功标准（验收）
- `OnTurnEnd` 到达客户端时能被一个明确的接收模块处理，并把关键字段写入到远端玩家对象/缓存。
- `dotnet build networkplugin/NetWorkPlugin.csproj -c Release` 不再因为 `mana` 缺失而失败。

## 风险与约束
- `INetworkPlayer` 可能只提供最小接口；对远端状态的写入应优先走现有 `RemoteNetworkPlayer` / `NetworkManager` 可用的扩展点，避免强耦合。
- `EndTurnSyncPatch` 在允许本地真正结束回合前会重置 `_localEndedTurn`；因此接收端与发送端应以“事件本身”作为事实来源，而非依赖协商标志。
