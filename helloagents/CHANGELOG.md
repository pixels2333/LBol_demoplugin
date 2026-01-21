# Changelog

本文文件记录项目所有重要变更（Keep a Changelog 风格）。

## [Unreleased]

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

### 修复
- 修复 `TradeSlotWidget` 多次 SetCard 导致按钮回调累积的问题。
- 修复编译错误：引入 `INetworkPlayer` 的 mana 兼容层并替换直接访问，`networkplugin` 可编译通过。
- 修复 `MidGameJoinManager` 编译错误：将 `GetRoomStatus(...)` 引用改为 `GetRoomInfo(...)`。
- 修复 `NetWorkPlayer` 构造函数潜在空引用：`VisitingNode` 为空时初始化 `location_X/location_Y` 回退为 0。

### 文档
- 补齐 `INetworkPlayer` 的 XML 文档注释：解释 `stance/ultimatePower/mana/UpdateLocation/GetMyself` 的网络语义与 TODO 背景。
