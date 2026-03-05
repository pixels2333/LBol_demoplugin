# 变更提案: networkplugin-review-optimization-closure

## 元信息
```yaml
类型: 优化治理
方案类型: implementation
优先级: P0-P2
状态: 草稿
创建: 2026-03-05
来源: networkplugin 审查结论（聊天协议漂移 / 敌人生成功能双路径 / Campfire补丁禁用 / NAT占位实现 / 玩家模型重复 / 幽灵方法冗余）
```

---

## 1. 需求

### 背景
本次审查聚焦 `networkplugin/`，确认当前核心问题不是单点 bug，而是“历史兼容层 + 协议口径分叉 + 未收敛补丁链路”叠加，具体表现为：

1. 聊天消息 DTO 在 `ChatMessage.cs` 与 `ChatUI.cs` 双轨并存，字段语义不一致（`Username` vs `PlayerName`）。
2. `CampfireSyncPatch.cs` 整体被 `#if false` 禁用，消息常量仍存在，形成“定义可用但功能不可用”的漂移。
3. 敌人生成存在 `EnemySpawned` 与 `BattleEnemySpawned` 双路径，后续意图/状态同步对 SpawnId 的一致性依赖被放大。
4. `NetWorkPlayer` 在两个目录中重复定义，且包含未实现方法，主流程已转向 `INetworkPlayer` 体系。
5. 消息常量中心已存在，但路由层仍混入字面量判断，`IsGameEvent` 规则在 Client/Server/Relay 存在分叉风险。
6. NAT 辅助中 UPnP 仍是占位行为，能力边界与日志表达不够收口。
7. 配置层仍保留 `#if false` 参考块，增加维护噪音。
8. 代码中存在“定义即终点”的幽灵方法（仅定义无调用），包括玩家历史模型占位方法、UI 私有工具函数及插件入口遗留字段，持续增加维护噪音与误判风险。

### 目标
1. 建立“单一聊天协议模型”，移除 UI 层本地 DTO 重定义。
2. 重新启用并闭合 Campfire 网络同步链路（至少覆盖 Upgrade/RemoveCard）。
3. 将敌人生成同步收敛为单路径，保障 SpawnId 在生成/意图/状态链路中的一致性。
4. 完成玩家模型收敛：去重 `NetWorkPlayer` 历史定义，统一落到 `INetworkPlayer`。
5. 统一消息路由规则，消除关键流程中的消息名字面量。
6. 明确 NAT/UPnP 支持边界：短期收口、长期可演进。
7. 清理配置遗留噪音，降低后续维护成本。
8. 清理高置信幽灵方法与遗留字段，建立“可删除/需保留（反射/ReversePatch）”边界。

### 非目标
- 不恢复“字节级存档同步”旧链路（保持当前弃用策略）。
- 不重写整个网络架构（仅收敛高风险漂移点）。
- 不在本提案内引入新的大型 UI 功能。

### 约束条件
```yaml
兼容性: 保持现有房间基础联机流程可用（Host/Join/心跳/基础战斗同步）
可回滚性: 每个子项按独立提交执行，支持按模块回滚
性能约束: 不引入额外常驻高频轮询
实现边界: 优先修改现有文件，不额外扩展无必要抽象层
```

### 验收标准
- [ ] 聊天发送/接收仅使用一套 DTO（`networkplugin/Chat/ChatMessage.cs`），`ChatUI.cs` 不再定义同名模型。
- [ ] `CampfireUpgradeSelected` / `CampfireRemoveCard` 在 Host 和 Joiner 间可收发并落地，日志不再出现“已定义但无处理”的告警。
- [ ] 敌人生成链路只保留一种权威消息入口，`BattleEnemySpawned` 不再作为并行主流程。
- [ ] `NetWorkPlayer` 重复定义被清理，`NotImplementedException` 历史占位不再留在主路径。
- [ ] Client/Server/Relay 的 `IsGameEvent` 判定来源一致，关键消息不再依赖字面量分支。
- [ ] NAT 日志可准确表达“STUN可用 / UPnP禁用或未启用”的真实能力。
- [ ] 配置层 `#if false` 参考块清理完成，保留可执行配置定义。
- [ ] 幽灵方法治理完成：高置信“仅定义无调用”方法与字段被清理或明确标注保留原因；`RemoteCardUsePatch` ReversePatch stub 不被误删。

---

## 2. 方案总览

### 2.1 分期执行

#### Phase A (P0): 协议与运行时风险快速收敛
- 聊天 DTO 统一 + ChatUI 淡出逻辑修复。
- Campfire 同步补丁恢复为可执行状态。
- 消息路由关键分支（Client/Server/Relay）先做一致性对齐。

