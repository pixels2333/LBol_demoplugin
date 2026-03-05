# Changelog

本文文件记录项目所有重要变更（Keep a Changelog 风格）。

## [Unreleased]

### UI
- **[networkplugin]**: 重构 `TradeDetailDialog` 交易详情界面，采用分栏布局并优化列表显示。
- **[networkplugin]**: 引入紧凑型 `ListItem` 样式（44px 高度），替代原本笨重的按钮列表占位符。
- **[networkplugin]**: 优化卡牌与遗物选择器，移除冗余背景并改用网格布局 (Grid Layout)。
- **[networkplugin]**: 修正按钮颜色逻辑，移除确认/添加按钮的错误红色样式。
- **[networkplugin]**: 调整报价编辑器（Offer Editor）按钮缩放比例，提升布局平衡感。
- **[networkplugin]**: 升级 UI 版本至 `v6`，确保修改实时生效。

### Docs
- **[helloagents]**: 更新方案包 `plan/202602071900_trade-partner-picker-centered-ui`：聚焦 TradePanel 内 partner picker，使用游戏 UI 资源并以居中窗口弹层展示玩家列表。

### 变更
- **[networkplugin]**: 执行方案包 `plan/202603051930_networkplugin-review-optimization-closure` 的 5.5/5.6/6.1~6.4/7.5 收尾批次。
	- 消息治理：`DebutBonusSyncPatch`、`RemoteCardUsePatch`、`GameResultSyncPatch` 关键消息切换到 `NetworkMessageTypes` 常量；新增 `OnDebutBonusRolled` / `OnGameRunResult` 常量。
	- 结构清理：移除未接入的 `MessageCategories`，避免“定义但不使用”结构。
	- NAT 收口：`NatTraversal` 明确 UPnP 语义状态（`DisabledByConfig` / `UnsupportedOrUnavailable` / `AvailableButNotImplemented`），并统一 `[NATTraversal][UPnP]/[STUN]` 日志口径。
	- 配置对齐：`ConfigManager.Sync`/`SyncConfiguration` 新增 `EnableNatDetection` 与 `EnableUpnpExperimental`；`NetworkStatusIndicator` 增加 NAT/UPnP 状态展示。
	- 遗留清理：删除 `ConfigManager.FeatureToggles.cs` 与 `ConfigManager.Performance.cs` 的 `#if false` 历史参考块。
	- 验收范围调整：聊天功能移出当前方案验收范围；`Campfire` 与敌人链路手工双端回归因无测试环境顺延。
	- 验证：`dotnet build networkplugin/NetWorkPlugin.csproj` 通过（289 warnings，0 errors）。
- **[networkplugin]**: 执行方案包 `plan/202603051930_networkplugin-review-optimization-closure` 的 4.4~5.4（玩家模型映射补齐 + 三端事件判定同源收敛）。
	- `INetworkPlayer` 新增 `stage` 字段；`LocalNetworkPlayer/RemoteNetworkPlayer` 实现并贯通 `NetworkManager.UpdateSinglePlayer` 的 Stage 映射。
	- `OtherPlayersOverlayPatch` 修复 `TryGetJsonElement` 的 string payload 解析断点，并将 `Welcome/PlayerListUpdate/PlayerJoined/PlayerLeft/HostChanged` 切换为 `NetworkMessageTypes` 常量引用。
	- `NetworkMessageTypes` 新增统一判定入口 `IsGameEvent(messageType, route)`（Client/HostServer/Relay），并补充 Chat/Battle/Room/Trade 分组。
	- `NetworkClient/NetworkServer/RelayServer` 的 `IsGameEvent` 改为委托同源规则，消除三端规则漂移。
	- 验证：Problems 视图无错误；关键符号引用链完整（`IsGameEvent`/`stage`/`TryGetJsonElement`）。
