# 任务清单: inrun-map-progress-sync

目录: helloagents/plan/202601251728_inrun-map-progress-sync/

---

## 任务状态符号说明

| 符号 | 状态 | 说明 |
|------|------|------|
| [ ] | pending | 待执行 |
| [√] | completed | 已完成 |
| [X] | failed | 执行失败 |
| [-] | skipped | 已跳过 |
| [?] | uncertain | 待确认 |

---

## 执行状态
```yaml
总任务: 22
已完成: 8
完成率: 36%
```

---

## 任务列表

### 0. 方案落地前置校验（不改代码）

- [√] 0.1 明确“地图进度”权威定义与可观测来源
  - 目标: 明确 LBoL 中节点状态的真实来源（Visited/Passed/Visiting/Active 等），以及如何从运行时提取为 nodeKey
  - 验证:
    - 在本项目中定位并记录 nodeKey 的生成规则（当前 RoomSync 使用 Act:X:Y:StationType；MapStateSnapshot 目前使用 string 列表）
    - 输出结论到本方案的执行备注（仅备注，不新增文档）

- [√] 0.2 对照参考项目行为（只读，不拷贝）
  - 目标: 提取 TogetherInSpire 的“清房间/跳过同步/允许重连”高层行为
  - 参考文件:
    - D:/programme/TogetherInSpire/Together-in-Spire-v6.4.20.jar_Decompiler.com/spireTogether/patches/network/RoomEntryPatch.java
    - D:/programme/TogetherInSpire/Together-in-Spire-v6.4.20.jar_Decompiler.com/spireTogether/patches/ExitGamePatch.java
  - 验证: 形成 5-10 条“行为要点”列表，写入本任务清单的执行备注


### 1. 协议清理：删除 HostSaveTransfer（主机存档分片传输）

- [√] 1.1 删除网络消息常量
  - 文件: networkplugin/Network/Messages/NetworkMessageTypes.cs
  - 操作: 删除 OnHostSaveTransferStart、OnHostSaveTransferChunk、OnHostSaveTransferEnd
  - 验证:
    - 全项目无引用（编译期通过）

- [√] 1.2 删除 MidGameJoinManager 的接收处理
  - 文件: networkplugin/Network/MidGameJoin/MidGameJoinManager.cs
  - 操作:
    - 删除 switch/case 中对 OnHostSaveTransfer* 的分支
    - 删除 HandleHostSaveTransferStart/Chunk/End 方法
    - 删除 PendingJoin 中用于接收 save bytes 的字段（如存在）
  - 验证:
    - 中途加入仍能收到 FullStateSyncResponse（无 hostSaveBytes）

- [√] 1.3 删除 MidGameJoinManager 的发送处理
  - 文件: networkplugin/Network/MidGameJoin/MidGameJoinManager.cs
  - 操作:
    - 删除 TrySendHostGameRunSaveToJoiner 及其调用点
    - 删除 TryPersistAndMaybeRestoreHostGameRun
  - 验证:
    - FullStateSyncResponse payload 不再包含 HostGameRunSaveBytes
    - 加入流程不再尝试 RestoreGameRun


### 2. FullStateSnapshot 聚焦“地图进度 + 确定性种子”

- [√] 2.1 让 FullStateSnapshot 聚焦“种子 + 地图进度”
  - 文件: networkplugin/Network/Snapshot/FullStateSnapshot.cs
  - 操作:
    - 保留 GameStateSnapshot.RootSeed/UISeed/StageIndex（由 ReconnectionManager.CreateFullSnapshot 填充）
    - 保留 MapStateSnapshot.MapSeedUlong + MapProgress 字段（见 2.2）
    - 评估 BattleStateSnapshot 是否仍必要（优先用 RoomSync）
  - 结果:
    - BattleStateSnapshot 暂不移除：当前仍被 TurnAction_Patch 用于 OnBattleStart/OnBattleEnd 事件载荷；后续战斗同步以 RoomSync/EnemyIntentSync 为主可逐步降低其重要性
  - 验证:
    - build 通过；FullStateSnapshot 结构未破坏

- [√] 2.2 扩充 MapStateSnapshot（地图进度 + Checkpoint）
  - 文件: networkplugin/Network/Snapshot/MapStateSnapshot.cs
  - 新增字段:
    - NodeStates: Dictionary<string,string>（nodeKey -> state）
    - ClearedNodes: List<string>
    - LastCheckpointId: string（主机递增或时间戳型）
    - LastCheckpointAtUtcTicks: long
  - 兼容要求:
    - 保留旧字段 VisitedNodes/RevealedNodes/PathHistory
    - 新字段默认值必须可反序列化（空字典/空列表）
  - 验证:
    - build 通过；缺失字段不崩溃


