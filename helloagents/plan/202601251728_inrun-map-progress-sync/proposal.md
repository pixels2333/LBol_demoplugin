# 变更提案: inrun-map-progress-sync

## 元信息
```yaml
类型: 重构/新功能
方案类型: implementation
优先级: P0
状态: 草稿
创建: 2026-01-25
```

---

## 1. 需求

### 背景
当前中途加入/重连流程仍包含“主机发送 GameRunSaveData 存档分片（HostSaveTransfer）→ 客户端落盘并可能 RestoreGameRun”的链路。

用户已明确：本项目的“存档同步”不是玩家档/元进度，而是“一局游戏内”的地图进度同步。

因此需要将“传存档文件/恢复 GameRun”的方案删除或改造成“只同步地图种子 + 地图进度/房间状态”，并支持：
- 中途加入
- 断线重连（包含回到主菜单后再重连）

并且要求数据方向固定为：主机 → 客户端（客户端仅发起请求/确认，不作为权威来源）。

参考实现（仅作行为/思路参考，不拷贝代码）：
- D:/programme/TogetherInSpire/Together-in-Spire-v6.4.20.jar_Decompiler.com/spireTogether/patches/network/RoomEntryPatch.java（清房间/跳过同步的行为提示）
- D:/programme/TogetherInSpire/Together-in-Spire-v6.4.20.jar_Decompiler.com/spireTogether/patches/ExitGamePatch.java（允许随时重连的用户提示）

### 目标
1) 删除现有“HostSaveTransfer + RestoreGameRun”相关协议与实现。
2) 将中途加入/重连的核心同步切换为：
   - 地图确定性种子（RootSeed/UISeed/StageIndex/MapSeedUlong）
   - 地图进度（节点状态、路径、当前位置、已清节点集合）
   - 房间战斗状态（进入正在战斗节点时可同步怪物血量/意图等；复用现有 RoomSync/EnemyIntentSync）
3) 中途加入者在地图界面进行“追赶/退关”：
   - 从第一关或其“离开时节点”开始
   - 逐步追赶到队伍当前进度
4) 奖励/选择交互策略按用户确认：
   - 1.A：每个玩家各自奖励
   - 2.B：奖励选择界面照常弹出，让加入者自己选
5) 断线重连按用户确认：
   - 3.B：支持回到主菜单后重连
6) 关键节点同步范围按用户确认：
   - 5.B：除战斗结束/领奖/进下一层/存档点外，还包含事件房间结果、商店购买、休息升级等“会改变地图推进或房间状态的提交点”。

### 约束条件
```yaml
时间约束: 无
性能约束: 仅在关键节点发送同步；避免每帧/高频广播
兼容性约束:
  - 不再使用 System.Text.Json（已迁移到 Newtonsoft.Json 兼容层）
  - 尽量复用现有消息路由（FullStateSync/RoomStateRequest/Response）
业务约束:
  - 主机权威：地图进度以主机为准
  - 不同步玩家档/元进度
  - 不发送 GameRunSaveData 存档文件
```

### 验收标准
- [ ] 中途加入：加入者不接收/不落盘主机存档文件，仍可通过“地图进度追赶”进入与队伍一致的地图节点。
- [ ] 断线重连（含回到主菜单）：重连后通过“地图进度追赶”回到队伍进度；不依赖主机下发存档文件。
- [ ] 进入正在战斗的节点：加入者可同步到怪物血量与意图（至少 HP/Block/Shield + Intent），并能在地图/战斗界面保持一致性。
- [ ] 同步频率：仅在关键节点发送（按 5.B 列表）；无持续刷屏。
- [ ] 协议清理：NetworkMessageTypes 中不再包含 OnHostSaveTransferStart/Chunk/End；MidGameJoinManager 中不存在相关 handler/发送逻辑。

---

## 2. 方案

### 技术方案
本方案将“中途加入/重连”的同步分为三层：

