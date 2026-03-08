# 任务清单: otherplayers-overlay-native-ui

目录: `helloagents/archive/2026-03/202603070020_otherplayers-overlay-native-ui/`

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
总任务: 15
已完成: 14
已跳过: 1
待确认: 0
完成率: 93%
```

---

## 任务列表

### 1. 远端战斗状态数据链路补强

- [√] 1.1 在 `networkplugin/Network/Snapshot/PlayerStateSnapshot.cs` 中新增远端符卡积攒能量相关数值字段
  - 目标: 为 overlay 提供 `CurrentPower`、`PowerPerLevel`、`MaxPowerLevel` 或等价字段
  - 验证: 快照序列化/反序列化后，字段能在发送端与接收端保持一致

- [√] 1.2 在 `networkplugin/Network/PlayerEntity.cs` 中补齐快照构建/应用对符卡积攒能量字段的读写
  - 目标: 让完整快照与回合快照能带出远端当前充能数值，而不是只保留 `ultimatePower` bool 语义
  - 验证: `CreateSnapshot()` / `ApplySnapshot()` 覆盖新增字段

- [√] 1.3 在 `networkplugin/Patch/Network/TurnStartSnapshotReceivePatch.cs` 与 `networkplugin/Patch/Network/TurnEndSnapshotReceivePatch.cs` 中落地远端 HP/Shield/Block/Power 数值
  - 目标: 让远端玩家对象或 overlay 适配缓存拿到最新战斗快照
  - 验证: turn start / turn end 接收后可读取到对应玩家的最新数值

- [√] 1.4 新增 `networkplugin/Patch/UI/OtherPlayersOverlay/OtherPlayersOverlayBattleState.cs`
  - 目标: 汇总 `INetworkManager.GetPlayer(playerId)`、回合快照和完整状态同步结果，输出 overlay 专用的远端战斗状态读取接口
  - 验证: `OtherPlayersOverlayPatch` 不再直接散落读取多个来源；同一方法可拿到 HP/Shield/Block/Power/PowerPerLevel

### 2. OtherPlayersOverlay 原生控件复用

- [√] 2.1 在 `networkplugin/Patch/UI/OtherPlayersOverlayPatch.cs` 中调整 `PrepareAvatarTemplate(...)`
  - 目标: 保留 `powerText`、`gauge1`、`gauge2`、`gauge3`，只关闭交互、Tooltip 与粒子
  - 验证: 运行时模板仍为非交互控件，且 gauge 区域可见

- [√] 2.2 在 `networkplugin/Patch/UI/OtherPlayersOverlayPatch.cs` 中扩展 `AvatarEntryUi`
  - 目标: 为每个条目保存头像、名字、状态、`powerText`、三段 gauge、血条组件引用
  - 验证: `EnsureAvatarEntry(...)` 返回的条目结构足以完成全部 UI 绑定

- [√] 2.3 在 `networkplugin/Patch/UI/OtherPlayersOverlayPatch.cs` 中把 `skillImage` 改为角色头像加载逻辑
  - 目标: 优先复用角色头像资源，而不是 Ultimate Skill 图标
  - 验证: 不同角色玩家显示各自头像；资源缺失时回退到现有头像缓存/白图

### 3. 原生血条接入与布局收敛

- [√] 3.1 在 `networkplugin/Patch/UI/OtherPlayersOverlayPatch.cs` 中新增原生血条模板解析/缓存逻辑
  - 目标: 从 live `UnitStatusWidget` / `HealthBar` 或 `UnitStatusHud` 模板克隆可复用血条子树
  - 验证: 首次进入战斗后仅创建一次模板，后续条目复用缓存模板

- [√] 3.2 在 `networkplugin/Patch/UI/OtherPlayersOverlayPatch.cs` 中实现右侧血条挂接与更新
  - 目标: 使用 `HealthBar.SetHp/SetShield/TweenHp` 显示远端 HP/Shield/Block，并按照玩家血量上限匹配长度
  - 验证: 远端受伤、治疗、获得护盾/格挡后，overlay 血条数值和 fillAmount 正常更新

- [√] 3.3 在 `networkplugin/Patch/UI/OtherPlayersOverlayPatch.cs` 中调整横向布局算法
  - 目标: 在保留 `BaseMana` 锚定的前提下，为“头像卡片 + 右侧血条”提供足够宽度，并在多人时自动缩放
  - 验证: 2-4 名远端玩家并排时不重叠、不超出主要可视区域

### 4. façade 稳定与兼容回归

- [√] 4.1 保持 `networkplugin/Patch/UI/OtherPlayersOverlay/OtherPlayersOverlayPlayerStore.cs` 现有名字/快照入口稳定
  - 目标: `ResolveDisplayName(...)`、`SnapshotPlayersDetailed()` 等现有调用方无需同步重构
  - 验证: `TradePanel`、地图图标、目标选择辅助静态入口不需要改签名

- [√] 4.2 在 `networkplugin/Patch/UI/OtherPlayersOverlayPatch.cs` 中保留 `TryAttachUiToBaseMana()` 现有层级与定位规则
  - 目标: UI 样式升级不影响此前刚修复的 `BaseMana` 同级容器约束
  - 验证: `NetworkPlugin_OtherPlayersOverlay` 仍挂到 `BaseMana` 同级容器，屏幕位置不漂移

### 5. 验证与知识库同步

- [√] 5.1 执行 `dotnet build networkplugin/NetWorkPlugin.csproj -v minimal`
  - 目标: 确认本次 UI 与数据链路改造可编译
  - 验证: 构建 0 error

- [-] 5.2 执行多人战斗手工回归
  - 场景: 远端获得符卡能量、使用符卡、受到伤害、治疗、获得格挡/护盾、掉线重连
  - 验证: overlay 的头像、充能条、血条与真实远端状态一致
  - 备注: 本次会话仅完成静态实现与编译验证，缺少双端联机运行环境，手工回归顺延给下一次实机验证

- [√] 5.3 更新 `helloagents/modules/networkplugin.md` 与 `helloagents/CHANGELOG.md`
  - 目标: 记录 overlay 原生化 UI 的结构约定、数据来源与回归要点
  - 验证: 模块文档与变更记录可追溯本方案的最终落地情况

---

## 执行备注

| 任务 | 状态 | 备注 |
|------|------|------|
| 1.x | completed | 已补齐 `PlayerStateSnapshot` / `PlayerEntity` / 回合快照接收 / 重连链路上的数值化 power 字段 |
| 2.x | completed | `UltimateSkillPanel` gauge 与 `powerText` 已恢复显示，头像改为远端角色头像，Tooltip/粒子保持关闭 |
| 3.x | completed | 已缓存并复用 `UnitStatusHud` 的原生血条模板，条目右侧会按远端 HP/Shield/Block 刷新显示 |
| 4.x | completed | 继续保留 façade 对外静态入口，并维持 `BaseMana` 同级挂载与锚点定位逻辑 |
| 5.x | partial | 本次已完成 `dotnet build networkplugin/NetWorkPlugin.csproj -v minimal`；多人实机回归因缺少双端环境顺延 |
|------|------|------|