- **[networkplugin]**: 执行方案包 `plan/202603051930_networkplugin-review-optimization-closure` 的 3.4~3.6（敌人状态/意图/回放收敛，保留 1 个迭代兼容桥）。
	- `NetworkMessageTypes` 新增 `BattleEnemyIntentChanged`、`BattleEnemyStateChanged` 常量，并将 `EnemyStateUpdate` 显式定义为兼容桥事件。
	- `EnemySyncPatch` 改为 Host 权威发送：主发 `BattleEnemyStateChanged`，兼容镜像 `EnemyStateUpdate`。
	- 新增 `EnemyStateReceivePatch`：客机接收 `BattleEnemyStateChanged/EnemyStateUpdate`，按 `SpawnId` 主键定位并保留 `RootIndex+Id` 兜底匹配，落地 HP/Block/Shield。
	- `EnemyIntentSyncPatch` 与 `EnemyIntentReceivePatch` 收敛为常量事件名；接收侧升级为 SpawnId 主键 + 兼容键双写双查。
	- `NetworkClient/NetworkServer/RelayServer` 的 `IsGameEvent` 显式纳入敌人状态/意图新旧事件名。
	- `MidGameJoinManager.ShouldReplayEventType` 改为显式白名单（含 `BattleEnemySpawned/BattleEnemyIntentChanged/BattleEnemyStateChanged` 与旧名兼容）。
	- 构建验证：`dotnet build networkplugin/NetWorkPlugin.csproj` 通过（289 warnings，无新增错误）。
- **[networkplugin]**: 执行方案包 `plan/202603051930_networkplugin-review-optimization-closure` 的 3.1~3.3（敌人出生单入口收敛，保留 1 个迭代兼容桥）。
	- `NetworkMessageTypes` 新增 `BattleEnemySpawned` 主路径常量；`EnemySpawned` 标注为兼容桥事件。
	- `SpawnedEnemyManager` 发送侧改为主发 `BattleEnemySpawned`，并兼容镜像发送 `EnemySpawned`（仅过渡期保留）。
	- `EnemySpawnSyncPatch` 接收侧统一为 `BattleEnemySpawned/EnemySpawned` 双入口共用同一落地处理。
	- `NetworkClient/NetworkServer/RelayServer` 的 `IsGameEvent` 显式纳入 `BattleEnemySpawned`，降低对前缀匹配的隐式依赖。
	- 构建验证：`dotnet build networkplugin/NetWorkPlugin.csproj` 通过（289 warnings，无新增错误）。
- **[networkplugin]**: 执行方案包 `plan/202603051930_networkplugin-review-optimization-closure` 的 2.5（Campfire 中途加入追赶最小补充）。
	- `RoomStateSnapshot` 新增 `CampfireEvents`（最近 8 条）事件摘要字段，支持 Joiner 最小追赶载荷。
	- `CampfireSyncPatch` 广播 payload 新增 `RoomKey`；新增房间级事件缓存与 `MergeCatchupEvents`（仅缓存+ActionId 去重标记，不强行执行游戏动作）。
	- `RoomSyncManager` 在 Request/Upload/Response 链路透传 `CampfireEvents`，客户端收到 `RoomStateResponse` 后执行追赶合并。
	- `RoomStateSyncPatch.BuildSnapshot` 附带当前房间 Campfire 事件摘要。
	- 构建验证：`dotnet build networkplugin/NetWorkPlugin.csproj` 通过（warning 从 290 降至 289，无新增错误）。
- **[networkplugin]**: 执行方案包 `plan/202603051930_networkplugin-review-optimization-closure` 的幽灵方法治理批次。
	- 清理插件入口无用字段 `Plugin.netWorkPlayer`，并删除无引用重复 DTO `Network/NetworkPlayer/dto/NetWorkPlayer.cs`。
	- 移除无调用接口/实现占位：`GetMyself`、`UpdateStance`、`NetworkManager.UpdatePlayerInfo(object)`。
	- 删除 `ShopTradeIconPatch` 中 3 个无调用私有方法，并标注 `RemoteCardUsePatch.Card_GetActions_Original` 为 ReversePatch 白名单保留项。
	- 构建验证：`dotnet build networkplugin/NetWorkPlugin.csproj` 通过（存在既有 warning，无新增错误）。
