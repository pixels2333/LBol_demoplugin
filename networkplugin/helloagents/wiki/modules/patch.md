# Patch 模块

目录：`Patch/`

## 职责
- 通过 Harmony Patch 截获关键游戏流程
- 将本地行为转为同步事件并调用 `ISynchronizationManager` / `INetworkClient`
- 接收远端事件后，应用到本地游戏状态（按具体 Patch/Manager 实现）

## 组织方式
- `Patch/Actions/`：动作/卡牌相关
- `Patch/Network/`：网络同步点（回合、地图、敌人、状态等）
- `Patch/UI/`：联机 UI 与交互行为补丁（退出限制、目标选择等）
- `Patch/Map/`、`Patch/EnemyUnits/` 等：按功能拆分

## Patch 编写约定
- 入口：通过 `ModService.ServiceProvider` 获取服务（`INetworkClient` / `ISynchronizationManager` 等）。
- 单机不影响：未联机或取不到服务时必须“继续原逻辑”（Prefix 返回 `true` 或 Postfix 不改动）。
- 联机保护：联机相关逻辑必须先检查 `INetworkClient.IsConnected`。
- 序列化：网络负载尽量是可 JSON 序列化的匿名对象/DTO；避免直接传 Unity 引用。

## 关键补丁（节选）
- `Patch/UI/ExitGamePatch.cs`：联机状态下限制危险入口、引导安全退出流程（参照 TiS 的退出类补丁思路）。
- `Patch/Network/EndTurnSyncPatch.cs`：回合流转与输入锁定相关同步。
- `Patch/Actions/PlayCardAction_Patch.cs`：卡牌使用生命周期同步。
- `Patch/Network/UnlockEverythingForMPPatch.cs`：联机解锁统一（参照 TiS 的 “treatEverythingAsUnlocked” 思路）。

### UnlockEverythingForMPPatch
- 文件：`Patch/Network/UnlockEverythingForMPPatch.cs`
- 目的：联机时统一“解锁相关判定”，避免各玩家本地档案进度不同导致的内容差异。
- 行为（仅在 `INetworkClient.IsConnected == true` 时生效）：
  - `GameMaster.CurrentProfileLevel` 视为 `ExpHelper.MaxLevel`（UI/博物馆/谜题/角色解锁判定等）
  - `GameRunStartupParameters.UnlockLevel` 视为 `ExpHelper.MaxLevel`（开局卡牌/展品池等生成逻辑）

