# networkplugin

## 职责

提供联机同步与 UI 扩展补丁（HarmonyPatch + LiteNetLib）。

---

以下内容从旧版 wiki 迁移，保持原文：

# 模块: networkplugin

## 目的
提供联机同步与 UI 扩展补丁。

## 规范
### Trade（交易同步）
- Host 权威会话：`networkplugin/Patch/Network/TradeSyncPatch.cs` 维护 `TradeSessionState` 并广播。
- Client UI：`networkplugin/UI/Panels/TradePanel.cs` 仅联机可用，完成后仅对本地 `GameRun` 落地。
- `networkplugin/UI/Factories/TradePanelRuntimeFactory.cs` 与 `networkplugin/UI/Widgets/TradeSlotWidget.cs` 当前约定为：常规 Unity UI 访问（按钮、图片、`FindObjectsByType`、模板克隆、Tooltip 显隐）默认直接走主路径，不再用静默 `try-catch` 包裹；仅对外部资源装载或重连类边界行为保留必要异常兜底。
- `networkplugin/UI/Panels/TradePanel.cs` 与 `networkplugin/UI/Dialogs/TradeDetailDialog.cs` 当前约定为：按钮回调、列表重建、文本刷新、TradePanel/Dialog 间的常规返回链路默认直接执行；仅对交易结算、取消请求、异步回跳和原生 overlay 构建外层保留必要异常保护。

#### TradePanel partner picker（选择交易对象）
- 交易对象选择弹层在运行时克隆游戏内 `UI/Dialogs/MessageDialog` 作为窗口框架，并复用 `UI/Panels/HistoryPanel` 的 `ScrollRect + RecordRow` 构建列表。
- 部分环境下 UI 指针事件可能无法到达行模板（RaycastTarget/层级/输入系统差异）；因此提供额外的“点击捕获器”辅助路径：每帧通过 Unity InputSystem `Mouse`（并兼容 legacy `UnityEngine.Input`）检测左键点击，再用 RectTransform 命中测试遍历候选项内全部 `Graphic` 以解析点击到的玩家。

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
- `ShopTradeIconPatch` 在克隆 `CardService` 按钮样式后，仅解析并更新交易按钮的主标题文本；只有未命中 `TMP_Text` 时才回退到 legacy `Text`，并在 fallback/异常层级时输出 hierarchy 日志。
- `ShopTradeIconPatch` 现会为 `CardService` / `ReturnButton` 原生容器保存 `anchoredPosition/sizeDelta/localScale/localPosition` 快照，并在隐藏或异常清理时完整恢复，避免商店场景残留布局漂移。
- 两者通过 `networkplugin/Patch/UI/TradeUiMessages.cs` 统一“未连接/配置禁用/缺少 TradePanel 实例”等提示。

#### 主菜单多人入口（多人游戏）

目的：在主菜单（新游戏/继续/设置同一层级）提供默认可见的“多人游戏”入口，并进入插件提供的联机方式选择 UI。

实现：`networkplugin/Patch/UI/MainMenuMultiplayerEntryPatch.cs`

- 注入点：
  - `MainMenuPanel.Awake` Postfix
  - `MainMenuPanel.RefreshProfile` Postfix
- 按钮创建：克隆主菜单现有按钮作为模板，并插入到按钮组中。
- 模板按钮定位（分层降级）：
  1) 优先读取 `MainMenuPanel` 的 `newGameButton` 字段
  2) 反射扫描 `MainMenuPanel` 的所有 `Button` 字段
  3) 从面板子节点扫描 `Button` + `TextMeshProUGUI` 作为候选兜底
- 点击行为：打开遮罩式“多人游戏”面板（非 `MessageDialog`），提供三个按钮：
  - 做房主：启动本机服务器并连接
  - 加入房主：弹出加入确认并连接配置的服务器地址
  - 返回：关闭面板

说明：主菜单结构变动时，若模板定位失败会输出 warning 日志但不会导致主菜单崩溃。

