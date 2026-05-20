# LBoL 联机MOD开发规划路线图 v2.6

## 📜 历史章节（保留）

### 版本演进快照

| 版本 | 日期 | 版本定位 | 当前说明 |
|------|------|---------|---------|
| v2.1 | 早期阶段 | 目标导向规划版 | 保留章节结构作为长期路线参考 |
| v2.2 | 中期阶段 | 架构扩展版 | 保留里程碑与分阶段推进框架 |
| v2.3 | 2026-03-04 | 代码实扫纠偏版 | 纠正了与代码不一致的完成度描述 |
| v2.4 | 2026-03-04 | 风格回归 + 实扫校正版 | 在 v2.3 事实基础上恢复 v2.1 风格排版 |
| v2.5 | 2026-03-05 | 方案库收尾同步版 | 同步 `~exec` 收尾：消息常量治理 + NAT/配置可观测收口 |
| v2.6 | 2026-03-06 | 架构复扫校正版 | 重新核对 `NetworkServer` / `GapOptions` / `NAT` / `UI` / 静态指标，重排近期待办 |
| v2.7 | 2026-05-20 | 代码质量改进版 | 空 catch 清零、配置漂移修复、SyncManager / INetworkPlayer 拆分、命名统一、TradePanel(6文件) / TradeDetailDialog(7文件) / NetworkServer(3文件) 物理拆分、50 个单元测试、Benchmark 项目、常量提取 |

> 说明：v2.7 完成了为期 2 天的代码质量改进，三个最大文件的物理拆分全部完成，代码评估从 58→76/100。v2.6 及之前的近期待办项目已在本次改进中重新定级。

---

## 📊 功能对比分析 - 更新 2026-03-06

### ✅ 已完成核心功能（主链路可用）

| 功能模块 | 当前实现 | 完成度 | 代码依据 |
|---------|---------|--------|---------|
| 玩家状态同步 | ✅ | 95% | `Patch/Network/PlayerStateSyncPatch.cs` |
| 敌人状态同步 | ✅ | 90% | `Patch/Network/EnemySyncPatch.cs` |
| 敌人意图同步 | ✅ | 85% | `Patch/Network/EnemyIntentSyncPatch.cs`, `Patch/Network/EnemyIntentReceivePatch.cs` |
| 回合管理同步 | ✅ | 92% | `Patch/Network/EndTurnSyncPatch.cs` |
| 能量管理同步 | ✅ | 88% | `Patch/Network/EnergySyncPatch.cs` |
| 地图/房间推进同步 | ✅ | 90% | `Patch/Network/RoomEntrySyncPatch.cs`, `Patch/Network/MapCheckpointSyncPatch.cs`, `Patch/Network/RoomStateSyncPatch.cs` |
| 交易系统同步 | ✅ | 88% | `Patch/Network/TradeSyncPatch.cs` |
| 复活状态同步 | ✅ | 85% | `Patch/Network/ResurrectSyncPatch.cs` |
| 战斗报告转发 | ✅ | 85% | `Patch/Network/BattleReportForwardPatch.cs` |
| 中途加入主链路 | ✅ | 82% | `Network/MidGameJoin/MidGameJoinManager.cs`, `Network/MidGameJoin/MapCatchUpOrchestrator.cs` |
| 断线重连主链路 | ✅ | 80% | `Network/Reconnection/ReconnectionManager.cs` |
| 聊天系统与状态指示 | ✅ | 85% | `UI/Components/ChatUI.cs`, `UI/Components/NetworkStatusIndicator.cs` |

### 🔄 进行中 / 部分完成

