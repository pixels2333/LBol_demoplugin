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
- UPnP：默认按“不支持/不可用”处理，不作为主流程依赖（失败仅记录日志）。
- STUN：提供最小 Binding 探测获取公网端点，作为可选增强/诊断手段。

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
