# how

## 总体思路
把 TODO 的“应用主机存档状态”拆成三层（按你的目标：真实改写局内进度）：
1) **可靠收发层**：确保 SaveSyncRequest/Response/QuickSaveSync 能作为网络事件被收发与定向路由。
2) **存档恢复层（决定性）**：客户端通过游戏原生入口 `GameMaster.RestoreGameRun(GameRunSaveData)` 重建地图节点/站点/房间等“进度”。
3) **运行时补偿层**：用 `FullStateSnapshot`（以及可选 MissedEvents）对齐玩家显示态与战斗态（怪物 HP、状态、意图等）。

备注：仅靠 `FullStateSnapshot` 不足以把游戏推进到对应地图/站点；必须借助 `GameRunSaveData` 的恢复流程。

## 协议与数据契约

### 事件类型
统一使用 `NetworkMessageTypes` 常量（避免硬编码字符串）：
- `NetworkMessageTypes.SaveSyncRequest`
- `NetworkMessageTypes.SaveSyncResponse`
- `NetworkMessageTypes.QuickSaveSync`
- 已存在：`NetworkMessageTypes.OnGameSave`、`NetworkMessageTypes.OnGameLoad`

### SaveSyncRequest Payload（建议，LAN 简化，不做防刷）
字段（平铺，便于现有 JSON 反序列化逻辑）：
- `Timestamp` (long)
- `RequesterId` (string)  // 客户端 PlayerId
- `SaveSlot` (int)        // 存档槽位编号（你已确认）
- `RequestType` (string)  // "FullSync" | "DiffOnly" 等
- `LastKnownEventIndex` (long, optional)  // 若可提供，用于后续追赶
- `RoomId` (string, optional) // Relay 模式可附带

### SaveSyncResponse Payload（建议，满足“真实改写进度”）
- `Timestamp` (long)
- `RequesterId` (string)
- `HostId` (string)
- `SaveSlot` (int)
- `SyncType` (string)
- `FullSnapshot` (FullStateSnapshot)          // 你已确认使用
- `GameRunSaveDataBytesBase64` (string)       // 关键：用于客户端调用 RestoreGameRun 重建进度
- `MissedEvents` (List<GameEvent>, optional)  // 可选：复用 MidGameJoin 回放逻辑
- `ErrorMessage` (string, optional)

编码说明：
- 主机侧使用 `SaveDataHelper.SerializeGameRun(GameRunSaveData, compress: true)` 得到 byte[]。
- JSON 传输将 byte[] 编码为 Base64 字符串。
- 客户端侧 Base64 解码后 `SaveDataHelper.DeserializeGameRun(bytes)` 得到 `GameRunSaveData`。
- 客户端调用 `GameMaster.RestoreGameRun(saveData)` 进入/重建。

## 可靠收发层设计（关键）

### 1) 客户端 IsGameEvent 白名单
`NetworkClient.IsGameEvent(...)` 需要把以下类型纳入 GameEvent 处理：
- `NetworkMessageTypes.SaveSyncRequest`
- `NetworkMessageTypes.SaveSyncResponse`
- `NetworkMessageTypes.QuickSaveSync`
（否则会被当作 Unknown message type 丢弃）

### 2) 直连/Host 模式：NetworkServer 路由
`NetworkServer.IsGameEvent(...)` 同样需要纳入以上类型。
并在 `HandleGameEvent(...)` 中增加路由策略：
- `SaveSyncRequest`：**定向转发给房主**（类似 FullStateSyncRequest）。
- `SaveSyncResponse`：**仅单播给请求方**（类似 FullStateSyncResponse）。
- `QuickSaveSync`：一般可广播（可配置/节流）。

### 3) Relay 模式：RelayServer 识别与路由
`RelayServer.IsGameEvent(...)` 目前只包含 `StateSyncRequest` 等少数类型。
需要补齐：
- `SaveSyncRequest`/`SaveSyncResponse`/`QuickSaveSync`
并在 switch/case 中实现类似 FullStateSync 的房间路由：
- Request -> host
- Response -> requester/target