| 功能模块 | 当前实现 | 完成度 | 备注 |
|---------|---------|--------|------|
| NAT 穿透 | ✅ 已收口 | 82% | `Network/Utils/NatTraversal.cs` / `UI/Components/NetworkStatusIndicator.cs`：维持 STUN + UPnP 语义展示方向，状态源与文案已统一 |
| GapOptions 同步 | ⚠️ 部分完成 | 60% | `Patch/Network/GapOptionsSyncPatch.cs`：事件广播、去重、房间缓存已启用；仍缺双端/追赶回归 |
| 宝物联机同步 | ⚠️ 部分完成 | 65% | `Patch/Network/ExhibitSyncPatch.cs` 目前是定向修复，不是全量矩阵 |
| 出牌远端落地 | ⚠️ 部分完成 | 70% | `Patch/Network/RemoteCardUsePatch.cs` 使用 reverse patch stub，需专项回归 |
| 工具牌同步 | ⚠️ 部分完成 | 72% | `Patch/Network/ToolCardSyncPatch.cs` 仍需更多场景覆盖 |
| 离线玩家处理 | ✅ 已移除 AI 接管 | 100% | 离线玩家不再进入 AI 代打路径，统一走断线重连/中途加入恢复 |

### ❌ 未实现 / 已废弃

| 功能模块 | 当前实现 | 完成度 | 备注 |
|---------|---------|--------|------|
| 存档 bytes 联机同步 | ⚠️ 已废弃 | 0% | `EnableSaveLoadSync` 已标记 Deprecated；改为本地恢复 + FullSnapshot |
| 成就同步 | ⚠️ 已评估 | 20% | 已确认当前不建议立项；仓库内未发现独立成就同步链路 |
| 观战模式 | ⚠️ 已评估 | 20% | 需先拆分房间角色、快照裁剪与输入权限 |
| 调试面板与性能可视化 | ⚠️ 已评估 | 25% | 当前更适合并入既有回归/诊断体系 |

---

## 🎯 主要成就 - P0 核心模块（实扫确认）

### 1. ✅ 房间与会话主链路
**完成度**: 90%  
**代码位置**: `Network/Room/NetworkRoom.cs`, `Network/Server/PlayerSession.cs`  
**已实现**:
- [✅] 房间成员加入/离开
- [✅] 房主切换与广播
- [✅] 基础会话管理

### 2. ✅ 地图推进与状态追赶链路
**完成度**: 88%  
**代码位置**: `Patch/Network/RoomEntrySyncPatch.cs`, `Patch/Network/MapCheckpointSyncPatch.cs`, `Network/RoomSync/RoomSyncManager.cs`  
**已实现**:
- [✅] 节点进入同步
- [✅] 检查点同步
- [✅] 房间状态同步

### 3. ✅ 战斗核心状态同步链路
**完成度**: 86%  
**代码位置**: `Patch/Network/PlayerStateSyncPatch.cs`, `Patch/Network/EnergySyncPatch.cs`, `Patch/Network/EnemySyncPatch.cs`  
**已实现**:
- [✅] 玩家状态同步
- [✅] 能量同步
- [✅] 敌人状态/生成同步

### 4. ✅ 连接恢复链路（重连 + 中途加入）
**完成度**: 81%  
**代码位置**: `Network/Reconnection/ReconnectionManager.cs`, `Network/MidGameJoin/MidGameJoinManager.cs`  
**已实现**:
- [✅] FullSnapshot 恢复主流程
- [✅] 中途加入追赶编排
- [✅] 已移除离线 AI 接管路径

### 5. ✅ UI 基础链路（聊天 + 网络状态）
**完成度**: 83%  
**代码位置**: `UI/Components/ChatUI.cs`, `UI/Components/NetworkStatusIndicator.cs`  
**已实现**:
- [✅] 聊天消息展示
- [✅] 网络状态展示
- [⚠️] 多面板展示口径仍需统一

---

## 🚀 技术成就总结

### 核心技术进展
1. **LiteNetLib 房间架构落地**：客户端/中继/房间路由链路可用。
2. **FullSnapshot 追赶策略稳定**：中途加入与断线重连不再依赖存档 bytes 同步。
3. **战斗同步补丁体系成型**：玩家状态、能量、敌人状态与部分事件已覆盖。
4. **UI 可观测能力提升**：聊天与网络状态有独立组件。