#### Phase B (P1): 结构技术债清理
- 敌人生成双路径合并。
- 玩家模型重复定义清理。
- 消息名字面量去除与集中化治理。
- 幽灵方法与遗留字段清理（含定义即终点方法、无调用私有工具函数）。

#### Phase C (P2): 能力边界与可维护性收口
- NAT/UPnP 能力边界收口（短期禁用策略清晰化）。
- 配置遗留块清理。
- 文档与路线图同步更新。

### 2.2 变更范围
```yaml
涉及模块:
  - Chat
  - Patch/Network
  - Network/Messages
  - Network/Client
  - Network/Server
  - Network/NetworkPlayer
  - Network/Utils
  - Configuration
  - Docs(PLANNING_ROADMAP/helloagents)
预计变更文件: 16-24
预计新增文件: 0-2 (仅在必须拆分消息路由规则时新增)
```

### 2.3 风险评估

| 风险 | 等级 | 影响 | 缓解策略 |
|------|------|------|----------|
| 聊天字段统一后旧 payload 兼容问题 | 中 | Joiner 收不到历史聊天 | 在边界层保留一次性字段兼容映射并加日志观测 |
| Campfire 补丁恢复后重复执行 | 中 | 选择事件重复落地 | 引入请求去重键（playerId + floor + actionId） |
| 敌人生成路径合并导致旧补丁遗漏 | 高 | SpawnId 不一致，意图同步失配 | 先落地“单入口 + 统一绑定”，再删除旧分支 |
| 清理 NetWorkPlayer 影响隐式引用 | 中 | 编译失败或运行时空引用 | 先全局替换引用，再删重复类 |
| 消息字面量清理范围过大 | 中 | 引入回归 | 仅先覆盖高频链路，分批推进 |
| 幽灵方法误删反射入口 | 中 | Harmony ReversePatch 或反射调用失效 | 对 `RemoteCardUsePatch` 反向补丁桩方法建立白名单，先证据化再删除 |

---

## 3. 文件级改动设计

### 3.1 聊天协议统一（P0）

| 文件 | 具体改动 | 预期结果 |
|------|----------|----------|
| `networkplugin/Chat/ChatMessage.cs` | 设为唯一聊天 DTO；统一字段口径（主字段 `PlayerName`），并在反序列化边界兼容旧字段读入 | 聊天模型单一、字段语义统一 |
| `networkplugin/UI/Components/ChatUI.cs` | 删除文件尾本地 `ChatMessage`/`ChatMessageType` 重定义；改为直接引用 `NetworkPlugin.Chat` 命名空间模型；修复消息淡出时间计算逻辑（不再使用 `name.GetHashCode()` 构造时间） | UI 与网络层协议一致，淡出逻辑稳定 |
| `networkplugin/Chat/ChatConsole.cs` | 发送消息时仅填充统一字段口径；移除双字段写入分支 | 发送端契约明确 |
| `networkplugin/Core/SynchronizationManager.cs` | 清理 `UserName/PlayerName/PlayerId` 的历史并行解析分支，保留最小兼容入口并统一落到 `PlayerName` | 接收链路简化，减少歧义 |
| `networkplugin/Network/Messages/NetworkMessageTypes.cs` | 复核 `ChatMessage` 常量使用点，禁止新引入聊天消息字面量 | 聊天消息名来源单一 |

### 3.2 Campfire 同步恢复（P0）

| 文件 | 具体改动 | 预期结果 |
|------|----------|----------|
| `networkplugin/Patch/Network/CampfireSyncPatch.cs` | 移除 `#if false` 禁用块，恢复可执行 Patch；实现 Upgrade/RemoveCard 的发送、接收、落地和去重 | 篝火行为可联机同步 |
| `networkplugin/Patch/Network/RoomStateSyncPatch.cs` | 在房间状态快照中补充 Campfire 相关阶段状态（仅最小必要字段）用于中途加入追赶 | Joiner 进入后状态更一致 |
| `networkplugin/Network/Client/NetworkClient.cs` | 注册 Campfire 相关消息处理器，统一走 GameEvent 通道 | 客户端不再漏处理 |
| `networkplugin/Network/Server/NetworkServer.cs` | 补齐 Campfire 消息分类与广播规则（Host 权威） | 服务端路由闭环 |
| `networkplugin/Network/Server/RelayServer.cs` | 与 `NetworkServer` 保持相同 GameEvent 分类策略 | Relay/直连行为一致 |

### 3.3 敌人生成链路收敛（P1）