## 落地应用层设计（实现 TODO）

### A) 存档恢复层（地图/节点/站点/房间一致的核心）
目标：把客户端“进度”改写到与主机一致。

流程：
1. 客户端收到 SaveSyncResponse，解析 `GameRunSaveDataBytesBase64`。
2. 解码并 `SaveDataHelper.DeserializeGameRun(bytes)` 得到 `GameRunSaveData`。
3. 若 `GameMaster.CurrentGameRun == null`：直接 `GameMaster.RestoreGameRun(saveData)`。
4. 若 `GameMaster.CurrentGameRun != null`：
   - 由于原生 `RestoreGameRun` 会报 `AlreadyInGameRun`，需要先走一次“退出当前局内”流程，再恢复。
   - 可选策略：调用 `GameMaster.RequestAbandonGameRun(skipResult: true)` 或等价的退出路径（以实现阶段验证为准）。
   - 退出完成后再 `RestoreGameRun(saveData)`。

约束：
- `GameMaster.CoRestoreGameRun` 只支持特定 Timing（EnterMapNode/BattleFinish/AfterBossReward/Adventure）。
  这会决定“任何时候都能请求”的实际效果：
  - 请求当然能发；但如果主机当前并没有一个可恢复的 Timing，客户端的恢复可能需要降级为“下一次保存点再应用”。
  （实现阶段会在主机侧选择最接近的 SaveTiming，或在响应中返回 ErrorMessage。）

### B) 运行时补偿层（怪物HP/战斗态/玩家态）
目标：在恢复后（或不恢复时），尽力把战斗内的敌人与玩家状态对齐。

1. 玩家态：
- `FullSnapshot.PlayerStates` -> 更新 `RemoteNetworkPlayer` 字段（显示态与轻逻辑）。

2. 敌人态：
- `FullSnapshot.BattleState.Enemies` -> 在客户端当前 `BattleController` 中找到对应 `EnemyUnit`。
- 对齐键优先级建议：
  - 优先使用现有 `SpawnedEnemySyncPatch` 的 SpawnId（如果可从 snapshot 获取/扩展）。
  - 其次用 EnemyId + Index 组合（风险：同类多只时可能错位）。
- 应用内容：HP/MaxHP/Block/Shield/StatusEffects/Intention。

3. 追赶事件（可选）：
- 若响应带 `MissedEvents`：通过 `NetworkClient.InjectLocalGameEvent` 按 MidGameJoin 的过滤策略回放。

### 核心映射策略（建议）
- PlayerId 统一使用 `NetworkIdentityTracker` 分配的 playerId（而不是 userName），避免同名冲突。
- `FullStateSnapshot.PlayerStates[*].PlayerId` 应与 `INetworkPlayer.playerId` 对齐。
- RemoteNetworkPlayer 支持字段赋值（HP/maxHP/block/shield/coins/mana/location_X/location_Y/chara/endturn/mood/exhibits），适合作为“显示态/轻同步态”载体。

## 并发、幂等、节流
- LAN 环境下不做防刷；但客户端应用仍以 `Timestamp` 做幂等：只应用“更晚”的响应。
- 仍坚持 Request/Response 只定向单播，避免广播放大。

## 观测与日志
- 每次请求/响应输出关键字段：RequesterId/HostId/SaveSlot/Timestamp。
- 应用阶段输出：
  - 应用了多少 PlayerStates
  - 是否回放了 MissedEvents
  - 降级原因（例如缺少 FullSnapshot / 无法找到目标玩家 / JSON 解析失败）

## 与现有模块的关系
- FullStateSnapshot：
  - 创建：`ReconnectionManager.CreateFullSnapshot()` 已可用。
  - 回放：`NetworkClient.InjectLocalGameEvent` + MidGameJoin 的过滤策略可复用。
- SaveLoadSyncPatch：
  - 承担“发起请求/响应 + 解析响应 + 触发恢复/补偿”的补丁职责。
  - TODO 主要落在：响应解析、调用 `RestoreGameRun`、以及敌人/玩家应用器。