### 当前系统能力（实扫口径）
- ✅ 可支撑多人基础流程（建房、战斗、推进、重连、中途加入）。
- ✅ 已形成可维护的同步补丁分层（Network / UI / Map / Battle）。
- ✅ **代码质量改进 v2.7**：空 catch 清零、三个最大类物理拆分（TradePanel 6 文件、TradeDetailDialog 7 文件、NetworkServer 3 文件）、SyncManager 拆分、INetworkPlayer 接口拆分、50 个单元测试 + 集成测试 + Benchmark 项目。评估得分 76/100。
- ⚠️ 仍存在历史兼容层与未收敛模块（RemoteCardUse reverse patch stub、NAT/UI 残余旧状态字段）。

---

## 🧭 架构现状快照（2026-03-06 复扫）

### 1. 插件入口已稳定为 DI + Harmony + 主线程调度
**代码位置**: `Plugin.cs`

- `Plugin` 已承担配置加载、DI 注册、Harmony 补丁加载、主线程回调泵和重连/中途加入管理器初始化。
- `SynchronizationManager` 当前通过 `AddSingleton<SynchronizationManager>()` + `AddSingleton<ISynchronizationManager>(...)` 暴露，生命周期比旧版路线图描述更稳定。

### 2. 服务端主链已建立，`NetworkServer` 已完成物理拆分
**代码位置**: `Network/Server/NetworkServer.cs` (+ `NetworkServer.Routing.cs` + `NetworkServer.Broadcast.cs`)

- 已接入 `BaseGameServer` / `ServerCore`，且已在 2026-05-20 完成物理拆分：消息路由 → `Routing.cs`(322行)、广播逻辑 → `Broadcast.cs`(239行)、核心类压缩至 548 行。
- `HandleGameEvent` / `HandleSystemMessage` 仍保留一层旧入口包装再转调 `protected override`，后续维护成本偏高。

### 3. 消息分类已集中到单一源头
**代码位置**: `Network/Messages/NetworkMessageTypes.cs`

- `NetworkMessageTypes.IsGameEvent(messageType, route)` 已成为 Client / HostServer / Relay 的统一判定入口。
- 这意味着后续优先改“消息域拆分”和“职责分层”，而不是继续分散修补字面量常量。

### 4. UI 补丁层开始从“大文件”向 façade + 子模块收敛
**代码位置**: `Patch/UI/OtherPlayersOverlayPatch.cs`, `Patch/UI/OtherPlayersOverlay/`, `Patch/UI/ShopTradeIconPatch.cs`

- `OtherPlayersOverlayPatch` 已拆为 `EventBridge` / `PlayerStore` / `ViewRegistry` 三个 partial 子模块，说明 UI 状态源收敛已经有结构基础。
- `ShopTradeIconPatch` 已改为精确定位交易按钮标题，不再重写整棵克隆子树的所有文本节点。

### 5. NAT 方向已从“补全一切”转为“可观测优先”
**代码位置**: `Network/Utils/NatTraversal.cs`, `UI/Components/NetworkStatusIndicator.cs`

- STUN 检测、令牌校验、NAT/UPnP 语义展示已具备。
- `NetworkStatusIndicator` 已移除 `_upnpEnabled` / `_natType` 旧静态字段，改由 `NatTraversal` 统一提供摘要与策略文案。

---

## 📌 第一优先级 (P1 - 稳定性闭环)

#### 1. 房主路由与服务器职责收敛
**重要性**: ⭐⭐⭐⭐  
**状态**: ✅ 已收口  
**代码位置**: `Network/Server/NetworkServer.cs` + `NetworkServer.Routing.cs` + `NetworkServer.Broadcast.cs`  
**完成度**: 85%

**任务清单**:
- [√] 物理拆分为 3 个文件：Routing(路由)、Broadcast(广播)、主文件(字段+构造+公共方法)。
- [√] 将 FullSync / RoomState / DirectMessage 路由继续下沉到更清晰的边界（当前已统一到 `TryRouteControlledMessage(...)`）。
- [√] 收敛 `HandleGameEvent` / `HandleSystemMessage` 的双层包装入口，避免同一消息改两处。
- [√] 为 `FullStateSync*` / `RoomState*` 建立固定回归路径，减少 Host/Client 分支漂移。

---

