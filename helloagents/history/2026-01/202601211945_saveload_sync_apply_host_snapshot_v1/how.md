# how

## 总体思路
把 TODO 的“应用主机同步数据”按你最新规则拆成三件事：
1) **房间标识层（RoomKey）**：让主机与所有客户端能一致地指向“同一个房间/节点”。
2) **房间状态中枢层（主机缓存与分发）**：谁先进入房间（或先开战）就把房间信息上传到主机；主机把最新房间状态同步给其他人。
3) **客户端应用层（生成 + 奖励/残局应用）**：其他人进入该房间时，按主机提供的怪物清单生成对应怪物，并把血量/格挡/护盾/状态/意图等改到一致；若战斗已结束则直接应用“已清房”并领取奖励。

## 协议与数据契约

### 事件类型
统一使用 `NetworkMessageTypes` 常量（避免硬编码字符串）。

建议新增/复用的事件类型（名称可在实现时对齐项目已有风格）：
- `RoomStateRequest`：玩家即将进入/已进入某房间时，向主机请求该房间的最新状态。
- `RoomStateResponse`：主机把该房间的状态回给请求方（单播）。
- `RoomStateUpload`：先进入的人把该房间“当前权威状态”上传给主机（单播到主机）。
- `RoomStateBroadcast`（可选）：主机把更新推给所有人（广播或按订阅推送）。

仍可保留：
- `SaveSyncRequest/SaveSyncResponse`（如果你还想保留“按存档槽编号请求”的入口），但它的语义应当转为“请求房间/节点快照”，而不是恢复整局存档。

### RoomStateRequest Payload（建议，LAN 简化，不做防刷）
字段（平铺，便于现有 JSON 反序列化逻辑）：
- `Timestamp` (long)
- `RequesterId` (string)  // 客户端 PlayerId
- `RoomKey` (string)      // 房间唯一标识（关键）
- `SaveSlot` (int, optional) // 若仍挂在存档槽入口下：槽位编号从 1 开始
- `RequestType` (string)  // "RoomState" | "RoomOutcome" 等
- `RoomSeed` (long, optional) // 若该房间未同步过，可用于本地生成一致怪物
- `RoomVersion` (long, optional) // 乐观并发控制：请求方已知的版本号

### RoomStateResponse Payload（建议，满足“残局应用/已清房领奖励”）
- `Timestamp` (long)
- `RequesterId` (string)
- `HostId` (string)
- `RoomKey` (string)
- `RoomVersion` (long) // 主机侧递增版本
- `RoomPhase` (string) // "NotVisited" | "InBattle" | "BattleFinished" | "RewardClaimed" 等
- `RoomSeed` (long, optional)
- `Enemies` (list)
  - 每个敌人至少需要：
    - `EnemyId`/`EnemyName`（用于生成）
    - `Index`（同类多只时稳定排序）
    - `Hp`/`MaxHp`/`Block`/`Shield`
    - `StatusEffects`（可简化：只同步关键状态）
    - `Intention`（可选，尽力）
- `RoomOutcome` (object, optional)
  - 用于战斗结束后“直接拿奖励”的信息（例如奖励类型、数量、已领取标记）。
- `ErrorMessage` (string, optional)

重点：
- 本方案不再依赖 `RestoreGameRun(GameRunSaveData)` 来“重建整局进度”。
- 进入房间时，客户端按 `Enemies` 清单生成对应怪物，再把状态调到一致。

## 可靠收发层设计（关键）

### 1) 客户端 IsGameEvent 白名单
`NetworkClient.IsGameEvent(...)` 需要把“房间同步”相关消息纳入 GameEvent 处理，否则会被丢弃。
至少包括：
- `RoomStateRequest/RoomStateResponse/RoomStateUpload`（以及可选 `RoomStateBroadcast`）
若继续沿用 `SaveSync*` 名称，也需要纳入。

