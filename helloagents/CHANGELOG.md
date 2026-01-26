# Changelog

本文文件记录项目所有重要变更（Keep a Changelog 风格）。

## [Unreleased]

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