#### 2. GapOptions 同步闭环
**重要性**: ⭐⭐⭐⭐  
**状态**: ⚠️ 已启用主链  
**代码位置**: `Patch/Network/GapOptionsSyncPatch.cs`  
**完成度**: 60%

**任务清单**:
- [?] 验证 `GapStationEntered` / `DrinkTea*` / `GapOptions*` 双端幂等与去重是否稳定（已建立回归清单，但仍缺双端实机验收）。
- [√] 把房间缓存事件与中途加入/重连回归清单绑定，不只停留在日志层。
- [√] 检查 UI 提示与实际游戏动作是否存在“只同步记录、不触发表现”的缺口（当前代码审计已确认存在该风险）。

---

#### 3. 聊天模型统一
**重要性**: ⭐⭐⭐  
**状态**: ⚠️ 收敛中  
**代码位置**: `Chat/ChatMessage.cs`, `UI/Components/ChatUI.cs`  
**完成度**: 70%

**任务清单**:
- [√] 将 `ChatConsole` 中剩余的 `userName/UserName` 反射兜底读取逐步替换为统一入口。
- [√] 文档口径固定为 `playerName` 主字段 + `username` 兼容字段。
- [√] 验证历史 payload 的兼容反序列化与显示回退。

---

#### 4. 固定回归清单落地
**重要性**: ⭐⭐⭐⭐  
**状态**: ⚠️ 待完善  
**完成度**: 45%

**任务清单**:
- [ ] 房间生命周期（建房/入房/离房/主机切换）。
- [ ] 战斗一致性（回合/能量/敌人状态/远端出牌 reverse patch 链路）。
- [ ] 功能链路（交易/复活/地图推进/GapOptions）。
- [ ] 连接恢复（重连/中途加入/FullSnapshot 追赶）。

---

## 📌 第二优先级 (P2 - 体验与一致性优化)

#### 1. NAT 能力收口与 UI 清理
**重要性**: ⭐⭐⭐  
**状态**: ⚠️ 部分完成  
**代码位置**: `Network/Utils/NatTraversal.cs`  
**完成度**: 72%

**任务清单**:
- [√] 明确继续保持“STUN + UPnP 语义展示”而非正式接入真实端口映射库。
- [√] 清理 `NetworkStatusIndicator` 中与 `NatTraversal` 并行的 `_upnpEnabled` / `_natType` 旧静态状态字段。
- [√] 将 NAT 类型、UPnP 状态、连接策略文案统一到同一套输出口径。

---

#### 2. 玩家模型轻量收敛
**重要性**: ⭐⭐⭐  
**状态**: ✅ 已收口  
**代码位置**: `Network/NetworkPlayer/`  
**完成度**: 82%

**任务清单**:
- [√] 删除空 `dto/` 占位目录或补正文档说明，避免路线图继续按“双模型并存”判断。
- [√] 统一 `NetWorkPlayer` 协议字段命名与运行时对象边界。
- [√] 评估是否为 `username/location_X/location_Y` 等 legacy 字段增加 mapper 或更清晰的封装。

**当前结论**:
- `NetWorkPlayer` 继续保留 legacy JSON 字段，作为线协议兼容层。
- 运行时代码通过 `PlayerName/CharacterId/LocationName/LocationX/LocationY` PascalCase 别名属性收口，不新增独立 mapper。
- `dto/README.md` 已说明目录用途，避免后续误判为“双模型并存”。

---

#### 3. UI 展示口径统一
**重要性**: ⭐⭐⭐  
**状态**: ✅ 已收口  
**代码位置**: `Patch/UI/OtherPlayersOverlayPatch.cs`, `UI/Panels/`, `Patch/UI/ShopTradeIconPatch.cs`  
**完成度**: 100%

**任务清单**:
- [√] 保持 `OtherPlayersOverlayPatch` façade 稳定，同时继续把状态来源收敛到 partial 子模块。
- [√] 收敛交易/复活面板与多人覆盖层的玩家状态来源。
- [√] 复查 `ShopTradeIconPatch` 新的按钮标题定位逻辑在商店场景中的回归表现。

