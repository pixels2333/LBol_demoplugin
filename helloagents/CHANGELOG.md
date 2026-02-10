# Changelog

本文文件记录项目所有重要变更（Keep a Changelog 风格）。

## [Unreleased]

### Docs
- **[helloagents]**: 更新方案包 `plan/202602071900_trade-partner-picker-centered-ui`：聚焦 TradePanel 内 partner picker，使用游戏 UI 资源并以居中窗口弹层展示玩家列表。

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
- **[networkplugin]**: 修复状态效果同步日志刷错：部分状态效果 `HasLevel=false`，读取 `StatusEffect.Level` 会抛 `has no level`；改为仅在 `HasLevel` 时读取并将 Level 作为可空字段输出。
- **[networkplugin]**: 避免网络事件缓冲区因同一 tick 重复 key 导致的异常（SortedList duplicate key）。
- **[networkplugin]**: 修复 CardStateChanged 负载序列化失败（ManaGroup 自引用）——卡牌快照改为发送 `CostText`。
- **[networkplugin]**: 限流/去重高频同步事件：`UpdatePlayerLocation` 与 `OnMoodEffectStateSync`，减少刷屏与重复发送。
- **[networkplugin]**: 为 `FullStateSyncRequest` 增加 2 秒节流并附带 `RequestId`，降低重连路径重复请求。

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
