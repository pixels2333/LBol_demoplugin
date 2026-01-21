# 模块: networkplugin

## 目的
提供联机同步与 UI 扩展补丁。

## 规范
### Trade（交易同步）
- Host 权威会话：`networkplugin/Patch/Network/TradeSyncPatch.cs` 维护 `TradeSessionState` 并广播。
- Client UI：`networkplugin/UI/Panels/TradePanel.cs` 仅联机可用，完成后仅对本地 `GameRun` 落地。

#### v2 状态机与握手
- `Open`：双方可以修改报价；每次 OfferUpdate 会重置双方确认。
- `Preparing`：双方进行本地严格校验（卡实例存在、金币足够、展品存在且可交易），并通过 PrepareResult 上报；失败时回到 `Open` 允许修改后重试。
- `Completed`：客户端严格落地（找不到/不可移除/金币不足/展品创建失败等都视为失败）。

#### v2 多资产报价
- 卡牌/道具：用 `CardRef` 列表表达（Tool 即 `CardType.Tool`）。
- 金币：`MoneyA/MoneyB`。
- 展品：`ExhibitsA/ExhibitsB`（最小字段为 `ExhibitId`）。

#### 展品可交易规则
- 默认仅允许 `LosableType == Losable` 的 Exhibit 进入报价。
- 额外黑名单：`networkplugin/UI/Panels/TradeExhibitRules.cs` 通过枚举维护不可交易展品（枚举成员名 == `Exhibit.Id`）。

#### 入口一致性
- GapStation 入口：`networkplugin/Patch/UI/GapOptionsPanel_Patch.cs`。
- 商店入口：`networkplugin/Patch/UI/ShopTradeIconPatch.cs`。
- 两者通过 `networkplugin/Patch/UI/TradeUiMessages.cs` 统一“未连接/配置禁用/缺少 TradePanel 实例”等提示。

### 远端队友作为出牌目标（单体目标）
- 通过 `RemotePlayerProxyEnemy : EnemyUnit` 兼容 `UnitSelector.SelectedEnemy` 的类型约束。
- 发送端在 `Card.GetActions` 处拦截：不本地结算，改为发送 `OnRemoteCardUse`。
- 目标端接收后按蓝图结算，再发送 `OnRemoteCardResolved` 广播结算后的状态快照。
- 接收端对 `OnRemoteCardResolved` 做 `ResolveSeq/Timestamp/RequestId` 去重与乱序丢弃，避免状态回滚。

### 玩家身份与玩家列表来源
- 客户端侧“玩家列表/数量”由 `NetworkManager` 维护（轻量缓存），不承担完整权威同步。
- 服务器侧分配的 `PlayerId` / Host 信息由 `NetworkIdentityTracker` 从 GameEvent 提取并缓存。
- `NetworkManager` 在收到 `Welcome/PlayerListUpdate/PlayerJoined/PlayerLeft` 等事件后同步缓存，并可通过 `GetAllPlayers/GetPlayerCount/GetPlayer` 查询。

### MidGameJoin（中途加入）
- 消息类型：
  - `MidGameJoinRequest` / `MidGameJoinResponse`：Joiner ⇄ Host 的请求/批准（通过 Relay `DirectMessage` 转发）。
  - `FullStateSyncRequest` / `FullStateSyncResponse`：Joiner 向 Host 拉取 `FullSnapshot + MissedEvents`（通过 Relay `DirectMessage` 转发，绕开 RelayServer 的 FullSync 路由 TODO）。
- 最小流程：
  1. Joiner 先 `JoinRoom` 成为房间成员（旁观者）。
  2. Joiner 发送 `DirectMessage(Target=Host, Type=MidGameJoinRequest, Payload={RoomId, PlayerName, ClientPlayerId, RequestId})`。
  3. Host 自动批准并回包 `MidGameJoinResponse(JoinToken + BootstrappedState)`（可诊断日志 + 超时清理）。
  4. Joiner `ExecuteJoin(JoinToken)`：先 FastSync，再发 `FullStateSyncRequest(JoinToken)` 获取 FullSync。
  5. Joiner 收到 `FullStateSyncResponse` 后尽力回放 `MissedEvents`（失败降级为“仅快照/不中断”）。
- 最小安全约束：
  - `JoinToken` 仅由 Host 签发、带超时、单次消费（在 Host 侧随 `FullStateSyncRequest` 校验并移除）。

### FullStateSync（完整快照同步）路由约定
- 目的：避免把 `JoinToken` / `FullSnapshot` / `MissedEvents` 作为房间广播扩散，且避免无谓负载放大。
- Relay 模式（RelayServer）：
  - `FullStateSyncRequest`：按 `RoomId` 作用域定向转发给房主（HostPlayerId）。
  - `FullStateSyncResponse`：仅单播给 `TargetPlayerId`。
  - 对 `DirectMessage` 内层为 `FullStateSync*` 的情况，服务端强制按房间作用域与房主规则路由，忽略客户端自填的目标。
- Host/直连模式（NetworkServer）：
  - 服务端实现 `DirectMessage` 中继（用于 `MidGameJoin*` 与 `FullStateSync*` 的既有链路）。
  - `FullStateSyncRequest`：服务端定向转发给房主客户端（由房主侧 `MidGameJoinManager` 校验 JoinToken 并生成响应）。
  - `FullStateSyncResponse`：服务端仅单播给 `TargetPlayerId`。

## 方案库记录：TurnEnd

### OnTurnEnd（回合结束快照）
- 发送端：在 `EndPlayerTurnAction.Execute` 的 Harmony Postfix 触发，发送 `NetworkMessageTypes.OnTurnEnd`，payload 为 `TurnEndStateSnapshot`。
- 接收端：订阅 `INetworkClient.OnGameEventReceived`，解析 `OnTurnEnd` 并更新远端玩家状态缓存（不直接改动 LBoL 的战斗状态）。
  - 实现：`networkplugin/Patch/Network/TurnEndSnapshotReceivePatch.cs`
- 边界：
  - `EndTurnSyncPatch` 负责 EndTurnRequest/Confirm 的协商与 UI 锁定。
  - `OnTurnEnd` 仅用于“回合已实际结束后”的边界快照/对齐，不参与协商。

### mana 兼容层
- 背景：历史代码中存在对 `INetworkPlayer.mana` 的直接访问，但该成员未在接口中声明。
- 方案：使用反射兼容层读取/写入实现类上的 `mana` 属性，避免改接口造成破坏性修改。
  - 实现：`networkplugin/Utils/NetworkPlayerManaCompat.cs`