| 文件 | 具体改动 | 预期结果 |
|------|----------|----------|
| `networkplugin/Patch/EnemyUnits/SpawnedEnemyManager.cs` | 作为唯一“敌人出生事件”发送入口；消息负载中包含稳定的 SpawnId 与基础定位字段 | 出生事件入口单一 |
| `networkplugin/Patch/Network/EnemySpawnSyncPatch.cs` | 作为唯一接收与落地入口；统一 SpawnId 绑定和重放顺序控制 | 接收侧一致性提高 |
| `networkplugin/Patch/Network/SpawnedEnemySyncPatch.cs` | 下线 `BattleEnemySpawned` 主流程逻辑（保留兼容桥接期后再删）；删除与主入口重复的发送分支 | 双路径冲突消失 |
| `networkplugin/Patch/Network/EnemySyncPatch.cs` | 所有敌人状态定位优先使用 SpawnId（其次才兜底 RootIndex+Id） | 状态同步匹配更稳定 |
| `networkplugin/Patch/Network/EnemyIntentReceivePatch.cs` | 与统一 SpawnId 定位逻辑对齐 | 意图同步与出生链路一致 |

### 3.4 玩家模型收敛（P1）

| 文件 | 具体改动 | 预期结果 |
|------|----------|----------|
| `networkplugin/Network/NetworkPlayer/NetWorkPlayer.cs` | 移除未实现占位方法，或将该类完全迁出主流程引用（最终以 `INetworkPlayer` 为准） | 主路径不再依赖历史实现 |
| `networkplugin/Network/NetworkPlayer/dto/NetWorkPlayer.cs` | 删除重复 DTO 定义，避免双类同名语义冲突 | 模型来源唯一 |
| `networkplugin/Plugin.cs` | 清理旧 `NetWorkPlayer` 成员和初始化路径，统一注入 `INetworkPlayer`/`LocalNetworkPlayer` | 插件入口依赖收敛 |
| `networkplugin/Network/NetworkPlayer/LocalNetworkPlayer.cs` | 校验本地玩家字段覆盖范围，补齐必要同步字段映射 | 本地玩家模型稳定 |
| `networkplugin/Network/NetworkPlayer/RemoteNetworkPlayer.cs` | 校验远端玩家更新路径与新契约一致 | 远端玩家状态一致 |

### 3.5 消息常量与路由治理（P1）

| 文件 | 具体改动 | 预期结果 |
|------|----------|----------|
| `networkplugin/Network/Messages/NetworkMessageTypes.cs` | 将现有常量按域分组并补齐注释；若保留 `MessageCategories`，则接入实际路由而非闲置 | 消息定义可维护 |
| `networkplugin/Network/Client/NetworkClient.cs` | 清理关键链路字面量（聊天、敌人生成、Campfire、回合事件）改为常量引用 | 客户端路由一致 |
| `networkplugin/Network/Server/NetworkServer.cs` | 同步移除字面量分支，`IsGameEvent` 规则与 Client/Relay 同源 | Host 路由一致 |
| `networkplugin/Network/Server/RelayServer.cs` | 使用同一套路由判定来源，避免 relay 特例漂移 | Relay 路由一致 |
| `networkplugin/Patch/Network/*.cs` | 扫描并替换高频消息字面量，确保补丁层不再扩散字符串常量 | 消息治理闭环 |

### 3.6 NAT 与配置收口（P2）

| 文件 | 具体改动 | 预期结果 |
|------|----------|----------|
| `networkplugin/Network/Utils/NatTraversal.cs` | 明确区分 STUN 探测与 UPnP 能力；UPnP 未实现路径返回统一状态和日志，不再误导为“可用但失败” | NAT 能力边界清晰 |
| `networkplugin/Configuration/ConfigManager.Sync.cs` | 配置项与运行时能力对齐（禁用或显式实验开关二选一） | 配置语义一致 |
| `networkplugin/UI/Components/NetworkStatusIndicator.cs` | 在状态文本或系统日志中补充 NAT 关键状态（已连接/穿透类型/UPnP状态） | 问题定位效率提高 |
| `networkplugin/PLANNING_ROADMAP.md` | 将 NAT 与消息治理进展回写路线图，移除过期状态描述 | 文档与代码一致 |

### 3.7 幽灵方法治理（P1）

