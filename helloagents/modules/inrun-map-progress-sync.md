# inrun-map-progress-sync

## 目标

将“中途加入/断线重连”的核心从“主机存档分片传输 + RestoreGameRun”切换为：
- 主机权威 FullSnapshot（Seeds + MapState + Checkpoint）
- 客户端在地图界面本地追赶（对齐地图节点状态/路径/当前位置；战斗节点复用 RoomSync + EnemyIntentSync）

## #codebase 索引

### FullSnapshot（主机权威）

- `networkplugin/Network/Reconnection/ReconnectionManager.cs`
  - `CreateFullSnapshot()`：构建 `FullStateSnapshot`，填充 `GameStateSnapshot`（RootSeed/UISeed/StageIndex + host start config）与 `MapStateSnapshot`（MapSeedUlong/节点状态/路径/清节点/提交点）。
  - `MarkMapCheckpoint(reason, nodeKey)`：主机侧关键提交点（cp00000001:reason）生成与记录。

- `networkplugin/Network/Snapshot/FullStateSnapshot.cs`
  - 容器：`GameState` + `MapState` + `PlayerStates` + `BattleState`（暂保留）。

- `networkplugin/Network/Snapshot/GameStateSnapshot.cs`
  - `RootSeed/UISeed/StageIndex`：确定性生成基础。
  - `Difficulty/Puzzles/GameMode/ShowRandomResult/StageTypeNames/DebutAdventureTypeName`：Joiner 选角但锁定主机开局参数所需字段。

- `networkplugin/Network/Snapshot/MapStateSnapshot.cs`
  - `MapSeedUlong`：用于 joiner 对齐 stage map seed。
  - `NodeStates`（nodeKey -> status string）：用于 UI/节点状态对齐。
  - `ClearedNodes`：用于后续“清节点追赶/领奖”实现。
  - `PathHistory/CurrentLocation`：用于追赶路径。
  - `LastCheckpointId/LastCheckpointAtUtcTicks`：用于重连对齐诊断。

### MidGameJoin / FullStateSync

- `networkplugin/Network/MidGameJoin/MidGameJoinManager.cs`
  - `RequestJoin/ApproveJoin/ExecuteJoin`：JoinToken 发放、FullStateSync 请求/响应、MissedEvents 回放。

- `networkplugin/Network/MidGameJoin/MapCatchUpOrchestrator.cs`
  - `SetPendingFullSnapshot()`：保存待应用快照，并在无本地 GameRun 时提示打开 `StartGamePanel`。
  - `TryApplyPendingToCurrentRun()`：可分帧推进的追赶入口（内部维护追赶会话），按预算逐步应用 `MapStateSnapshot`（路径/节点状态/VisitingNode/RoomStateRequest）。
  - `TryGetPendingFullSnapshot(out snapshot)`：供 joiner 开局锁定/阶段对齐补丁读取主机开局参数。
  - `PumpMainThread()`：主线程周期性驱动（由 `Plugin.Update()` 调用），用于：
    - 即使不开地图也能不断尝试 apply pending snapshot（避免卡死在“等待刷新”）。
    - 只在主线程提示 StartGamePanel（避免后台线程 UI 调用）。
    - MapState 应用成功后主动请求当前房间的 `RoomStateSnapshot`（catch-up 过程绕开 EnterNode，不会自然触发 RoomStateRequest）。
  - 4.4：PathHistory 逐节点追赶
    - 通过 `GameMap.EnterNode(forced=true)` 逐节点重建路径（清空 `_path` 后重放）。
    - 配合 `RoomStateSyncPatch` 忽略 `forced` 参数，避免重建路径时刷 `RoomStateRequest`。

### Joiner 开局锁定 / StageIndex 对齐

- `networkplugin/Patch/MidGameJoin/JoinerStartGameLockPatch.cs`
  - Patch `LBoL.Presentation.GameMaster.StartGame(ulong? seed, ...)`：当 joiner 有 pending FullSnapshot 时，强制覆盖 `seed/difficulty/puzzles/gameMode/showRandomResult/stages/debutAdventureType`。

- `networkplugin/Patch/MidGameJoin/JoinerStageIndexAlignPatch.cs`
  - Patch `LBoL.Core.GameRunController.EnterNextStage`（Prefix）：当 joiner 初次进入第一关前，设置私有 `_stageIndex = hostStageIndex - 1`，使第一次 `EnterNextStage()` 直接进入主机所在 stage，从而尽快满足 `MapSeedUlong` 对齐。

### Map / Room / Battle 同步（追赶依赖）

