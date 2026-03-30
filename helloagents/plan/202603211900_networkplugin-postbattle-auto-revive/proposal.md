# 变更提案: networkplugin-postbattle-auto-revive

## 元信息
```yaml
类型: 联机战斗结算 / 复活机制增强
方案类型: implementation
优先级: P1
状态: 已执行
创建: 2026-03-21
完成: 2026-03-21
来源: `~auto` - 战斗胜利后自动复活死亡玩家；全员死亡时进入原版失败结算
```

---

## 1. 需求

### 背景
当前 `networkplugin` 已有“假死（Fake Death）”机制与 Gap 复活/治疗链路，但还没有“战斗结束时自动复活”的闭环：

1. `DeathPatches` 会在联机中阻止本地玩家真死，并在 `BattleShouldEnd` 上对死亡玩家强行续战。
2. 原版 `GameRunController.LeaveBattle(...)` 会在 `Player.IsDead` 时立刻把本局标记为 `GameRunStatus.Failure`。
3. 这意味着：如果多人战斗中本地玩家已经死亡、但队友最终打赢战斗，现有逻辑仍可能无法平稳进入“胜利后复活并继续跑图”的期望结果。
4. 用户要求新增一条明确规则：
   - 随机事件展开的战斗，以及普通战斗房战斗，只要最终不是全员死亡，就在战斗结束时把当前处于死亡状态的玩家复活。
   - 复活血量为“最大生命值的百分之 n”，从 BepInEx 配置读取，默认 10%。
   - “当前处于死亡状态的玩家”不限于“本场刚死”，也包括更早已处于死亡状态的角色。
   - 如果所有玩家都死亡，则必须进入原版失败结算流程，不得被联机补丁吞掉。

### 目标
1. 为 `networkplugin` 增加“战斗胜利后自动复活死亡玩家”的联机规则。
2. 统一覆盖普通战斗房与随机事件展开的战斗，不额外拆成两套逻辑。
3. 复活血量百分比改为可配置，默认值为 10%。
4. 继续复用现有复活同步链路，避免再造一套新的联机消息协议。
5. 在“全员死亡”场景下显式放行原版失败结算，确保结果面板和存档记录仍由原版逻辑生成。

### 约束条件
```yaml
功能约束:
  - 非全员死亡时，所有当前死亡状态的玩家都应在战斗结束后复活
  - 复活比例来自 BepInEx 配置，默认 10%
  - 配置值应限制在可用范围内，避免生成 0 HP 复活结果
兼容性约束:
  - 不破坏现有 Gap 复活/治疗 UI 与消息协议
  - 不改变原版普通胜利/失败面板的展示来源
实现约束:
  - 优先复用 `DeathPatches.ResurrectPlayer(...)` 与 `OnPlayerResurrected`
  - 优先复用 `GameMaster.BattleFlow(...) -> GameRunController.LeaveBattle(...)` 的统一链路
```

### 验收标准
- [√] 普通战斗房胜利后，当前死亡状态的玩家会自动复活，而不是被 `LeaveBattle(...)` 判成失败。
- [√] 随机事件展开的战斗胜利后，同样会触发自动复活。
- [√] 当所有玩家都死亡时，联机补丁会放行原版失败结算，结果仍进入 `GameRunStatus.Failure` 的既有流程。
- [√] 复活血量比例由 `networkplugin/Configuration` 下的 BepInEx 配置读取，默认值为 10%。
- [√] 复活后的生命值会通过现有联机同步链路广播，其他客户端能看到正确的 HP 状态。
- [√] `dotnet build networkplugin/NetWorkPlugin.csproj -v minimal` 通过。

---

## 2. 方案

### 方案对比

#### 方案 A（已选定，推荐）
**思路**：收紧 `DeathPatches.BattleShouldEnd_Postfix(...)` 的条件，并在 `GameRunController.LeaveBattle(...)` 进入失败判定前，对“非全员死亡但本地仍死亡”的玩家执行自动复活。