1) 连接与授权层（保持现有 JoinToken/ApprovedJoin）：
   - 加入者发起 RequestJoin → 主机批准（JoinToken）
   - 加入者在 JoinToken 有效期内请求 FullStateSync

2) 状态快照层（主机 → 客户端）：
   - FullStateSnapshot 仍作为“握手快照”容器，但重点强化其中：
     - GameStateSnapshot: RootSeed/UISeed/StageIndex
     - MapStateSnapshot: MapSeedUlong + 节点进度
   - 去除/不再使用 hostSaveBytes（GameRunSaveData bytes）

3) 追赶执行层（客户端本地执行）：
   - 客户端基于 FullStateSnapshot.MapState 的权威进度，在地图界面执行追赶：
     - 若客户端已有本地 GameRun（重连场景）：以本地为基础，校准到主机进度
     - 若客户端无本地 GameRun（纯中途加入）：创建新 GameRun（使用主机下发的种子），再追赶
   - 对于“已清节点”：跳过战斗过程，直接进入奖励/结算流程，并按 2.B 弹 UI 让加入者自行选择
   - 对于“正在战斗节点”：
     - 进入节点时发送 RoomStateRequest
     - 由主机单播 RoomStateResponse
     - 客户端应用怪物状态并接入 EnemyIntentSync

关键点：
- 主机仅同步“地图进度/房间战斗快照”，不传客户端状态；加入者的卡牌/展品选择完全由其本地决定。
- 为保证追赶可执行，需要在本项目中补齐“节点状态与奖励可追赶”的本地执行入口（LBoL 的奖励通常由战斗结束后触发 UI；需要在追赶时触发同等流程）。

### 影响范围
```yaml
涉及模块:
  - networkplugin/Network/MidGameJoin: 删除 HostSaveTransfer，改造为地图进度追赶
  - networkplugin/Network/Snapshot: 扩充 MapStateSnapshot（节点状态/关键提交点）
  - networkplugin/Network/Reconnection: FullSnapshot 构建补齐 MapStateSnapshot 关键字段
  - networkplugin/Network/RoomSync + Patch/Network/RoomStateSyncPatch: 复用并补齐“追赶时进入战斗节点”
  - networkplugin/Patch/Network/SaveLoadSyncPatch: 评估删除或改造成“关键节点提交点广播”（不再做存档同步）
  - lbol/LBoL.*: 增加若干 Harmony Patch 点，用于捕获/触发关键节点提交与追赶执行（只做最小侵入）
预计变更文件: 10-20（含新增若干 Snapshot/Progress 类型与 Patch）
```

### 风险评估
| 风险 | 等级 | 应对 |
|------|------|------|
| 追赶时“跳过战斗但仍要拿奖励”在 LBoL 中缺少公开入口 | 高 | 在实现阶段先做最小可用：仅支持“已清战斗节点=标记已访问+不发奖励”，再逐步补齐奖励触发；并在 plan 中明确需要先做原版流程考古与验证 Patch 点 |
| 加入者从零开始追赶，RNG 状态可能与主机不同导致奖励候选不同 | 中 | 使用 RootSeed/UISeed/StageIndex/MapSeedUlong 对齐大种子；在追赶实现中尽量复用原版进入节点/结算流程以保持 RNG 消耗一致；必要时引入“主机记录的关键提交点摘要”以校正 |
| 事件/商店/休息等非战斗节点的“结果”对加入者公平性影响大 | 中 | 按 5.B 将这些节点纳入提交点；优先保证“节点已清/路径一致”，玩家收益允许个体差异；必要时增加配置开关（后续迭代） |
| 删除 HostSaveTransfer 可能影响现有 continue/host-continue 逻辑 | 中 | 在实施阶段先做编译期拆除 + 运行时回归测试；必要时保留本地保存（客户端自己保存）以支持主菜单重连 |

---

## 3. 技术设计（可选）