- **[networkplugin]**: 执行方案包 `plan/202603051930_networkplugin-review-optimization-closure` 的聊天协议统一批次。
	- 统一聊天模型到 `Chat/ChatMessage.cs`，主字段收敛为 `PlayerName`，并兼容读取旧 `username` 负载。
	- 删除 `UI/Components/ChatUI.cs` 内重复 `ChatMessage/ChatMessageType` 定义，改为复用 `NetworkPlugin.Chat` 命名空间模型。
	- 修复聊天消息淡出计时：由哈希构造时间改为显式记录消息创建时间，避免淡出跳变。
	- `ChatConsole` 发送改为 `NetworkMessageTypes.ChatMessage` 常量；`SynchronizationManager` 接收侧名字解析统一为 `PlayerName` 优先并兼容旧字段。
	- 构建验证：`dotnet build networkplugin/NetWorkPlugin.csproj` 通过（存在既有 warning，无新增错误）。

### Fixed
- **[networkplugin]**: 修复服务器端 GameEvent 分类遗漏导致的回合结束卡死：将 `EndTurnRequest/EndTurnStatus/EndTurnConfirm` 与 `CardStateChanged` 归类为 GameEvent，避免被当作未知系统消息丢弃（影响 Host/Relay）。
- **[networkplugin]**: 修复客户端点击结束回合后掉线与按钮卡死：补齐 `EndTurn*`/`CardStateChanged` 的 GameEvent 分类；在 `PollEvents()` 中周期发送 `Heartbeat` 保活；断线时强制恢复 EndTurn 按钮可点击。
- **[networkplugin]**: 修复部分场景下结束回合后意外断线回主菜单：`GameMaster.QuitGame` 在联机中可能被内部流程触发；改为拦截并忽略该调用（仅记录告警+调用栈），避免误触发“断开联机并返回主菜单”。
- **[networkplugin]**: 修复结束回合“偶发不推进”的软锁：确认到达时战斗可能尚未进入 `IsWaitingPlayerInput`（动画/结算中），原逻辑只尝试一次导致永远不放行；改为主线程上短暂延迟重试并设置超时兜底（避免永久锁手）。
- **[networkplugin]**: 改善商店交易入口布局：将“交易”按钮插入到商店底部按钮条中，位于“卡牌服务”与“关闭商店”之间，并对相邻按钮做缩放以避免遮挡。
- **[networkplugin]**: 交易面板改为运行时克隆游戏 UI 模板创建（背景/按钮/TMP），并移除 `AddComponent<TradePanel>` 的裸创建回退，避免序列化字段缺失导致空引用。
- **[networkplugin]**: TradePanel partner picker 弹层改为克隆 `UI/Dialogs/MessageDialog` 作为遮罩/窗口框架（替换运行时纯色遮罩与 Outline 边框），列表仍使用 CommonButtonWidget 行样式以保持风格统一。
- **[networkplugin]**: TradePanel partner picker 禁用 fallback/兜底策略：列表滚动区与行模板直接复用 `UI/Panels/HistoryPanel` 的 `ScrollRect + RecordRow`，空列表提示复用 `MessageDialog` 的 subText，确保可见控件均为游戏 UI 资源。
- **[networkplugin]**: 修复 TradePanel partner picker 候选“点不了”：点击检测改为优先使用 Unity InputSystem 的 `Mouse`（兼容禁用 legacy `UnityEngine.Input` 的环境），并在命中判定中遍历候选项全部 `Graphic`，避免根 Rect 为 0 导致无法选中。
- **[networkplugin]**: 改善 TradePanel partner picker 在“严格同节点”筛选下的空列表体验：打开时若尚未同步到自身位置，则显示“正在同步位置信息...”并在 1.0 秒内自动刷新一次；同时增加“刷新”按钮用于手动重试（并在位置仍缺失时重新触发一次性等待刷新）。
- **[networkplugin]**: 修复状态效果同步日志刷错：部分状态效果 `HasLevel=false`，读取 `StatusEffect.Level` 会抛 `has no level`；改为仅在 `HasLevel` 时读取并将 Level 作为可空字段输出。
- **[networkplugin]**: 避免网络事件缓冲区因同一 tick 重复 key 导致的异常（SortedList duplicate key）。
- **[networkplugin]**: 修复 CardStateChanged 负载序列化失败（ManaGroup 自引用）——卡牌快照改为发送 `CostText`。
- **[networkplugin]**: 限流/去重高频同步事件：`UpdatePlayerLocation` 与 `OnMoodEffectStateSync`，减少刷屏与重复发送。
- **[networkplugin]**: 为 `FullStateSyncRequest` 增加 2 秒节流并附带 `RequestId`，降低重连路径重复请求。
- **[networkplugin]**: 修复 TradePanel 运行时 UI “矩形块”观感：`TradePanelRuntimeFactory` 的面板背景与交易槽位背景改为使用游戏内 `Adventure` 切片背景（`ResourcesHelper.LoadUiBackground("Adventure")` + `Image.Type.Sliced`），并用白色 tint 让纹理可见。
- **[networkplugin]**: TradePanel 交易槽位 UI 改为直接克隆游戏内 `CommonButtonWidget` 按钮模板并挂载 `TradeSlotWidget`，确保槽位外观使用原生按钮素材而非运行时纯色矩形。
- **[networkplugin]**: TradePanel 移除全屏半透明背景色块：改为透明 raycast blocker，仅使用 `UI/Dialogs/MessageDialog` 的游戏框架提供视觉；同时 `TradeSlotWidget` 选中态不再把按钮底图染成纯色块（优先仅调整 alpha），避免再次出现“纯色矩形”。
- **[networkplugin]**: 增强“是否加载了新 DLL”的可观测性：启动时输出插件程序集路径、最后写入时间、大小与 FNV64 指纹，便于排查“没有任何变化”是否为部署路径/版本不一致。
- **[networkplugin]**: TradePanel 状态文案 `Trade.WaitingForItems` 缺失本地化时回退为中文，避免 Unity Log 刷屏并更直观地看到状态变化。
- **[tooling]**: `copy_networkplugin_dll.ps1` 支持自动探测目标 Mods 目录（从 BepInEx/ModLBoL 日志推断），并在复制前后输出源/目标的元信息与 SHA256（可选复制支持库）。
- **[networkplugin]**: TradePanel 报价编辑弹层（`EnsureOfferEditorOverlay()`）改为优先从 `UI/Dialogs/MessageDialog` 预制体提取 TMP/按钮模板并克隆，按钮会禁用多余 Button/Tooltip 行为；同时为容器增加 `Adventure` 切片背景，确保报价编辑区域整体观感为游戏原生 UI。