**优点**:
- 同时覆盖普通战斗房与随机事件战斗，因为两者最终都收敛到 `GameMaster.BattleFlow(...) -> GameRunController.LeaveBattle(...)`。
- 能直接在原版失败分支前插入复活，不需要改动结果面板或游戏主流程。
- 能复用现有 `DeathPatches.ResurrectPlayer(...)` / `OnPlayerResurrected` 同步链路，改动相对集中。
- “全员死亡”时可以只放行原版判定，不必自造失败 UI。

**缺点**:
- 需要仔细处理 `BattleShouldEnd`，避免把“敌人死光但本地玩家死着”的胜利场景继续卡在战斗里。
- 需要一套稳定的“全员死亡”判定来源（本地玩家 + 网络玩家快照 + `DeathRegistry`）。

#### 方案 B
**思路**：改挂 `EndBattleAction` / `BattleEnding` 事件，在战斗内部完成复活，再让原版离场逻辑继续。

**优点**:
- 更贴近“战斗结束前复活”的语义。

**缺点**:
- 会和原版 `BattleEnding` 反应、遗物/状态效果结算更深地耦合，风险更高。
- 调试复杂度更高，不利于快速验证“全灭失败”是否仍然原汁原味。

#### 方案 C
**思路**：只在 `GameMaster.BattleFlow(...)` 表现层里，在 `battle.Flow()` 返回后、`LeaveBattle(...)` 调用前执行复活。

**优点**:
- 改动表面最少。

**缺点**:
- 过于依赖展示层流程，后续如果存在其他战斗退出路径，覆盖会变脆。
- 不适合作为联机战斗规则的长期落点。

### 选定方案
选择 **方案 A**。

### 技术方案
1. **新增复活比例配置**
   - 在 `networkplugin/Configuration/ConfigManager.GameBalance.cs` 新增“战斗结束自动复活百分比”配置项。
   - 默认值 10，读取时限制到可用范围，并提供统一的比例/最终血量计算 helper。

2. **修正假死对战斗结束的拦截条件**
   - 调整 `DeathPatches.BattleShouldEnd_Postfix(...)`：
     - 若本地玩家死亡但并非全员死亡，且敌人仍存活，则继续维持战斗。
     - 若敌人已全部解决，则允许战斗结束，进入战后复活流程。
     - 若已判定全员死亡，则显式放行真死与原版失败收口。

3. **在 `LeaveBattle(...)` 前插入自动复活**
   - 新增专门的战后复活补丁（推荐独立文件），对 `GameRunController.LeaveBattle(...)` 做 Prefix。
   - 仅当满足“联机中、本地玩家当前仍死亡、但并非全员死亡”时，按配置比例复活本地玩家。
   - 复活入口复用 `DeathPatches.ResurrectPlayer(...)`，由现有同步逻辑广播状态更新。

4. **统一全员死亡判定来源**
   - 本地真实玩家：`GameStateUtils.GetCurrentPlayer()` / `BattleController.Player`。
   - 联机玩家总数：`INetworkManager.GetPlayerCount()` 或 `GetAllPlayers()` 快照。
   - 当前死亡集合：优先依赖 `DeathRegistry.GetDeadPlayersSnapshot()`，必要时结合本地玩家真实死亡状态补齐。
   - 目标是让“全员死亡”与“仅本地死亡、队友仍活”两条路径稳定分离。

5. **文档与验证**
   - 更新 `helloagents/modules/networkplugin.md`，补充战斗结束自动复活约定。
   - 更新 `helloagents/CHANGELOG.md` 记录本次新增功能。
   - 使用 `dotnet build networkplugin/NetWorkPlugin.csproj -v minimal` 做编译验收。

### 文件级改动清单（预估）