### 架构设计
```mermaid
flowchart TD
  A[Client Join/Reconnect] -->|RequestJoin| B[Host MidGameJoinManager]
  B -->|ApprovedJoin + JoinToken| A
  A -->|FullStateSyncRequest| B
  B -->|FullStateSyncResponse: FullStateSnapshot(MapState+Seeds)| A
  A --> C[Client CatchUpOrchestrator]
  C -->|Enter cleared nodes + open reward UI| D[LBoL Map/Reward Flow]
  C -->|Enter combat node -> RoomStateRequest| E[Host RoomSyncManager]
  E -->|RoomStateResponse| C
  C -->|Apply monster snapshot + intents| F[BattleState Sync]
```

### API设计（网络消息形态）
本方案优先复用现有 FullStateSyncRequest/Response 与 RoomStateRequest/Response。

需要新增/调整的 payload 字段：
- FullStateSnapshot.GameState: RootSeed/UISeed/StageIndex
- FullStateSnapshot.MapState: MapSeedUlong + 节点进度字段（见 tasks.md 设计任务）

可选新增（若需要明确追赶阶段）：
- OnClientCatchUpProgress（客户端→主机，仅用于 UI/日志，不做权威）
- OnHostMapCheckpoint（主机→客户端，关键节点广播/单播；取决于现有事件系统是否够用）

### 数据模型（草案）
现有 MapStateSnapshot 字段不足以表达“每个节点的完成状态/关键提交点”。建议扩充：
- NodeStates: Dictionary<string, string>（nodeKey -> state: NotVisited/Visiting/Visited/Passed/Cleared/Escaped 等）
- ClearedNodes: List<string>（快速索引用）
- LastCheckpointId/LastCheckpointTimestamp（用于重连对齐）
- PathHistory 保留作为追赶路线

---

## 4. 核心场景

### 场景: 中途加入（无本地 GameRun）
**模块**: networkplugin/Network/MidGameJoin + 客户端追赶执行层
**条件**: 客户端在主菜单或未进入本局
**行为**:
1) 客户端 RequestJoin 获取 JoinToken
2) 客户端请求 FullStateSync
3) 客户端收到 FullStateSnapshot（包含 seeds + MapState）
4) 客户端创建新 GameRun（使用 seeds）并停留在地图界面
5) 客户端按 MapState.PathHistory 追赶：
   - 已清节点：快速完成并弹出奖励选择（2.B）
   - 正在战斗节点：请求并应用 RoomState
6) 追赶完成后进入与队伍一致的节点
**结果**: 加入者地图进度与主机一致，并能继续正常游玩

### 场景: 断线重连（回到主菜单）
**模块**: networkplugin/Network/Reconnection + MidGameJoin
**条件**: 客户端已有本地存档/本地保存点
**行为**:
1) 客户端从主菜单恢复本地 GameRun（本地行为，不依赖主机存档下发）
2) 客户端发起重连并请求 FullStateSync
3) 客户端按 MapState 对齐并执行“退关/追赶”直到一致
**结果**: 不中断本地成长，重新回到队伍进度

---

## 5. 技术决策

### inrun-map-progress-sync#D001: 移除主机存档分片传输，改用地图进度追赶
**日期**: 2026-01-25
**状态**: ✅采纳
**背景**: 现有 HostSaveTransfer 违背“只同步局内地图进度”的需求，且存档格式/版本兼容风险高。
**选项分析**:
| 选项 | 优点 | 缺点 |
|------|------|------|
| A: 继续传 GameRunSaveData（旧方案） | 加入者可直接恢复完整状态 | 与需求冲突；存档兼容/安全风险大；实现复杂 |
| B: 只传 seeds + MapState + RoomState（追赶） | 符合需求；数据更小；方向清晰 | 需要补齐追赶与奖励流程；实现复杂度转移到 Patch 点 |
**决策**: 选择方案B
**理由**: 与用户需求一致，且更可控；可逐步增强追赶质量。
**影响**: MidGameJoinManager、NetworkMessageTypes、Snapshot/Reconnection、Map/Room Patch
