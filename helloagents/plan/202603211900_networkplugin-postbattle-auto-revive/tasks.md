# 任务清单: networkplugin-postbattle-auto-revive

目录: `helloagents/plan/202603211900_networkplugin-postbattle-auto-revive/`

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
总任务: 8
已完成: 8
已跳过: 0
待确认: 0
完成率: 100%
当前阶段: 开发实施完成，待归档
```

---

## 任务列表

### 1. 配置项接入（P1）

- [√] 1.1 在 `networkplugin/Configuration/ConfigManager.GameBalance.cs` 中新增战斗结束自动复活百分比配置项
  - 目标: 新增 BepInEx 配置，默认值 10，语义明确为“最大生命值百分比”
  - 验证: 已新增 `BattleAutoReviveHpPercent`，并接入 `BindGameBalanceSettings(...)`

- [√] 1.2 为自动复活百分比增加统一读取/裁剪 helper
  - 目标: 保证配置取值落在可用范围，避免出现 0 HP 或超过最大生命的异常结果
  - 依赖: 1.1
  - 验证: 已新增 `GetBattleAutoReviveHpPercent()` 与 `CalculateBattleAutoReviveHp(...)`

### 2. 战斗结束判定修正（P1）

- [√] 2.1 调整 `networkplugin/Patch/DeathPatches.cs` 中 `BattleShouldEnd_Postfix(...)` 的假死拦截条件
  - 目标: 仅在“本地死亡 + 非全员死亡 + 敌人仍存活”时阻止战斗结束
  - 验证: 已按“继续战斗 / 放行胜利收尾 / 放行原版失败”三分支收口

- [√] 2.2 为 `DeathPatches` 增加“全员死亡 / 敌人是否清空 / 复活血量计算”的辅助判定逻辑
  - 目标: 复用 `DeathRegistry`、`INetworkManager` 与本地玩家状态形成稳定判断
  - 依赖: 2.1
  - 验证: 已新增 `AreAllKnownPlayersDead(...)`、`AreAllEnemiesResolved(...)`、`CalculateConfiguredAutoReviveHp(...)`

### 3. 战后自动复活落地（P1）

- [√] 3.1 新增战后自动复活补丁，对 `LBoL.Core.GameRunController.LeaveBattle(...)` 增加 Prefix
  - 目标: 在原版失败判定前处理“非全员死亡但本地仍死亡”的自动复活
  - 验证: 已新增 `Patch/BattleEndAutoRevivePatch.cs`

- [√] 3.2 复用现有 `DeathPatches.ResurrectPlayer(...)` / `OnPlayerResurrected` 同步链路，不新增重复协议
  - 目标: 自动复活后其他客户端能收到 HP 更新
  - 依赖: 3.1
  - 验证: 自动复活直接调用 `DeathPatches.ResurrectPlayer(...)`，未新增消息类型

- [√] 3.3 保持“全员死亡 -> 原版失败结算”链路原样可达
  - 目标: 当所有玩家都死亡时，不执行自动复活，而是允许 `GameRunStatus.Failure` 落地
  - 依赖: 2.2, 3.1
  - 验证: `ShouldAutoReviveAfterBattle(...)` 会在全员死亡时放行真死，不执行自动复活

### 4. 验证与文档同步（P1）

- [√] 4.1 运行 `dotnet build networkplugin/NetWorkPlugin.csproj -v minimal` 做构建验证
  - 目标: 确认新增配置与补丁能够正常编译
  - 验证: 构建通过（260 warnings，0 errors）

- [√] 4.2 更新 `helloagents/modules/networkplugin.md` 与 `helloagents/CHANGELOG.md`
  - 目标: 记录新的战斗结束自动复活规则、配置项和失败分支约束
  - 依赖: 1.x, 2.x, 3.x, 4.1
  - 验证: 已同步模块文档、变更日志与方案执行结果

---

## 执行备注

| 任务 | 状态 | 备注 |
|------|------|------|
| 设计确认 | completed | 已确认采用“方案 A：收紧 BattleShouldEnd + 在 LeaveBattle 前按配置复活” |
| 构建验证 | completed | `dotnet build "networkplugin/NetWorkPlugin.csproj" -v minimal` 通过（260 warnings，0 errors） |
| 运行时回归 | uncertain | 当前会话未接入游戏运行时，建议补做双端联机手工验证 |