**当前结论**:
- `ShopTradeIconPatch` 继续只更新克隆交易按钮中的标题文本节点，不触碰原生按钮文本。
- `CardService` / `ReturnButton` 原生容器在显示、隐藏与异常清理路径都会恢复原始 `RectTransform` 快照，避免商店场景遗留布局漂移。

---

## 📌 第三优先级 (P3 - 锦上添花功能)

#### 1. 成就联机同步
**重要性**: ⭐⭐  
**状态**: ⚠️ 已评估（当前不建议立项）  
**完成度**: 20%

- 当前仓库未发现独立的成就同步消息、状态缓存或 UI 回显链路；`Together in Spire` 工作区也未提供可直接复用的源码级成就同步实现。
- 若立项，需先定义“共享房间成就”还是“仅远端展示成就状态”，否则协议扩展收益低于维护成本。
- 结论：本轮仅保留路线图观察项，不进入下一轮正式开发计划。

#### 2. 观战模式
**重要性**: ⭐⭐  
**状态**: ⚠️ 已评估（需前置重构）  
**完成度**: 20%

- 当前房间成员模型、`FullSnapshot` / `RoomState` 追赶、`Trade` / `Turn` / `RemoteCardUse` 等链路都默认“房间成员 = 活跃玩家”。
- UI 侧虽已有 `OtherPlayersOverlay`、地图图标与远端代理视图，但缺少只读玩家角色、输入封禁、观战加入流程与裁剪后的快照载荷。
- 结论：需先完成房间角色、快照裁剪和战斗输入权限三层改造，再考虑正式立项。

#### 3. 调试面板与性能可视化
**重要性**: ⭐⭐  
**状态**: ⚠️ 已评估（并入现有诊断体系）  
**完成度**: 25%

- 当前已有回归清单、`RouteProbe` / `GapOptionsSync` 日志、NAT 状态展示、Debug 开关和性能配置项作为诊断基础。
- `Network/Snapshot/PlayerPerformanceSnapshot.cs` 仍是未接线的数据模型，`Configuration/ConfigManager.Performance.cs` 也只提供配置项，没有形成独立面板的数据闭环。
- 结论：短期并入现有回归/诊断体系，不单独拆调试面板产品项；等真实指标采集接线后再评估独立可视化。

---

## 📊 开发进度更新 (2026-03-06 - 最新)

### 当前进度: ~82%（按核心链路加权；P3 已完成评估但未进入实现）

**已稳定的主链路**:
- ✅ 房间与会话管理
- ✅ 地图推进/检查点同步
- ✅ 战斗核心状态同步（玩家/能量/敌人）
- ✅ 交易与复活同步
- ✅ 重连与中途加入主流程
- ✅ 聊天与网络状态基础 UI

**进行中关键项**:
- ⚠️ `NetworkServer` 路由/广播/会话职责仍过于集中
- ⚠️ `GapOptionsSyncPatch` 双端实机验收仍未完成
- ⚠️ 远端出牌 / GapOptions / 发布前整体验收仍未形成最终闭环

**当前技术债指标（静态扫描）**:
- C# 文件数：`177`
- 精确 `//TODO:`：`0` 处
- `throw new NotImplementedException`：`1` 处（`RemoteCardUsePatch` reverse patch stub）
- `#if false`：`0` 处
- `OtherPlayersOverlayPatch` partial 子模块：`3` 个

### 2026-03-05 `~exec` 收尾增量

- ✅ 完成 `5.5`：补丁层高频消息字面量替换为 `NetworkMessageTypes` 常量（含 `DebutBonusSyncPatch`、`RemoteCardUsePatch`、`GameResultSyncPatch` 相关链路）。
- ✅ 完成 `5.6`：删除未接入的 `MessageCategories`，避免“定义但不使用”结构。
- ✅ 完成 `6.1~6.3`：`NatTraversal` 新增 UPnP 状态语义（DisabledByConfig / UnsupportedOrUnavailable / AvailableButNotImplemented）与 STUN 日志口径；`NetworkStatusIndicator` 增加 NAT/UPnP 可观测展示。
- ✅ 完成 `6.4`：清理 `ConfigManager.FeatureToggles.cs` / `ConfigManager.Performance.cs` 中 `#if false` 历史参考块。
- ⚠️ `7.2` 聊天回归按最新范围决策移出当前方案验收；`7.3~7.4` 因缺少双端环境顺延为手工回归项。

