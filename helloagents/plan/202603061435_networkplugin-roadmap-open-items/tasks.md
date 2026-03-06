# 任务清单: networkplugin-roadmap-open-items

目录: `helloagents/plan/202603061435_networkplugin-roadmap-open-items/`

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
总任务: 29
已完成: 25
完成率: 86%
提取来源: networkplugin/PLANNING_ROADMAP.md v2.6
去重规则:
  - 以 P1/P2/P3 优先级章节为主
  - “关键 TODO 清单”仅作为时间优先级补充
  - 已废弃 SaveSync bytes 同步不纳入执行任务
```

---

## 任务列表

### 1. P1 - 服务器路由与职责收敛

- [√] 1.1 收敛 `Network/Server/NetworkServer.cs` 中 `FullStateSync*`、`RoomState*`、`DirectMessage` 的路由边界
  - 验证: Host/Client/Relay 消息流不再依赖重复入口分支

- [√] 1.2 收敛 `HandleGameEvent` / `HandleSystemMessage` 的双层包装入口，避免同一消息在旧入口和 override 层重复维护
  - 依赖: 1.1

- [√] 1.3 为 FullSync / RoomState 建立固定回归路径，覆盖请求、响应、定向转发与异常路径
  - 依赖: 1.1
  - 验证: 至少有一组固定回归清单可覆盖 `FullStateSyncRequest`、`FullStateSyncResponse`、`RoomStateRequest`、`RoomStateResponse`

### 2. P1 - GapOptions 与远端出牌链路闭环

- [?] 2.1 验证 `Patch/Network/GapOptionsSyncPatch.cs` 中 `GapStationEntered`、`DrinkTea*`、`GapOptions*` 的双端幂等与去重稳定性
  - 验证: 同步事件不重复触发，不因重复收包造成多次表现

- [√] 2.2 将 `GapOptionsSyncPatch` 的房间缓存事件与中途加入 / 重连回归清单绑定
  - 依赖: 2.1
  - 验证: 重连或中途加入后可验证最近 GapOptions 事件是否被正确追赶

- [√] 2.3 排查 `Patch/Network/RemoteCardUsePatch.cs` 的 reverse patch stub 风险，并补全对应回归项
  - 验证: 远端出牌至少覆盖正常出牌、异常分支、回放/结算链路

- [√] 2.4 检查 GapOptions / 远端出牌链路是否存在“状态已同步但 UI/游戏表现未落地”的缺口
  - 依赖: 2.1, 2.3

### 3. P1 - 聊天模型统一

- [√] 3.1 收敛 `Chat/ChatConsole.cs` 中剩余的 `userName/UserName` 反射兜底读取，统一到单一名称解析入口
  - 验证: 发送与接收路径都优先使用 `playerName`

- [√] 3.2 固定聊天协议口径为 `playerName` 主字段 + `username` 兼容字段，并同步更新相关文档说明
  - 依赖: 3.1

- [√] 3.3 验证历史聊天 payload 的兼容反序列化与显示回退
  - 依赖: 3.1
  - 验证: 旧消息仍能显示发送者名称，且不会覆盖新字段语义

### 4. P1 - 固定回归清单落地

- [√] 4.1 建立房间生命周期固定回归：建房、入房、离房、主机切换
  - 验证: 每项都有明确预期行为和日志/状态检查点

- [√] 4.2 建立战斗一致性固定回归：回合、能量、敌人状态、远端出牌链路
  - 验证: 双端状态一致，无重复广播或丢事件

- [√] 4.3 建立功能链路固定回归：交易、复活、地图推进、GapOptions
  - 验证: 功能成功与失败路径均有检查项

- [√] 4.4 建立连接恢复固定回归：断线重连、中途加入、FullSnapshot 追赶
  - 验证: 断线后恢复状态与房主权威状态一致

### 5. P2 - NAT 能力收口与状态源统一

- [√] 5.1 明确 NAT 方向是继续保持 “STUN + UPnP 语义展示” 还是正式接入真实端口映射库
  - 验证: 形成单一结论并能写回路线图/模块文档

- [√] 5.2 清理 `UI/Components/NetworkStatusIndicator.cs` 中与 `Network/Utils/NatTraversal.cs` 并行的 `_upnpEnabled` / `_natType` 旧静态状态字段
  - 依赖: 5.1

- [√] 5.3 将 NAT 类型、UPnP 状态、连接策略文案统一到同一套输出口径
  - 依赖: 5.1, 5.2
  - 验证: UI 显示、日志、文档使用同一术语集合

### 6. P2 - 玩家模型轻量收敛

- [√] 6.1 处理 `Network/NetworkPlayer/dto/` 空占位目录：删除或补正文档说明，消除“双模型并存”误判
  - 验证: 路径语义与实际代码结构一致

- [√] 6.2 统一 `Network/NetworkPlayer/NetWorkPlayer.cs` 的协议字段命名与运行时对象边界
  - 依赖: 6.1

- [√] 6.3 评估 `username/location_X/location_Y` 等 legacy 字段是否需要 mapper、封装或显式兼容层
  - 依赖: 6.2
  - 验证: 至少形成一套明确的迁移/保留策略

### 7. P2 - UI 展示口径统一

- [√] 7.1 保持 `Patch/UI/OtherPlayersOverlayPatch.cs` façade 稳定，同时继续把状态来源收敛到 partial 子模块
  - 验证: 外部调用入口不破坏，内部状态流更清晰

- [√] 7.2 收敛交易/复活面板与多人覆盖层的玩家状态来源
  - 依赖: 7.1

- [√] 7.3 复查 `Patch/UI/ShopTradeIconPatch.cs` 新的按钮标题定位逻辑在商店场景中的回归表现
  - 依赖: 7.2
  - 验证: 仅改克隆交易按钮标题节点；`CardService` / `ReturnButton` 原生容器在隐藏与异常路径都会恢复原始布局快照

### 8. P3 - 扩展功能立项评估

- [√] 8.1 评估成就联机同步是否值得立项，并明确范围、收益和依赖前置条件
  - 验证: 已形成“当前不建议立项”的可行性结论；先维持本地成就体系

- [√] 8.2 评估观战模式是否值得立项，并明确需要的网络 / UI / 房间模型改造面
  - 验证: 已形成“需房间/快照/UI 前置重构后再议”的判断依据

- [√] 8.3 评估调试面板与性能可视化是否单独成项，还是并入现有回归/诊断体系
  - 验证: 已形成“并入现有回归/诊断体系，不单独拆产品项”的结论

### 9. 发布前整体验收与闭环

- [?] 9.1 完成 P1/P2 剩余项后的整体验证，确认主链路、体验项和 UI 口径没有互相回退
  - 依赖: 1.1-7.3
  - 阻塞: `2.1` 仍需双端实机验收，当前无法在静态环境完成最终发布前整体验证

- [?] 9.2 输出一致性问题清单与修复闭环，记录“路线图 → 方案包 → 实施结果”的差异项
  - 依赖: 9.1
  - 阻塞: 等待 `9.1` 完成实机整体验证后再做最终差异归档

- [?] 9.3 评估 P3 是否进入下一轮正式开发计划，并决定是继续拆新方案包还是留在路线图层
  - 依赖: 8.1, 8.2, 8.3, 9.2
  - 阻塞: `8.x` 已形成结论，但仍需等待 `9.2` 的发布前差异闭环后再正式封版

---

## 执行备注

> 执行过程中的重要记录

| 任务 | 状态 | 备注 |
|------|------|------|
| 整体提取 | [√] | 已从 `PLANNING_ROADMAP.md v2.6` 提取全部未完成内容 |
| 去重规则 | [√] | 以 P1/P2/P3 为主，关键 TODO 清单仅做时间语义补充 |
| 废弃项处理 | [√] | `SaveSync bytes` 同步已明确排除，不纳入执行任务 |
| 执行批次 A | [√] | 已完成 1.1/1.2/1.3/3.1/3.2/3.3；新增 `networkplugin/NETWORK_ROUTE_REGRESSION_CHECKLIST.md` 固定回归清单，并补齐历史聊天 payload 回退 |
| 执行批次 B | [√] | 已完成 2.2/2.3/4.1/4.2/4.3/4.4；新增 `networkplugin/MULTIPLAYER_REGRESSION_CHECKLIST.md`，绑定 GapOptions / RemoteCardUse / 房间生命周期 / 连接恢复回归面 |
| GapOptions 审计 | [√] | 已确认 `GapOptionsSyncPatch.OnGameEventReceived(...)` 与 `MergeCatchupGapOptionsEvents(...)` 当前以缓存/日志为主，存在“状态已同步但表现未落地”的真实缺口；2.1 需双端实机继续验证 |
| NAT 收口 | [√] | 已完成 5.1/5.2/5.3；确认当前方向维持 `STUN + UPnP` 语义展示，不接入真实端口映射库，并移除 `NetworkStatusIndicator` 旧静态状态源 |
| 玩家模型收口 | [√] | 已完成 6.1/6.2/6.3；为 `dto/` 补充目录说明，并在 `NetWorkPlayer` 上新增 PascalCase 运行时别名层，保留 legacy JSON 字段不变 |
| 构建验证 | [√] | `dotnet build networkplugin/NetWorkPlugin.csproj -v minimal` 通过（257 warnings，0 errors） |
| UI 显示名收口 | [√] | `OtherPlayersOverlayPatch.ResolveDisplayName(...)` 已成为统一入口；`TradePanel`、复活登记链路、Overlay/地图图标/指向判定均复用同一显示名来源，本地玩家回退固定为 `GameStateUtils.GetCurrentPlayerName()` |
| ShopTrade 回归收口 | [√] | `ShopTradeIconPatch` 现保存并恢复 `CardService` / `ReturnButton` 原生容器的 `RectTransform` 快照；交易按钮仍只修改克隆标题文本 |
| P3 立项评估 | [√] | 已完成 8.1/8.2/8.3：成就同步暂不立项；观战模式需前置重构；调试/性能并入现有诊断体系 |
| 构建复验 | [√] | 本轮修改后再次执行 `dotnet build networkplugin/NetWorkPlugin.csproj -v minimal`，结果为 0 warnings / 0 errors |
| 后续执行 | [ ] | 剩余 `2.1` 双端实机验收，以及 `9.1/9.2/9.3` 发布前整体验证与差异闭环 |
|------|------|------|