### 远端队友作为出牌目标（单体目标）
- 通过 `RemotePlayerProxyEnemy : EnemyUnit` 兼容 `UnitSelector.SelectedEnemy` 的类型约束。
- 发送端在 `Card.GetActions` 处拦截：不本地结算，改为发送 `OnRemoteCardUse`。
- 目标端接收后按蓝图结算，再发送 `OnRemoteCardResolved` 广播结算后的状态快照。
- 接收端对 `OnRemoteCardResolved` 做 `ResolveSeq/Timestamp/RequestId` 去重与乱序丢弃，避免状态回滚。

### 聊天字段口径
- 聊天协议主字段固定为 `playerName`（`ChatMessage.PlayerName`）。
- 历史兼容字段保留为 `username`（`ChatMessage.LegacyUsername`）；同时接受旧包里的 `UserName` 只读输入兼容，不再作为新消息主写入字段。
- `ChatConsole` 发送端统一通过 `GameStateUtils.GetCurrentPlayerName()` 解析本地显示名，避免在聊天层重复维护 `userName/UserName/Name` 反射兜底。
- `ChatMessage.GetDisplayPlayerName()` 负责最终显示回退：优先 `playerName`，其次 `PlayerId`，最后显示 `玩家`。

### Host 路由固定回归
- `NetworkServer` 中 `FullStateSync*` / `RoomState*` 的受控路由已收敛到 `TryRouteControlledMessage(...)`，由 GameEvent/SystemMessage/DirectMessage 共用。
- 固定回归清单位于 `networkplugin/NETWORK_ROUTE_REGRESSION_CHECKLIST.md`，覆盖 Host/直连、Relay、异常路径与 `RouteProbe` 日志观察点。

### 综合多人回归清单
- `networkplugin/MULTIPLAYER_REGRESSION_CHECKLIST.md` 覆盖房间生命周期、战斗一致性、交易/复活/地图推进、GapOptions、断线重连、中途加入与 FullSnapshot 追赶。
- 其中 `RemoteCardUsePatch.Card_GetActions_Original(...)` 被视为 ReversePatch 白名单桩；回归重点是“不能真的落到桩体抛异常”，而不是简单删除该方法。
- `GapOptionsSyncPatch.MergeCatchupGapOptionsEvents(...)` 与 `GapOptionsSyncPatch.OnGameEventReceived(...)` 当前都以缓存/日志为主，不直接重放动作；回归时必须额外核对 UI/游戏表现是否真正落地。

### 玩家身份与玩家列表来源
- 客户端侧“玩家列表/数量”由 `NetworkManager` 维护（轻量缓存），不承担完整权威同步。
- 服务器侧分配的 `PlayerId` / Host 信息由 `NetworkIdentityTracker` 从 GameEvent 提取并缓存。
- `NetworkManager` 在收到 `Welcome/PlayerListUpdate/PlayerJoined/PlayerLeft` 等事件后同步缓存，并可通过 `GetAllPlayers/GetPlayerCount/GetPlayer` 查询。
- `OtherPlayersOverlayPatch.ResolveDisplayName(...)` 现作为 UI 层统一显示名入口：优先使用调用方显式传入名称，其次读取 `OtherPlayersOverlay` 玩家缓存，对本地玩家再用 `GameStateUtils.GetCurrentPlayerName()` 做运行时兜底。
- `TradePanel`、`DeathPatches` / `ResurrectSyncPatch`、Overlay 头像条、地图图标与远端目标判定都应复用该入口，避免再次在各 UI 面板内部分叉 `PlayerName/playerId/角色名` 的回退顺序。

### MidGameJoin（中途加入）
- 消息类型：
  - `MidGameJoinRequest` / `MidGameJoinResponse`：Joiner ⇄ Host 的请求/批准（通过 Relay `DirectMessage` 转发）。
  - `FullStateSyncRequest` / `FullStateSyncResponse`：Joiner 向 Host 拉取 `FullSnapshot + MissedEvents`（通过 Relay `DirectMessage` 转发，绕开 RelayServer 的 FullSync 路由 TODO）。