### 3. 主机侧：生成权威 MapStateSnapshot（关键提交点）

- [√] 3.1 在 ReconnectionManager.CreateFullSnapshot 中补齐 MapState
  - 文件: networkplugin/Network/Reconnection/ReconnectionManager.cs
  - 操作:
    - 从当前 GameRunController/Stage/GameMap 提取 MapSeedUlong、当前位置、路径历史、已清节点状态
    - 填充 NodeStates/ClearedNodes/LastCheckpoint*
  - 验证:
    - 在游戏运行中周期快照能反映节点推进

- [√] 3.2 新增“关键提交点”记录器（主机）
  - 目标: 仅在关键节点推进时刷新 checkpoint（满足 5.B）
  - 候选实现:
    - 在现有 Map/Room Patch 中增加 Hook：
      - 战斗结束
      - 奖励确认完成
      - 进入下一层
      - 存档点
      - 事件结算完成
      - 商店购买完成
      - 休息升级/恢复完成
  - 验证:
    - checkpoint 仅在上述时机变化
    - 日志能看到 checkpointId 单调递增

  执行备注:
    - 已实现主机侧 checkpoint 记录器：`ReconnectionManager.MarkMapCheckpoint(reason, nodeKey)`（自增序号 + UTC ticks）。
    - 已接入的关键提交点 Hook（Host-only）：
      - 进入节点：`RoomEntrySyncPatch` -> `GameMap.EnterNode`（忽略 forced EnterNode 以降低噪音）
      - 战斗结束：`TurnAction_Patch`（battle_end）
      - 下一层：`MapCheckpointSyncPatch` -> `GameRunController.EnterNextStage`（next_stage）
      - 奖励面板关闭：`MapCheckpointSyncPatch` -> `RewardPanel.OnHided`（reward_closed）
      - 站点完成（非战斗）：`MapCheckpointSyncPatch` -> `Station.Finish`（station_finish）
      - 商店购买后 UI 刷新：`MapCheckpointSyncPatch` -> `ShopPanel.SetShopAfterBuying`（shop_after_buying）
      - 间隙/休息选项确认关闭：`MapCheckpointSyncPatch` -> `GapOptionsPanel.SelectedAndHide`（gap_option_selected）


### 4. 客户端侧：追赶执行器（CatchUp Orchestrator）

- [√] 4.1 设计追赶状态机
  - 状态建议:
    - Idle
    - WaitingFullSnapshot
    - PreparingLocalRun
    - CatchingUpClearedNodes
    - CatchingUpActiveCombatNode
    - Completed
    - Failed
  - 输入:
    - FullStateSnapshot（seeds + MapState）
    - 本地是否已有 GameRun（重连 vs 新加入）
  - 输出:
    - 客户端地图进度对齐（停留在 MapPanel/节点选择界面）
  - 验证:
    - 能从主菜单发起并完成追赶

  执行备注:
    - 已新增最小追赶骨架 `MapCatchUpOrchestrator`：接收 FullSnapshot 后暂存，等待本地 GameRun/地图 UI 就绪再尽力应用。
    - 当前仅实现“MapState 消费 + UI/节点状态对齐”的骨架；不负责创建新 GameRun（纯中途加入场景仍需后续任务补齐）。

- [ ] 4.2 新加入：创建本地 GameRun 并对齐 seeds
  - 目标: 客户端无本地 GameRun 时，用主机 seeds 创建
  - 需要确认的技术点:
    - LBoL 是否允许直接设置 RootSeed/UISeed/StageIndex
    - 是否必须走“新开局/选角色”流程
  - 验证:
    - 生成的地图与主机一致（MapSeedUlong 一致）

- [ ] 4.3 重连：优先恢复本地存档，再对齐 MapState
  - 目标: 支持 3.B（回主菜单后重连）
  - 操作:
    - 保留并强化“客户端本地保存点”（不通过网络传输）
    - 重连后请求 FullSnapshot，再执行退关/追赶到主机进度
  - 验证:
    - 不依赖主机存档，仍可回到队伍进度

- [ ] 4.4 追赶路径：以 MapState.PathHistory 为准
  - 规则:
    - 从起点或离开节点开始
    - 逐节点推进直到到达主机 CurrentLocation
    - 已清节点：进入后直接触发“领奖/结算流程”（2.B）
    - 当前为战斗节点：进入后走 RoomSync
  - 验证:
    - 追赶过程中 UI 不被卡死
    - 追赶结束后地图节点状态一致


