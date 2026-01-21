# why

## 背景
`networkplugin/Patch/Network/SaveLoadSyncPatch.cs` 目前已实现：
- 主机保存/加载时广播诊断性快照（OnGameSave/OnGameLoad）。
- 客户端加载后向主机发起一次 SaveSyncRequest。
- 主机可发送 SaveSyncResponse（当前仅包含最小快照）。

但 TODO 仍未实现：
- `SaveSyncManager.ApplyHostSaveSync(...)` 仅记录日志，没有把主机侧“存档/局内状态”应用到客户端。

## 需求更新（你已明确的6点）
本方案以你的目标为准：
1) 客户端局内进度要被真正改写（地图节点/房间状态/怪物信息尽量一致）。
2) 场景优先：虚拟局域网联机（LAN）。
3) 同步数据基于 `FullStateSnapshot`。
4) 玩家侧：更新 `RemoteNetworkPlayer`；怪物侧：需要能改 `EnemyUnit` 的 HP 等战斗态。
5) SaveSlot 使用“槽位编号”。
6) 可随时请求，不做防刷（后续如需要再加）。

## 问题与根因
1. 事件路由缺口（必须修）
- `SaveSyncRequest`/`SaveSyncResponse`/`QuickSaveSync` 不以 `On` 开头，现状下可能不会被当作 GameEvent 处理，导致请求/响应被丢弃。

2. “真实改写局内进度”需要用到游戏原生的恢复流程
- `FullStateSnapshot` 本身只是“状态描述”，并不能自动让游戏进入对应地图节点/站点/战斗。
- 游戏侧确实存在可用的恢复入口：`LBoL.Presentation.GameMaster.RestoreGameRun(GameRunSaveData saveData)`。
- 但该入口有硬约束：当 `GameMaster.CurrentGameRun != null` 时会直接报错 `AlreadyInGameRun`。
  这意味着“随时请求并立即强制改写”在实现上通常需要先退出当前局内，再恢复。

3. 战斗态（怪物HP/状态/意图）与地图态分属不同层
- 地图/站点/进入节点等可通过 `GameRunSaveData` + `RestoreGameRun` 重建。
- 战斗中的敌人 HP 等更偏“运行时战斗态”，更适合通过同步补丁直接修改 `EnemyUnit`；
  但前提是能稳定对齐敌人身份（建议复用现有的 SpawnId 方案）。

## 目标（成功标准）
- 客户端任意时刻发起 SaveSyncRequest（携带 SaveSlot 编号）。
- 主机收到后返回 SaveSyncResponse，至少包含：
  - `FullStateSnapshot`（用于玩家/战斗态对齐）。
  - 以及可用于“真实恢复局内进度”的 `GameRunSaveData`（或等价序列化数据）。
- 客户端收到后：
  - 若不在局内：直接通过原生 `RestoreGameRun` 进入与主机一致的进度。
  - 若已在局内：执行一次“退出当前局内 -> RestoreGameRun”的强制重建路径，并尽力把战斗态/怪物态对齐。

## 约束与原则
- 网络层优先复用 `networkplugin/Network` 既有管线（FullStateSync/快照/InjectLocalGameEvent）。
- LAN 环境下不做防刷，但仍保持：Request/Response 只定向单播，避免全房间广播造成状态风暴。
- 不追求 100% 完美，但要做到“可观察、可回滚、不崩溃”。