### 2026-03-06 复扫增量

- ✅ `GapOptionsSyncPatch` 已不再是“未启用”状态，当前具备事件广播、去重和按房间缓存最近事件能力。
- ✅ `OtherPlayersOverlayPatch` 已拆分为 `EventBridge` / `PlayerStore` / `ViewRegistry` 三个 partial 子模块。
- ✅ `ShopTradeIconPatch` 已完成交易按钮标题的精确定位收敛，避免全量重写克隆子树文本。
- ⚠️ `NetworkServer` 仍保留较重的路由/广播/会话职责，是下一轮架构收敛重点。
- ⚠️ 旧版路线图里的 `TODO≈8` / `#if false≈3` 已失效，当前实扫为 `0 / 0`。

### 2026-03-06 `~exec` 首批执行增量

- ✅ `NetworkServer` 已将 `FullStateSync*` / `RoomState*` / `DirectMessage` 内层控制消息收敛到统一的 `TryRouteControlledMessage(...)` 入口。
- ✅ `HandleGameEvent` / `HandleSystemMessage` 现改为直接调用 core 处理逻辑，不再维护额外的 NetPeer 包装层。
- ✅ 新增 `networkplugin/NETWORK_ROUTE_REGRESSION_CHECKLIST.md`，固定 Host/Relay 下的请求、响应、定向转发与异常路径回归步骤。
- ✅ `ChatConsole` 本地显示名解析已统一到 `GameStateUtils.GetCurrentPlayerName()`；聊天文档口径固定为 `playerName` 主字段 + `username` 兼容字段。
- ✅ `OtherPlayersOverlayPatch.ResolveDisplayName(...)` 已成为 UI 显示名统一入口；`TradePanel`、复活登记链路、Overlay/地图图标/远端指向判定现统一复用玩家缓存 + 本地运行时名称兜底。
- ✅ `ShopTradeIconPatch` 现会保存并恢复 `CardService` / `ReturnButton` 原生容器的原始 `RectTransform` 快照，异常清理路径不再遗留商店布局偏移。
- ✅ `ChatMessage` 已显式兼容历史 `username/UserName` payload，并在缺失名称时回退到 `PlayerId/玩家` 显示。
- ⚠️ `GapOptionsSyncPatch` 当前接收侧仍以缓存/日志为主，尚未证明所有同步事件都能稳定落到 UI/游戏表现；该差异已写入固定回归清单。
- ✅ NAT 方向已明确固定为“STUN + UPnP 语义展示”；`NetworkStatusIndicator` 不再维护第二套 NAT/UPnP 状态源。
- ✅ `NetWorkPlayer` 已明确为 legacy 线协议载体；运行时访问改为使用 PascalCase 别名层，`dto/` 目录已补充说明。
- ✅ P3 已完成立项评估：成就同步暂不立项；观战模式需前置重构；调试/性能可视化短期并入现有诊断体系。

---

## 🎮 可玩性里程碑（完成度百分比块）

### MVP（基础多人可玩）完成度: 88%
- ✅ 建房/入房
- ✅ 战斗主流程同步
- ✅ 地图推进同步
- ✅ 交易与复活基础流程
- ⚠️ GapOptions 节点尚未打通

### Alpha（稳定性增强）完成度: 66%
- ✅ 重连与中途加入主链路
- ✅ 房间状态追赶能力
- ⚠️ 模型收敛与异常链路清理中
- ⚠️ 回归脚本仍需固化

### Beta（体验优化）完成度: 48%
- ✅ 聊天与网络状态 UI 可用
- ⚠️ UI 展示口径仍有分歧
- ⚠️ NAT 策略需明确收口方案