### 2) 直连/Host 模式：NetworkServer 路由
`NetworkServer.IsGameEvent(...)` 同样需要纳入以上类型。
并在 `HandleGameEvent(...)` 中实现路由策略（核心就是“主机是中枢”）：
- `RoomStateRequest`：定向转发给房主/主机逻辑处理后单播回应。
- `RoomStateUpload`：只允许上传到主机，由主机更新缓存版本后再响应/广播。
- `RoomStateResponse`：仅单播给请求方。

### 3) Relay 模式：RelayServer 识别与路由
Relay 也需要识别并转发上述房间同步事件：
- Request -> host
- Upload -> host
- Response -> requester/target
确保直连与 Relay 行为一致。

## 落地应用层设计（实现 TODO）

### A) 房间状态缓存（主机侧）
目标：主机作为中枢，缓存每个 RoomKey 的“最新权威状态”。

主机侧维护：
- `Dictionary<RoomKey, RoomStateSnapshot>`（含 RoomVersion）

更新来源：
- `RoomStateUpload`：由先进入的人上传。
- 主机可做校验：同一 RoomKey 只接受更高版本/更新的 Timestamp。

对外服务：
- `RoomStateRequest`：主机查缓存 -> 回 `RoomStateResponse`。
- 可选：当主机缓存更新时，广播 `RoomStateBroadcast` 给其他人（减少请求等待）。

### B) 房间进入时请求与应用（客户端侧）
目标：进入房间时，能“应用残局”或“直接领奖励”。

流程（建议）：
1. 客户端检测到即将进入/已进入房间（实现阶段从 lbol/ 找到合适的 Hook 点）。
2. 发送 `RoomStateRequest(RoomKey, RoomVersion)` 给主机。
3. 收到 `RoomStateResponse`：
   - `RoomPhase == NotVisited`：按 `RoomSeed`/同步 RNG 规则本地生成。
   - `RoomPhase == InBattle`：按 `Enemies` 清单生成对应怪物，并把状态改到一致（残局）。
   - `RoomPhase == BattleFinished`：直接应用“战斗结束”并发放/展示奖励（不再开战）。

说明：你要求“看起来像同一只怪”，本方案按“种类+序号+状态”重建，不依赖唯一 SpawnId。

### C) 战斗过程上传（先进入者 -> 主机）
目标：先进入的人持续把战斗状态上报，保证后来者进来能接上残局。

建议上报时机（实现时可按性能调整）：
- 战斗开始：上传一次完整初始敌人列表。
- 每回合开始/结束：上传一次（推荐），包含敌人状态与 RoomPhase。
- 战斗结束：上传一次 RoomOutcome（奖励信息）并标记 RoomPhase。

### 核心映射策略（建议）
- PlayerId 统一使用 `NetworkIdentityTracker` 分配的 playerId。
- RoomKey 必须稳定且可复现：需要从 lbol/ 里找到“地图节点唯一信息”的正确口径（例如层数+节点索引+路径）。
- 敌人对齐采用“EnemyId/类型 + Index + 状态”，避免依赖唯一 SpawnId。

## 并发、幂等、节流
- LAN 环境下不做防刷；但主机缓存与客户端应用仍建议按 `RoomVersion/Timestamp` 做幂等，只接受更“新”的状态。
- Request/Response 尽量单播；Broadcast 可选，避免状态风暴。

## 观测与日志
建议输出关键字段：
- RoomKey、RoomPhase、RoomVersion、上传者/请求者/主机 Id
- 应用阶段：生成了多少敌人、是否应用了残局、是否直接发放奖励
- 降级原因：无缓存/解析失败/RoomKey 不一致/敌人生成失败等

## 与现有模块的关系
本方案将主要复用：
- 现有网络事件收发与路由（直连与 Relay）。
- 现有玩家身份与房主判断（`NetworkIdentityTracker` 等）。

本方案将主要新增：
- RoomKey 的提取逻辑（来自 lbol/ 的地图/房间信息）。
- 房间状态快照的数据结构与主机缓存。
- 客户端“房间进入时请求 + 残局/奖励应用器”。