| 文件 | 具体改动 | 预期结果 |
|------|----------|----------|
| `networkplugin/Plugin.cs` | 删除仅定义未使用字段 `netWorkPlayer`；校验相关 using/注释同步收敛 | 插件入口不再携带无效状态字段 |
| `networkplugin/Network/NetworkPlayer/NetWorkPlayer.cs` | 清理仅定义无调用的历史占位方法（`SendData`、`IsLobbyOwner`、`IsPlayerInSameRoom`、`IsPlayerOnSameAct`）；保留需要的纯数据字段或整体迁出主流程 | 历史模型不再携带误导性未实现能力 |
| `networkplugin/Network/NetworkPlayer/dto/NetWorkPlayer.cs` | 该类当前无有效引用，作为重复 DTO 删除（删除前二次确认序列化入口） | 双类同名冲突与冗余维护成本消失 |
| `networkplugin/Network/NetworkPlayer/INetworkPlayer.cs` | 评估并下线 `GetMyself` 契约（当前仅接口+实现，无业务调用） | 接口职责收敛，去除非必要全局访问入口 |
| `networkplugin/Network/NetworkPlayer/LocalNetworkPlayer.cs` | 同步删除/收敛 `GetMyself` 与无调用空实现 `UpdateStance`（若接口调整） | 本地玩家实现与真实调用面一致 |
| `networkplugin/Network/NetworkPlayer/RemoteNetworkPlayer.cs` | 同步删除/收敛 `GetMyself` 与无调用空实现 `UpdateStance`（若接口调整） | 远端玩家实现与真实调用面一致 |
| `networkplugin/Network/Client/NetworkManager.cs` | 处理无调用入口 `UpdatePlayerInfo(object playerInfo)`：要么接入明确事件调用，要么删除并收敛相关解析逻辑 | 客户端管理器消除“预留但永不触发”的死路径 |
| `networkplugin/Patch/UI/ShopTradeIconPatch.cs` | 删除无调用私有方法 `ApplyCompactButtonStyle_NoThrow`、`ForceButtonLabelCenter`、`TryGetShopPanelRoot` | UI 补丁文件体积与噪音降低 |
| `networkplugin/Patch/Network/RemoteCardUsePatch.cs` | 明确 `Card_GetActions_Original` 为 ReversePatch stub（非幽灵方法），仅补充注释/白名单说明，不参与删除 | 防止误删反射/反向补丁入口 |

---

## 4. 验证计划

### 4.1 构建与静态检查
- `dotnet build networkplugin/NetWorkPlugin.csproj`
- 全局扫描：
  - 不再出现 `#if false` 包裹的 Campfire 主逻辑。
  - 不再出现并行主流程 `BattleEnemySpawned` 发送入口。
  - 不再出现 `ChatUI.cs` 内本地 `ChatMessage` 类型定义。
  - `Plugin.cs` 不再存在无调用字段 `netWorkPlayer`。
  - `ShopTradeIconPatch.cs` 不再存在上述 3 个无调用私有工具方法。
  - 目标幽灵方法经符号引用检查后不再出现“仅定义”状态（白名单除外）。

### 4.2 手工联机场景验收
1. Host + Joiner 进入房间后双向聊天（含系统消息、普通消息）。
2. Campfire 升级与移除卡牌在双方状态一致。
3. 战斗开始后敌人生成、意图更新、状态更新连续一致。
4. 断线后重连：聊天/战斗关键状态不出现明显错位。

### 4.3 回归关注点
- EndTurn 链路（避免再次误判为系统消息）。
- MidGameJoin 最小追赶链路（不引入新阻塞）。
- TradePanel、RoomState 既有功能不受影响。

### 4.4 幽灵方法治理专项验收
1. 对 `GetMyself`、`UpdateStance`、`UpdatePlayerInfo(object)` 等裁剪点执行符号引用复核，确认不存在残留调用断链。
2. 对 `RemoteCardUsePatch` 反向补丁桩执行联机战斗回归，确认未发生 Harmony 反向补丁失效。
3. 对删除的私有 UI 工具函数执行商店页布局回归，确认交易按钮布局与点击路径不退化。

---

## 5. 任务拆分引用

详细实施步骤见：
- `helloagents/plan/202603051930_networkplugin-review-optimization-closure/tasks.md`

---

## 6. 技术决策记录

### networkplugin-review-optimization-closure#D001: 聊天 DTO 采用“单模型 + 边界兼容读取”
**状态**: ✅采纳
**理由**: 立即消除双模型维护成本，同时避免历史房间 payload 直接失效。

### networkplugin-review-optimization-closure#D002: 敌人生成以 `EnemySpawned` 为唯一主路径
**状态**: ✅采纳
**理由**: 当前补丁生态中 `EnemySpawned` 已与 SpawnedEnemyManager 更紧密，迁移成本更低。

### networkplugin-review-optimization-closure#D003: NAT 策略短期收口为“STUN为主、UPnP显式未启用”
**状态**: ✅采纳
**理由**: 当前 UPnP 占位逻辑未形成可验证闭环，先避免误导行为，再规划独立实现。
