# 最小可运行验证: inrun-map-progress-sync

本文件用于补齐 `helloagents/plan/202601251728_inrun-map-progress-sync/tasks.md` 中的“测试与手工验收脚本”要求。

范围：仅覆盖本方案包的核心闭环（中途加入/重连/追赶/战斗意图 UI），不追求自动化单元测试。

## 0. 前置约束（必须满足）

- 联机不再传输 `GameRunSaveData` 存档 bytes（已废弃 SaveLoadSyncPatch 思路）。
- 主机权威：`FullStateSnapshot`（seeds + MapState + checkpoint）用于追赶。
- 战斗节点追赶：RoomSync 负责敌人血量/格挡等，EnemyIntent* 负责意图 UI。

## 1. 构建与部署（本地）

1) 构建 `networkplugin/NetWorkPlugin.csproj`
   - 允许存在历史 warning，但不允许出现 error。

2) 将 DLL 复制到游戏目录（已有脚本）
   - 脚本：`copy_networkplugin_dll.ps1`
   - 默认拷贝 Debug 输出；如你使用 Release，可用参数覆盖 SourceDll。

## 2. 手工验收场景

### 2.1 中途加入（Joiner 从主菜单加入一局进行中的 run）

步骤（建议 2 个客户端窗口/两台机器）：

1) Host 开启联机并进入 run，推进到地图中部（至少跨过 1-2 个节点）。
2) Joiner 连接后触发 mid-game join（`MidGameJoinManager` 发起 FullStateSyncRequest）。
3) Joiner 打开地图面板，观察追赶过程。

期望结果：

- Joiner 地图节点状态与 Host 基本一致（已访问/已清除/当前位置）。
- 追赶过程不卡死：MapCatchUpOrchestrator 以“分帧预算”推进。
- 若 PathHistory 包含已清战斗节点：Joiner 能弹出奖励/结算面板并可关闭后继续追赶。

关键日志（只要能看到其中一部分即可）：

- `[Reconnect] Catch-up completed.`（重连路径）
- `MapCatchUpOrchestrator` 相关推进日志（若当前版本有输出）

### 2.2 正在战斗节点加入（意图 UI）

步骤：

1) Host 进入战斗回合（敌人已展示 intent）。
2) Joiner 在战斗进行中加入/重连并追赶到战斗节点。

期望结果：

- Joiner 能看到敌人意图 UI（图标与伤害数字会随 Host 刷新）。
- Host 侧只负责广播，不会因为 Joiner 应用 intent 而引发回环刷屏。

关键日志：

- Host：`[EnemyIntentSync] Intentions updated...`（以及 join/reconnect 时的 broadcast）
- Joiner：`[EnemyIntentRecv] ...`（若当前版本开启了该日志）

### 2.3 回主菜单重连继续（本地 Restore + FullSnapshot 追赶）

步骤：

1) Joiner 在 run 进行中断线回主菜单。
2) Joiner 使用“重连继续”入口：先本地 `RestoreGameRun`，随后向 Host 拉 `FullStateSnapshot` 并追赶。

期望结果：

- Joiner 能进入局内（本地存档恢复成功）。
- 随后地图进度被追赶对齐（以 Host 为准）。

关键日志：

- `[MainMenuMultiplayerEntry] 联机已连接，开始本地恢复存档。`
- `[Reconnect] Catch-up completed.` 或失败提示弹窗。

## 3. 负向验收（确保没有回到旧方案）

- 代码库不存在 `networkplugin/Patch/Network/SaveLoadSyncPatch.cs`。
- `EnableSaveLoadSync` 即使被手动打开，也会在启动日志中提示 Deprecated 并被强制关闭。
- NetworkMessageTypes 不再包含 `OnHostSaveTransferStart/Chunk/End`。
