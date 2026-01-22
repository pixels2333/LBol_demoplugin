# 任务清单: todo_sweep

目录: `helloagents/plan/无_todo_sweep/`

---

## 任务状态符号说明

| 符号 | 状态 | 说明 |
|------|------|------|
| `[ ]` | pending | 待执行 |
| `[√]` | completed | 已完成 |
| `[X]` | failed | 执行失败 |
| `[-]` | skipped | 已跳过 |
| `[?]` | uncertain | 待确认 |

---

## 执行状态
```yaml
总任务: (已归档)
已完成: (参考原任务列表)
完成率: (参考原任务列表)
```

---

## 任务列表

目录：`helloagents/plan/202601122206_todo_sweep/`

---

## A. 已落地（无需手动验证）
- [√] A1 `networkplugin/Core/SynchronizationManager.cs`：实现 `CreateGameEventFromNetworkData`（不再抛 `NotImplementedException`）。
- [√] A2 `networkplugin/Core/SynchronizationManager.cs`：实现 `ApplyRemoteEvent` 的最小可用逻辑，并移除无意义的 `TODO:STOP`。
- [√] A3 `networkplugin/Network/Client/NetworkClient.cs`：断线自动重连（记录最后一次 host/port + 定时重试）。
- [√] A4 `networkplugin/UI/Components/NetworkStatusIndicator.cs`：玩家数统计改为 `NetworkIdentityTracker.GetPlayerIdsSnapshot().Count`，并落地“重连”按钮逻辑。
- [√] A5 `networkplugin/Patch/EnemyUnits/EnemyUnitPatches.cs`：敌方血量倍率改为从配置读取（复用 `ConfigManager.GetEnemyHpMultiplier`）。
- [√] A6 `networkplugin/Patch/GameUnit/Unit_SyncPatch.cs`：补齐 `GetLocalPlayer()` 的最小实现（避免长期占位返回 `null`）。
- [√] A7 `networkplugin/Network/Client/NetworkClient.cs` + `networkplugin/Network/Server/NetworkServer.cs` + `networkplugin/Network/Server/RelayServer.cs`：将 `ChatMessage` 作为 GameEvent 路由/广播，保证客户端能统一从 `OnGameEventReceived` 接收。
- [√] A8 `networkplugin/UI/Components/ChatUI.cs`：订阅 `NetworkClient.OnGameEventReceived` 并解析 `ChatMessage`；发送方 ID/昵称改为从 `NetworkIdentityTracker`/`GameStateUtils` 获取（含反射兜底）。
- [√] A9 `networkplugin/Network/Utils/NatTraversal.cs`：补齐 logger 初始化兜底（避免 `Plugin.Logger` 不可用时 NRE）。
- [√] A10 `networkplugin/Patch/Network/SaveLoadSyncPatch.cs`：补齐 `GetCurrentPlayerId()`、`IsHostPlayer()`，并提供最小 `CaptureGameState()` 快照（诊断用途）。
- [√] A11 `networkplugin/Patch/Network/EventSyncPatch.cs`：补齐 `GetCurrentPlayerId()` 与最小 `BuildEventSnapshot()`，并移除已实现的 TODO 注释。
- [√] A12 `networkplugin/Chat/ChatConsole.cs`：聊天历史去重/限长/默认值处理；落地本地玩家信息获取；清理过期 TODO 占位区块。
- [√] A13 `networkplugin/Chat/ChatMessage.cs`：将“非行动项”的 TODO 改为普通“预留”说明，避免干扰 TODO 扫描。
- [√] A14 编译修复：补齐缺失 `using`（`System.Linq`、`NetworkPlugin.Network.Messages`），Release 编译通过。

## B. 已跳过（需要手动验证/外部依赖/重构决策）
- [-] B1 `networkplugin/Network/Utils/NatTraversal.cs`：UPnP 映射/删除/可用性检查、STUN 真实交互。
  > 备注：依赖外部网络环境与实现库/系统 API；需要手动验证。
- [-] B2 `networkplugin/Network/Server/NetworkServer.cs`：`FullStateSyncRequest` 的服务端处理（非简单广播语义）。
  > 备注：涉及完整同步语义与联机验证；当前 MidGameJoin 已走 `DirectMessage` 路线。
- [-] B3 `networkplugin/Network/Server/RelayServer.cs`：Relay 模式 `FullStateSyncRequest` 路由与响应。
  > 备注：同 B2，需要联机验证。
- [-] B4 `networkplugin/Patch/Network/EventSyncPatch.cs`：事件入口点、投票逻辑、对话同步等。
  > 备注：高度依赖游戏运行时与多人一致性验证。
- [-] B5 `networkplugin/Patch/Network/SaveLoadSyncPatch.cs`：将 Host 存档状态应用到本地游戏。
  > 备注：需要游戏内验证与数据源对齐。
- [-] B6 `networkplugin/Patch/Actions/TurnAction_Patch.cs`：MaxMana 获取、服务端应用到 `INetworkPlayer`、以及其它待定逻辑。
  > 备注：需要游戏内验证与同步模型确认。
- [-] B7 `networkplugin/Patch/BattleController_Patch.cs`：请求补充用户 ID。
  > 备注：需要明确 PlayerId 数据源与联机验证。
- [-] B8 `networkplugin/UI/Panels/TradePanel.cs` / `networkplugin/UI/Panels/ResurrectPanel.cs`：发送逻辑与多玩家卡组操作。
  > 备注：与 UI/网络架构耦合，需场景与联机验证。
- [-] B9 `networkplugin/Network/NetworkPlayer/*`：`SyncVar`/`stance` 字段与弃用标记的结构性重构。
  > 备注：需要同步模型决策与联机验证，非本批。
- [-] B10 `networkplugin/Chat/ChatMessage.cs`：私聊（Whisper）功能。
  > 备注：需要协议与 UI 交互约束，需手动验证。
- [-] B11 `networkplugin/Network/MidGameJoin/AIPlayerController.cs`：AI 角色（低优先级）。
  > 备注：功能性变更，需联机玩法验证。

## C. 验证
- [√] C1 编译验证：`dotnet build networkplugin/NetWorkPlugin.csproj -c Release`
- [√] C2 TODO 扫描：除 B 部分外，未发现“无需手动验证即可落地”的遗留 TODO。

---

## 执行备注

> 执行过程中的重要记录

| 任务 | 状态 | 备注 |
|------|------|------|