### 5. 追赶时的“已清节点领奖/结算”实现（关键难点）

- [ ] 5.1 方案考古：定位 LBoL 奖励生成与领取的最小入口
  - 目标: 找到不经过真实战斗也能生成并展示 RewardPanel 的入口
  - 参考方向:
    - lbol/LBoL.Presentation/GameMaster.cs 的站点流程
    - lbol/LBoL.Presentation/UI/Panels/RewardPanel.cs 的领取逻辑
  - 验证:
    - 给出可 patch 的方法列表（带文件与方法名），写入执行备注

- [ ] 5.2 实现“追赶清节点”的结算触发
  - 目标: 加入者进入已清节点后，直接进入结算/奖励 UI
  - 规则: 2.B（让加入者自己选）
  - 验证:
    - 加入者可正常领取金币/卡牌/展品（按节点类型）

- [ ] 5.3 事件/商店/休息节点的追赶策略
  - 目标: 覆盖 5.B
  - 策略建议:
    - 事件：进入后直接进入事件结算（若无法重放则提供“跳过并标记已清”的降级）
    - 商店：允许进入但默认不强制购买；购买完成后标记 checkpoint
    - 休息：允许升级/休息；完成后标记 checkpoint
  - 验证:
    - 不会阻断追赶流程


### 6. 正在战斗节点：复用 RoomSync + EnemyIntentSync

- [ ] 6.1 明确进入战斗节点时的请求时机
  - 文件: networkplugin/Patch/Network/RoomStateSyncPatch.cs
  - 目标: 加入者进入战斗节点立即请求 RoomState
  - 验证:
    - 加入者能看到怪物 HP/Block/Shield

- [ ] 6.2 意图同步在追赶时的表现
  - 文件: networkplugin/Patch/Network/EnemyIntentSyncPatch.cs
  - 目标: 加入者进入战斗后能同步 intent
  - 验证:
    - intent 与主机一致，且不刷屏


### 7. SaveLoadSyncPatch 的处理

- [ ] 7.1 彻底删除或改造为“关键提交点广播”
  - 文件: networkplugin/Patch/Network/SaveLoadSyncPatch.cs
  - 约束:
    - 不能再做存档文件同步
    - 不能使用 System.Text.Json
  - 选项:
    - A: 删除该 Patch（若已无价值）
    - B: 改为在保存点/关键提交点时触发主机刷新 MapCheckpoint
  - 验证:
    - 不再出现 Utf8JsonWriter 相关风险路径


### 8. 网络路由与消息约束（Host -> Client）

- [ ] 8.1 明确哪些消息用单播/广播
  - 规则:
    - FullStateSyncResponse：定向（host -> target）
    - RoomStateResponse：定向（host -> requester）
    - MapCheckpoint（如新增）：可广播（host -> room）或定向给未追赶完成者
  - 验证:
    - Relay/Host 两种模式下都不泄漏 JoinToken


### 9. 测试与验证（按当前仓库条件落地）

- [ ] 9.1 新增最小测试工程（如当前无 tests）
  - 目标: 为 MapStateSnapshot/节点状态合并/序列化兼容写单测
  - 验证:
    - dotnet test 通过

- [ ] 9.2 手工验收脚本（运行时）
  - 场景:
    - 中途加入（加入时主机在地图）
    - 中途加入（加入时主机正在战斗）
    - 断线重连（同场景，含回主菜单）
  - 验证:
    - 无 HostSaveTransfer
    - 加入者最终位置与主机一致


---

## 执行备注

> 执行过程中的重要记录

| 任务 | 状态 | 备注 |
|------|------|------|
| 0.1 | completed | nodeKey 口径先对齐现有 RoomSync：Act:X:Y:StationType（见 RoomSyncManager.BuildRoomKey）；MapStateSnapshot 的 VisitedNodes/NodeStates 后续统一用同一口径存储，避免同时维护多套 key。 |
| 0.2 | completed | 参考行为要点（TogetherInSpire）：1) 进房间时重置房间参数与 action counter；2) 基于 (x,y,action) 构建确定性 roomRng；3) 发现房间已标记 cleared/escaped 时直接清房间并跳过同步；4) 无缓存则向服务器请求“是否允许生成房间”，或等待生成者数据；5) 退出确认文案：Host 退出会影响所有人，Client 可随时重连继续；6) 联机模式隐藏放弃运行按钮；7) 确认退出/放弃时重置 Mod 并返回主菜单而非彻底退出，保证可重连。 |