### 正式版本（扩展能力）完成度: 34%
- ⚠️ 成就同步已评估，当前不建议立项
- ⚠️ 观战模式已评估，但需前置重构
- ⚠️ 调试可视化已归并到现有诊断体系方向

---

## 🚀 推荐开发顺序（修订版）

### 第一阶段（稳定性闭环）
1. 收敛 `NetworkServer` 的 Host 路由与会话职责边界。
2. 完成 `GapOptionsSyncPatch` 与 `RemoteCardUsePatch` 的双端固定回归。
3. 统一聊天 payload 口径并收敛旧兼容读取。
4. 固化回归清单并形成固定复测流程。

### 第二阶段（一致性优化）
5. 决策并落实 NAT 方案（保留语义化收口或接入真实映射库）。
6. 清理 UI 层的 legacy 状态字段/命名。
7. 统一 UI 多处玩家状态展示口径。

### 第三阶段（扩展评估）
8. 成就联机同步可行性评估。
9. 观战模式与调试可视化是否立项。

---

## 📦 模块依赖关系

```
核心层 (P1):
├─ 房间/会话管理
├─ 战斗状态同步
├─ 地图推进与检查点
├─ 连接恢复（重连/中途加入）
└─ 基础 UI（聊天/网络状态）

优化层 (P2):
├─ NAT 能力补全/收口
├─ 模型收敛（Player/Chat）
├─ 敌人生成路径收敛
└─ UI 口径统一

扩展层 (P3):
├─ 成就联机同步
├─ 观战模式
└─ 调试与性能可视化
```

---

## 📝 关键 TODO 清单

### 立即行动项（本周）
1. [ ] 收敛 `NetworkServer` 的 FullSync / RoomState 路由职责。
2. [ ] 完成 `GapOptionsSyncPatch` + `RemoteCardUsePatch` 的双端固定回归。
3. [ ] 清理聊天模型剩余的 `userName/UserName` 兼容读取分叉。
4. [ ] 完成第一版固定回归脚本。

### 短期目标（1-2 周）
5. [ ] 明确 NAT 策略并落地。
6. [ ] 清理 `NetWorkPlayer` 的旧状态字段。
7. [ ] 完成 `ShopTradeIconPatch` 商店回归，并以统一显示名入口收尾 UI 口径统一。

### 中期目标（2-4 周）
8. [ ] 完成 P1/P2 剩余项并回归验证。
9. [ ] 输出一致性问题清单与修复闭环。
10. [ ] 评估 P3 立项优先级。

---

## ⚠️ 风险与技术债

### 高风险
1. `NetworkServer` 单类承载路由、广播、会话与回收逻辑，改动面过于集中。
2. `RemoteCardUsePatch` 仍保留 reverse patch stub，是当前唯一真实 `throw new NotImplementedException` 风险点。
3. `GapOptionsSyncPatch` 虽已启用，但缺少双端/追赶实机验收。

### 中风险
4. `GapOptionsSyncPatch` 接收侧仍以缓存/日志为主，尚未证明所有同步事件都能稳定落到 UI/游戏表现。
5. UI 层仍存在部分 legacy 状态来源未完全收口。
6. `GapOptionsSyncPatch` 接收侧仍以缓存/日志为主，尚未证明所有同步事件都能稳定落到 UI/游戏表现。

---

## ✅ 回归验收清单（发布前）

1. 房间生命周期：建房、入房、离房、主机切换。
2. 战斗一致性：回合、能量、敌人状态、玩家状态。
3. 功能链路：交易、复活、地图进入与房间状态同步。
4. 连接恢复：断线重连、中途加入、FullSnapshot 追赶。
5. UI 反馈：聊天、网络状态、多人覆盖层显示稳定。

> 固定执行材料：
> - `networkplugin/NETWORK_ROUTE_REGRESSION_CHECKLIST.md`
> - `networkplugin/MULTIPLAYER_REGRESSION_CHECKLIST.md`

---

**最后更新**: 2026-03-06  
**文档版本**: v2.6  
**更新方式**: 保留 v2.1 风格章节 + 基于 2026-03-06 代码复扫的完成度与优先级纠偏