### 变更
- **[networkplugin]**: 网络日志中文化与参数化：发送/接收日志附带 payload 指纹（FNV-1a 64）与关键字段摘要，便于区分“同一事件重复发送”与“不同事件”。
- **[networkplugin]**: 脱敏敏感字段：`joinToken` 不再出现在日志中（仅记录“已脱敏”）。

### 变更
- 局内地图进度同步（inrun-map-progress-sync）：移除 HostSaveTransfer 存档分片传输链路，改为 FullStateSnapshot 聚焦 seeds + MapState（主机权威）。
	- `networkplugin/Network/Messages/NetworkMessageTypes.cs`: 删除 `OnHostSaveTransferStart/Chunk/End`。
	- `networkplugin/Network/MidGameJoin/MidGameJoinManager.cs`: 删除 HostSaveTransfer 相关 handler/发送逻辑，FullStateSyncResponse 不再包含 host save bytes。
	- `networkplugin/Network/Snapshot/MapStateSnapshot.cs`: 扩充地图进度与提交点字段 `NodeStates`、`ClearedNodes`、`LastCheckpointId`、`LastCheckpointAtUtcTicks`（保留旧字段兼容）。
	- `networkplugin/Network/Reconnection/ReconnectionManager.cs`: 新增 `MarkMapCheckpoint(reason, nodeKey)` 并在 CreateFullSnapshot 中填充 LastCheckpoint*。