- 最小流程：
  1. Joiner 先 `JoinRoom` 成为房间成员（旁观者）。
  2. Joiner 发送 `DirectMessage(Target=Host, Type=MidGameJoinRequest, Payload=无)`。
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
  - `FullStateSyncRequest`：服务端定向转发给房主客户端（由房主侧 `MidGameJoinManager` 校验 JoinToken 并生成响应），并通过统一路由入口屏蔽错误目标。
  - `FullStateSyncResponse`：服务端仅单播给 `TargetPlayerId`，非 Host 响应会被直接丢弃。

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

### NetworkPlayer 模型边界
- `networkplugin/Network/NetworkPlayer/NetWorkPlayer.cs` 继续承担**历史线协议载体**职责，保留 `username`、`location_X`、`location_Y` 等字段名，避免破坏 JSON 兼容。
- 新运行时代码应优先使用 `NetWorkPlayer.PlayerName`、`CharacterId`、`LocationName`、`LocationX`、`LocationY` 这些 PascalCase 别名属性，而不是继续扩散 legacy 字段名。
- `networkplugin/Network/NetworkPlayer/dto/README.md` 明确说明：`dto/` 目录当前为未来 Wire DTO/Mapper 拆分预留，现阶段不代表“双模型并存”。

## NAT Traversal（NAT/端点辅助）

### 目标场景
- 主场景：虚拟局域网（VPN/LAN）直连房主 IP。
- 端口：固定且由用户输入。

### NatTraversal 约定
- 实现：`networkplugin/Network/Utils/NatTraversal.cs`
- `NatTraversal.NatInfo` 会被 Relay 侧缓存/转发（见 `RelayServer.HandleNatInfoReport/HandleNatInfoRequest`），因此需要稳定的 JSON 形状。

### 端点序列化
- `IPEndPoint` 默认无法被 `System.Text.Json` 序列化。
- 方案：使用属性级 `JsonConverter<IPEndPoint>`，字符串格式为 `ip:port`，保证两端默认 `JsonSerializer.Serialize/Deserialize` 可用。

### Token
- `GenerateConnectionToken/ValidateConnectionToken` 使用 TTL 校验（默认 5 分钟），用于防误用与过期控制（熟人局域网场景，不做强签名）。

### UPnP/STUN
- 当前方向固定为“STUN 检测 + UPnP 语义展示”，不把真实端口映射库接入主流程。
- UPnP：默认按“不支持/不可用”处理，不作为主流程依赖（失败仅记录日志）。
- STUN：提供最小 Binding 探测获取公网端点，作为可选增强/诊断手段。
- `NetworkStatusIndicator` 的 NAT/UPnP 展示现统一读取 `NatTraversal.GetStatusSummary()` 与 `NatTraversal.GetConnectionStrategySummary()`，不再保留独立旧状态缓存。

### P3 立项评估（2026-03-06）
- 成就联机同步：当前仓库未发现独立 achievement 消息、状态缓存或 UI 回显链路；本轮结论为暂不立项。
- 观战模式：当前房间成员模型、`FullSnapshot` / `RoomState` 追赶、`Trade` / `Turn` 输入链路都默认“成员即活跃玩家”；若要支持观战，需先拆分房间角色与输入权限。
- 调试面板 / 性能可视化：现有诊断基础更适合继续并入回归/日志体系；`networkplugin/Network/Snapshot/PlayerPerformanceSnapshot.cs` 仍未接线，`networkplugin/Configuration/ConfigManager.Performance.cs` 仅提供配置项。

## 接口定义（可选）

> 模块对外暴露的公共API和数据结构

### 公共API
| 函数/方法 | 参数 | 返回值 | 说明 |
|----------|------|--------|------|
| (见文档原文) | - | - | 模块接口以代码为准，文档记录关键约定 |

### 数据结构
| 字段 | 类型 | 说明 |
|------|------|------|
| - | - | - |

## 行为规范

> 描述模块的核心行为和业务规则

### 核心场景
**条件**: 需要联机连接（Host/Relay）
**行为**: 按模块约定发送/接收事件并保证一致性
**结果**: 本地与远端状态收敛一致

## 依赖关系

```yaml
依赖: protocol, networkplayer
被依赖: 无
```