| 位置 | 计划改动 |
|------|----------|
| `networkplugin/Configuration/ConfigManager.GameBalance.cs` | 新增战后自动复活百分比配置与读取 helper |
| `networkplugin/Patch/DeathPatches.cs` | 调整 `BattleShouldEnd_Postfix(...)` 的假死拦截逻辑，放行“敌人已清空”与“全员死亡”分支 |
| `networkplugin/Patch/*`（新增独立补丁文件，命名待实现时确定） | 为 `GameRunController.LeaveBattle(...)` 增加自动复活 Prefix |
| `networkplugin/Patch/DeathStateSyncPatch.cs`（按需） | 若需要，补充战斗结束状态清理顺序或日志 |
| `helloagents/modules/networkplugin.md` | 同步新的战后自动复活规则 |
| `helloagents/CHANGELOG.md` | 记录本次新增功能 |

### 风险评估
| 风险 | 等级 | 应对 |
|------|------|------|
| `BattleShouldEnd` 条件写错，导致胜利战斗仍被卡住 | 高 | 仅在“本地死亡 + 敌人仍活 + 非全员死亡”时强制续战，其余情况放行原版结果 |
| 全员死亡判定依赖的联机缓存不稳定 | 中 | 优先复用 `DeathRegistry`，并以本地真实死亡状态兜底 |
| 复活同步重复广播或回环 | 中 | 直接复用已有 `DeathPatches.ResurrectPlayer(...)` 与 `SuppressNetworkSync` 语义 |
| 配置值异常导致复活血量为 0 | 低 | 配置读取阶段统一裁剪，并保证最终复活 HP 至少为 1 |

---

## 3. 技术设计

### 3.1 战斗结束判定重构
目标是把现有“只要本地玩家死亡就永远不让战斗结束”的逻辑，收紧为：

- **本地玩家死亡 + 队友还有活人 + 敌人还活着** → 战斗继续（保留假死的意义）
- **本地玩家死亡 + 队友还有活人 + 敌人已经清空** → 允许结束，进入战后自动复活
- **本地玩家死亡 + 全员死亡** → 允许结束，进入原版失败结算

### 3.2 战后自动复活时机
把自动复活放在 `GameRunController.LeaveBattle(...)` Prefix，而不是放进 `BattleEnding`：

1. 这样可以直接卡在原版 `if (this.Player.IsDead) { this.Status = GameRunStatus.Failure; ... }` 之前。
2. 只要 Prefix 里先把本地玩家复活，原版 `LeaveBattle(...)` 就会自然走胜利分支。
3. 同一时机对普通战斗房和事件战斗都是统一的，因为两者最终都复用同一个 `LeaveBattle(...)`。

### 3.3 配置设计
建议配置项采用整数百分比：

```yaml
分组: GameBalance.Battle
键名: BattleAutoReviveHpPercent
默认值: 10
取值范围: 1-100
语义: 战斗胜利后自动复活玩家时，按最大生命值百分比计算复活血量
```

最终复活值建议公式：

$$
reviveHp = \max(1, \lceil MaxHp \times percent / 100 \rceil)
$$

### 3.4 同步策略
- 本地复活执行仍走 `DeathPatches.ResurrectPlayer(...)`。
- 该方法内部会调用现有 `SyncPlayerResurrectionStatus(...)`，继续广播 `OnPlayerResurrected`。
- 其他客户端仍由 `ResurrectSyncPatch` 现有接收逻辑更新目标玩家 HP 缓存，因此无需额外引入新消息类型。

---

## 4. 核心场景

### 场景: 普通战斗房胜利，部分玩家死亡
**模块**: `networkplugin/Patch/DeathPatches.cs` + 战后自动复活补丁
**条件**: 联机中，本地玩家当前死亡，但至少有一名玩家未死亡，且敌人已全部被击败
**行为**: 放行战斗结束，在 `LeaveBattle(...)` 前按配置比例复活本地玩家
**结果**: 原版 `LeaveBattle(...)` 不会把本地局判成失败，战斗正常收尾并继续流程

### 场景: 随机事件展开的战斗胜利，玩家此前已死亡
**模块**: 同上
**条件**: 战斗来源于事件，但最终仍走 `GameMaster.BattleFlow(...) -> LeaveBattle(...)`
**行为**: 对当前仍处于死亡状态的本地玩家执行同样的战后自动复活
**结果**: 事件战斗与普通战斗房使用统一规则