### 新增
- 局内地图进度同步（inrun-map-progress-sync）：新增主机侧“关键提交点”Hook（用于中途加入/重连对齐）。
	- `networkplugin/Patch/Network/RoomEntrySyncPatch.cs`: enter_node checkpoint（忽略 forced EnterNode）。
	- `networkplugin/Patch/Actions/TurnAction_Patch.cs`: battle_end checkpoint。
	- `networkplugin/Patch/Network/MapCheckpointSyncPatch.cs`: next_stage / reward_closed / station_finish / shop_after_buying / gap_option_selected checkpoints。

- 局内地图进度同步（inrun-map-progress-sync）：新增客户端侧最小追赶骨架（MapState 消费/应用）。
	- `networkplugin/Network/MidGameJoin/MapCatchUpOrchestrator.cs`: 暂存 FullSnapshot 并在本地 GameRun/地图 UI 就绪后尽力对齐节点状态与路径。
	- `networkplugin/Network/MidGameJoin/MidGameJoinManager.cs`: 收到 FullSnapshot 后写入追赶执行器。
	- `networkplugin/Patch/Map/MapPanelUpdateMapNodesStatusPatch.cs`: 在 MapPanel 刷新时 opportunistic apply pending MapState。
	- `networkplugin/Network/MidGameJoin/MapCatchUpOrchestrator.cs`: 追赶执行改为“可分帧会话 + 预算推进”，避免一次性应用导致 UI 卡顿；应用完成后仍保持 room-state 主动请求逻辑。
	- `networkplugin/Patch/Map/MapPanelUpdateMapNodesStatusPatch.cs`: 增加 `MapPanel.Update` 每帧小预算驱动，并在 `UpdateMapNodesStatus` 中使用更合理的预算，提升追赶收敛速度。

- 局内地图进度同步（inrun-map-progress-sync）：Joiner 开局锁定与 stage 对齐。
	- `networkplugin/Patch/MidGameJoin/JoinerStartGameLockPatch.cs`: Patch `GameMaster.StartGame`，joiner 选角但强制锁定主机 `seed/difficulty/puzzles/mode/stages/debutAdventureType`。
	- `networkplugin/Patch/MidGameJoin/JoinerStageIndexAlignPatch.cs`: Patch `GameRunController.EnterNextStage`，首次进入 stage 前设置 `_stageIndex = hostStageIndex - 1`，加速 `MapSeedUlong` 对齐。

- 局内地图进度同步（inrun-map-progress-sync）：断线重连（回主菜单）最小闭环。
	- `networkplugin/Patch/UI/MainMenuMultiplayerEntryPatch.cs`: Join 时检测本地可继续存档，支持“连接成功后本地 Restore → 向房主追赶 FullSnapshot”。
	- `networkplugin/Network/MidGameJoin/MidGameJoinManager.cs`: 新增 `BeginReconnectAndCatchUp(...)` 与 `TryGetApprovedJoinTokenByRequestId(...)`，避免 UI 线程同步等待。
	- `networkplugin/Plugin.cs`: 增加主线程调度队列（`RunOnMainThread`）与定期 catch-up pump，确保后台线程回调/弹窗安全并提升“不开地图也能追赶”的可靠性。
	- `networkplugin/Network/MidGameJoin/MapCatchUpOrchestrator.cs`: 增加 `PumpMainThread()`；SetPendingSnapshot 变为纯数据写入；MapState 应用成功后主动请求当前 `RoomStateSnapshot`（避免 catch-up 绕开 EnterNode 导致房间状态缺失）。
	- `networkplugin/Patch/Map/MapPanelUpdateMapNodesStatusPatch.cs`: 调整为 Prefix 先 apply pending map state，确保本次地图刷新直接展示对齐结果。
	- `networkplugin/Patch/Network/RoomStateSyncPatch.cs`: 忽略 `forced` 的 `GameMap.EnterNode`（用于路径重建/追赶），避免重建路径时刷 `RoomStateRequest`。
	- `networkplugin/Network/MidGameJoin/MapCatchUpOrchestrator.cs`: 追赶时按 `PathHistory` 逐节点调用 `GameMap.EnterNode(forced=true)` 重建 `_path`，使地图内部 bookkeeping 更一致（4.4 方向）。