- `networkplugin/Patch/Map/MapPanelUpdateMapNodesStatusPatch.cs`
  - `MapPanel.UpdateMapNodesStatus` Prefix：在 UI 读取节点状态前，先 `TryApplyPendingToCurrentRun()`，保证本次刷新直接展示对齐结果。
  - `MapPanel.Update` Postfix：地图面板每帧以小预算推进追赶，避免在单次 UI 刷新中集中做大量工作导致卡顿。
  - `MapPanel.UpdateMapNodesStatus` Postfix：继续用于“上报本地玩家位置”。

### 主线程调度与可靠触发

- `networkplugin/Plugin.cs`
  - `Plugin.Update()`：
    - 刷新主线程队列（`Plugin.RunOnMainThread(...)`），保证后台线程的回调/弹窗不会跨线程。
    - 低频调用 `MapCatchUpOrchestrator.PumpMainThread()`，让重连/中途加入在“不打开地图”的情况下也能推进。

- `networkplugin/Patch/Network/RoomStateSyncPatch.cs`
  - `GameMap.EnterNode` Postfix：进入节点后请求 `RoomStateRequest`。
  - `BattleController.StartBattle/EndTurn/EndBattle`：上传 `RoomStateSnapshot`；joiner 进入战斗后尽力应用主机敌人状态。

- `networkplugin/Patch/Network/EnemyIntentSyncPatch.cs`
  - `EnemyUnit.UpdateTurnMoves` Postfix：同步敌人意图（支持 `PauseIntentSync` 防回环）。
  - Host 权威：仅 Host 发送 `BattleEnemyIntentChanged`；并在 `PlayerJoined/Welcome/PlayerListUpdate` 时限频广播当前战斗所有敌人意图，帮助中途加入/重连快速看到正确 intent UI。

- `networkplugin/Patch/Network/EnemyIntentReceivePatch.cs`
  - 客户端接收 `BattleEnemyIntentChanged`：解析 JSON payload，按 SpawnId（优先）或 RootIndex+Id（回退）定位 EnemyUnit，写入 `EnemyUnit.Intentions` 并触发 `NotifyIntentionsChanged()`。

### 关键提交点（Host-only）

- `networkplugin/Patch/Network/RoomEntrySyncPatch.cs`：enter_node checkpoint。
- `networkplugin/Patch/Actions/TurnAction_Patch.cs`：battle_end checkpoint。
- `networkplugin/Patch/Network/MapCheckpointSyncPatch.cs`：next_stage / reward_closed / station_finish / shop_after_buying / gap_option_selected checkpoints。

## 当前状态

- 已实现：HostSaveTransfer 清理、FullSnapshot seeds+MapState、checkpoint 记录、joiner 选角提示、joiner 开局参数锁定、joiner stage index 对齐、MapState 最小应用骨架。
- 已实现（4.3）：回主菜单重连（客户端）：连接成功后先本地 `RestoreGameRun`，再通过 MidGameJoin 拉 FullSnapshot 并追赶。
  - `networkplugin/Patch/UI/MainMenuMultiplayerEntryPatch.cs`: Join 时检测本地存档，提供“重连并继续存档（本地恢复 + 向房主追赶）”。
    - 额外门槛：等待 `Welcome/PlayerListUpdate`（selfId + hostId）后再发起 mid-game join，降低“刚连接就 RequestJoin 失败”的概率。
    - Restore 后等待 `GameRun` 创建完成，避免 catch-up 误判“没有 run”而弹 StartGamePanel。
  - `networkplugin/Network/MidGameJoin/MidGameJoinManager.cs`: `BeginReconnectAndCatchUp(...)`（非阻塞，后台线程等待批准并执行 `ExecuteJoin`）。
- 已实现（4.4 基础）：追赶入口改为可分帧推进（内部会话 + 预算执行），并在 MapPanel 每帧小预算驱动以减少卡顿。
- 待实现：按 `PathHistory` 的“逐节点追赶/清节点领奖”（已部分落地，仍需覆盖更多站点类型与边界）。
- 已实现：正在战斗节点的意图 UI 追赶（RoomSync 负责敌人血量/格挡等，EnemyIntent* 负责 intent 显示）。

### SaveLoadSyncPatch 决策

- 结论：不实现/不恢复 `networkplugin/Patch/Network/SaveLoadSyncPatch.cs`，联机不再传输 `GameRunSaveData` bytes。
- 允许：回主菜单重连场景下，客户端本地 `RestoreGameRun`（读取本机存档）用于“继续游戏”。
- 联机追赶：始终以主机权威 `FullStateSnapshot`（seeds + MapState + checkpoint）为准。
- 配置：`EnableSaveLoadSync` 标记为 Deprecated，并在运行时强制关闭以避免误用。