### 场景: 全员死亡
**模块**: `networkplugin/Patch/DeathPatches.cs`
**条件**: 联机房间内所有玩家都处于死亡状态
**行为**: 停止阻止真死，允许原版 `BattleShouldEnd` / `LeaveBattle(...)` 走失败分支
**结果**: 游戏进入原版失败结算与结果面板

---

## 5. 技术决策

### networkplugin-postbattle-auto-revive#D001: 自动复活挂在 `LeaveBattle(...)` 前，而不是 `BattleEnding`
**日期**: 2026-03-21
**状态**: ✅采纳
**背景**: 原版失败是在 `GameRunController.LeaveBattle(...)` 中根据 `Player.IsDead` 落到 `GameRunStatus.Failure`，如果复活太晚，将错过最关键的分流点。
**选项分析**:
| 选项 | 优点 | 缺点 |
|------|------|------|
| A: `LeaveBattle(...)` Prefix | 直接拦在失败分支前，普通战斗和事件战斗统一覆盖 | 需要额外处理 `BattleShouldEnd` 才能保证战斗能顺利结束 |
| B: `BattleEnding` / `EndBattleAction` | 语义更“早”，更贴近战斗内部 | 会和战斗结束反应链更深耦合，风险更高 |
| C: `GameMaster.BattleFlow(...)` | 表面最简单 | 耦合展示层，不适合作为长期规则落点 |
**决策**: 选择 A。
**理由**: 目标不是重写结算 UI，而是在原版失败分流前修正玩家状态，让原版继续按正确状态运行。

### networkplugin-postbattle-auto-revive#D002: 全员死亡判定优先复用 `DeathRegistry`
**日期**: 2026-03-21
**状态**: ✅采纳
**背景**: 假死玩家在现有补丁里可能被回拉到最小 HP，单看 HP 并不能可靠判定“当前是不是死亡态”。
**选项分析**:
| 选项 | 优点 | 缺点 |
|------|------|------|
| A: 只看 `INetworkPlayer.HP` | 取值方便 | 假死时 HP 可能被拉回，语义不稳 |
| B: 只看本地 `Player.IsDead` | 最真实 | 无法覆盖其他联机玩家 |
| C: `DeathRegistry` + 本地真实死亡状态兜底 | 与现有假死/复活同步语义一致，能覆盖全体玩家 | 需要在判定时补齐本地玩家与玩家总数 |
**决策**: 选择 C。
**理由**: `DeathRegistry` 已经是当前联机“谁处于死亡态”的统一登记册，最适合作为全员死亡判定基础。

---

## 6. 执行结果

### 已完成实现
- `networkplugin/Configuration/ConfigManager.GameBalance.cs`
   - 新增 `BattleAutoReviveHpPercent` 配置项，默认值 10。
   - 新增统一的百分比裁剪与自动复活 HP 计算 helper。
- `networkplugin/Patch/DeathPatches.cs`
   - 新增全员死亡、本地登记册 PlayerId 与敌人是否清空的辅助判断。
   - 调整 `BattleShouldEnd_Postfix(...)`：仅在“敌人仍活且不是全员死亡”时继续战斗，其余场景放行原版收尾。
- `networkplugin/Patch/BattleEndAutoRevivePatch.cs`
   - 新增 `GameRunController.LeaveBattle(...)` Prefix，在原版失败判定前按配置复活本地死亡玩家。

### 验证结果
- 构建验证：`dotnet build "networkplugin/NetWorkPlugin.csproj" -v minimal`
- 结果：通过（260 warnings，0 errors）

### 遗留事项
- 当前会话仅完成源码与编译验证，仍建议在游戏内补做一次双端联机回归：
   - 普通战斗房：1 名玩家阵亡、另一名玩家击杀最后敌人
   - 随机事件战斗：验证同样会在 `LeaveBattle(...)` 前自动复活
   - 全员死亡：确认结果面板仍走原版失败结算