- 局内地图进度同步（inrun-map-progress-sync）：战斗意图追赶补齐（Host 权威 + 客户端落地）。
	- `networkplugin/Patch/Network/EnemyIntentSyncPatch.cs`: 限定为 Host 才广播 `BattleEnemyIntentChanged`；并在 `PlayerJoined/Welcome/PlayerListUpdate` 时限频重发“当前战斗所有敌人的意图”，加速 join/reconnect 的 UI 追赶。
	- `networkplugin/Patch/Network/EnemyIntentReceivePatch.cs`: 客户端接收 `BattleEnemyIntentChanged`，按 SpawnId/RootIndex+Id 定位 EnemyUnit，重建 `EnemyUnit.Intentions` 并触发 `NotifyIntentionsChanged()` 以刷新意图 UI。

- 局内地图进度同步（inrun-map-progress-sync）：补齐最小可运行验收材料。
	- `helloagents/plan/202601251728_inrun-map-progress-sync/verify.md`: 手工验收清单（中途加入/战斗意图/回主菜单重连）。
	- `debugtools/VerifyInrunMapProgressSync.cs`: 轻量级仓库不变量校验（无 SaveLoadSyncPatch、无 HostSaveTransfer 常量）。

### 新增
- **[networkplugin]**: 完善交易详情对话框 `TradeDetailDialog`：
    - **逻辑与同步**: 实现完整的报价同步、地理位置过滤（仅显示同节点玩家）、握手确认以及交易完成后卡牌/金币/遗物的严格扣除与增加。
    - **UI 风格沉浸化**: 彻底重构 UI 构建流程，移除“矩形块”原生样式，全面使用游戏内的 `Adventure` 背景素材、`CommonButtonWidget` 按钮素材以及 `RecordCardCell`/`ExhibitWidget` 等原生组件，确保视觉风格与游戏本体高度一致。
    - **交互增强**: 为交易面板添加游戏原生音效（确认/取消/点击），并在列表区域增加半透明装饰底色，提升操作反馈感。
- 交易同步：新增 `TradeSyncPatch`（Host 权威裁决 + 广播）与 `TradePanel` 联机接入，支持两端报价/确认/取消与完成后各自卡组落地（模型A）；并提供 `OnTradeSnapshotRequest` 用于重连/中途加入的会话状态恢复。
- 交易同步 v2：交易范围扩展为卡牌/道具/金币/Exhibit；增加 Preparing + PrepareResult 握手（本地严格校验并允许失败后回到 Open 重试）；TradePanel 增加交易对象选择 overlay、报价编辑（金币/展品），并在 Completed 阶段严格落地（缺失即失败）。
- 交易入口一致性：GapOptions 与 ShopTradeIcon 统一“交易不可用/未连接/配置禁用/缺少 TradePanel 实例”提示。
- Gap 复活同步：新增 `ResurrectPanel` + `ResurrectSyncPatch` + `DeathRegistry`，实现发起者请求 -> Host 广播 -> 目标本人落地复活；成功后发起者扣费、失败提示（等价退款）。
- 网络身份：补齐 `INetworkPlayer.playerId`（服务端分配并下发的唯一标识），并在 `LocalNetworkPlayer` 中通过 `NetworkIdentityTracker` 提供 selfId。
- 事件/对话同步：为 `EventSyncPatch` 增加最小追赶能力（缓存并周期性广播对话 options、对新加入/欢迎事件触发快照重发），并增强 `OnEventSelection` 的确定性落地（OptionId/Index 映射、幂等去重、落地后清理 pending）。
- 事件/对话同步：权威模型 B 支持（允许非 Host 上报事件开始，但最终选择仍由 Host 仲裁并广播）。
- 回合同步：新增 `OnTurnEnd` 回合结束快照结构与接收落地（缓存 + 更新远端玩家基础字段）。
- 远端目标出牌链路增强：未连接阻断/发送失败回退。Resolved 乱序丢弃（ResolveSeq/Timestamp/RequestId）。动画一致性改进。
- 文档：补充并归档“远端队友目标出牌闭环（目标端结算 + 快照广播）”方案包。
- 断线重连：补齐 `ReconnectionManager` TODO，并补齐 `FullStateSyncRequest/FullStateSyncResponse` 走 GameEvent 通道（Host/Relay/Client，待手动联机验证）。
- NetworkManager：清理过期 TODO 注释，并补齐 `UpdatePlayerInfo(object)` 作为兼容入口（复用既有 JSON 更新逻辑）。
- MidGameJoin：落地 `MidGameJoinRequest/Response` + 通过 Relay `DirectMessage` 跑通 FullSync 请求/响应，并加入最小事件追赶回放（失败降级）。
- FullStateSync：实现 RelayServer 的 FullStateSync 定向路由（请求转发给房主、响应单播给请求方），并在 NetworkServer 增加 DirectMessage 中继以支持 Host/直连模式下的同一链路。
- 房间/战斗残局同步：新增 RoomStateRequest/Response/Upload/Broadcast 消息类型与优先级，并提供主机缓存 + 客户端进入节点请求/战斗中上传的最小闭环；直连与 Relay 均支持定向路由。

### 修复
- 修复战斗同步补丁：`BattleController_Patch` 对齐 `BattleController.Damage/Heal/TryAddStatusEffect/RemoveStatusEffect` 的真实签名，并统一改为走 `SendGameEventData`；按“主机广播/客户端上报”拆分 Battle* 事件；状态效果同步升级为“增量为主 + 周期全量校验”，并移除对私有字段的 Traverse 依赖。
- 修复 `TradeSlotWidget` 多次 SetCard 导致按钮回调累积的问题。
- 修复编译错误：引入 `INetworkPlayer` 的 mana 兼容层并替换直接访问，`networkplugin` 可编译通过。
- 修复 `MidGameJoinManager` 编译错误：将 `GetRoomStatus(...)` 引用改为 `GetRoomInfo(...)`。
- 修复 `NetWorkPlayer` 构造函数潜在空引用：`VisitingNode` 为空时初始化 `location_X/location_Y` 回退为 0。
- 修复 NAT 信息序列化与 token 校验：为 `IPEndPoint` 增加 JSON 转换器（`ip:port`），修复连接 token 的 TTL 校验，并用最小 STUN Binding 探测替换占位实现；UPnP 默认按不可用回退。
- 修复主菜单缺少“多人游戏”入口：增强 `MainMenuMultiplayerEntryPatch` 的模板按钮定位（分层降级），并改为打开自定义遮罩面板 UI（做房主/加入房主/返回），入口默认可见。
- 修复“多人游戏”入口弹窗边界线与打开异常：将上下金色分隔线改为围绕弹窗容器绘制（向中心各内缩 50px），并确保 overlay 置顶显示；同时为创建流程补充异常日志，避免点击无反馈。
- 修复心情特效同步刷屏/失效：移除定时状态广播，改为“心情变化触发状态同步”（开始/结束时发送），并修复接收端对 string JSON payload 的解析与远端视图重建后的状态恢复。
- 修复 Host 默认端口：当 ServerPort 未配置或为非正数时，Host/Join 弹窗与本机服务器启动统一回退到 7777。
- 增强中途加入存档继承可观测性：为房主存档分片传输、客户端接收/解码、落盘与恢复补充关键日志（Plugin.Logger）。

### 文档
- 补齐 `INetworkPlayer` 的 XML 文档注释：解释 `stance/ultimatePower/mana/UpdateLocation/GetMyself` 的网络语义与 TODO 背景。

### [0.10.3] - 2026-02-13
- **[TradePanel]**: 优化报价编辑器（Offer Editor）布局。
  - 减小按钮缩放比例 (0.75x) 以适应行高。
  - 调整行间距与对齐方式，使文字和按钮分布更合理。
  - 强制 UI 版本更新至 `v4` 以刷新运行时实例。
  - 优化了卡牌、金币、展品三行的水平布局。
