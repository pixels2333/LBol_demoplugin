# how

## 1. 范围与输出物
范围：仅 networkplugin。

目标：联机同步为主（尽量不引入与联机无关的 UI 注入/平衡性改动）。

输出物（方案库产物）：
- PatchCatalog（概念模型与清单）：一条补丁记录包含：
  - category：Battle / Actions / Units / Map / UI / Network / SaveLoad 等
  - target：命名空间、类型名、成员名（方法/属性）
  - hook：Prefix/Postfix/Transpiler/ReversePatch（如确有必要）
   - purpose：联机同步（必要时可细分：状态同步/流程门控/一致性修复）
  - netEvent：对应网络消息类型（若适用）
  - guards：联机连接判断、房主/非房主限制、去重/抑制回环策略

说明：本方案只输出“清单 + 约定”，不包含自动生成器与代码落地。

## 2. 分类体系（建议与现状对齐）
参考现有 networkplugin/Patch 目录，建议固定一级分类：
- Patch/Battle：战斗控制器、回合推进、战斗开始/结束
- Patch/Actions：BattleActions 构造/执行产生的可同步副作用
- Patch/Units：Unit/PlayerUnit/EnemyUnit 的属性变更与状态效果
- Patch/Map：进入节点、地图 UI 刷新、节点标记等
- Patch/UI：主菜单入口、退出流程、选择器/指向等交互
- Patch/Network：网络握手、状态同步、存档/加载等
- Patch/SaveLoad：存档/读档关键边界（若现有目录未拆分，可先归到 Patch/Network）

命名约定（建议）：
- 同步类：*SyncPatch
- 门控/一致性修复：*GatePatch 或 *FixPatch（如项目现有习惯为 *_Patch，也可保持一致）
- “目录优先”：按场景归类，避免按作者/临时需求堆叠

## 3. 补丁点选择原则（从 lbol/ 源码推导）
从 lbol/ 选择补丁点优先级：
1) “状态写入点”优于“状态读取点”
   - 例如属性 setter、Add/Remove、Try* 等会改变状态的方法。
2) “一次性动作边界”优于“每帧 Update”
   - Update 类 Hook 仅用于订阅/驱动（必须加节流与连接判断）。
3) 选择可被可靠判定的边界
   - 能用参数/返回值推导出网络 payload，避免依赖内部临时字段。
4) 防止事件回环
   - 任何发送网络事件的补丁都需要具备抑制机制（本地应用远端事件时不再二次广播）。

Guard 约定（建议最小集合）：
- 连接判断：仅在联机连接状态下启用。
- 角色判断：需要时区分房主/非房主（例如“状态权威”只由房主发送）。
- 去重与回环抑制：
  - 发送前：检查是否正在“应用远端事件”。
  - 接收端应用后：不再触发同类发送。
- 线程/时序：避免在高频路径里做重序列化；必要时节流。

## 4. 方案库清单格式（建议）
为了让清单可长期维护，建议按“分类 -> 条目”组织；每条记录至少包含以下字段：

- 分类：category
- 目标：target（Namespace.Type.Member）
- Hook：hook（Prefix/Postfix/Transpiler/ReversePatch）
- 目的：purpose
- 事件：netEvent（若适用）
- Guard：guards（连接/房主/抑制回环/节流）

推荐维护方式：
- 先写清单条目，再写/改补丁类；避免“补丁先行、清单滞后”。

## 5. 与现有实现的对齐
- 命名：沿用现有后缀 *_Patch 或 *SyncPatch 的习惯。
- 连接判断：仅在联机连接状态下生效，不影响单机。
- 投递方式：按事件重要性选择可靠/不可靠（设计时在 catalog 标注）。

## 6. PatchCatalog 初版条目（基于 lbol/ 源码 + 现有补丁目录）
说明：以下为“方案库条目”，用于指导后续补丁实现/审计；不在本阶段要求新增代码。

### Battle
1) LBoL.Core.Battle.BattleController.Damage
- hook：Postfix
- purpose：同步伤害结算后的状态（HP/Shield/Block 等）
- netEvent：OnDamage 或 BattleDamage（以现有事件命名体系为准）
- guards：仅联机；回环抑制；必要时房主权威

2) LBoL.Core.Battle.BattleController.Heal
- hook：Postfix
- purpose：同步治疗后的 HP
- netEvent：OnHeal
- guards：仅联机；回环抑制

3) LBoL.Core.Battle.BattleController.TryAddStatusEffect / RemoveStatusEffect
- hook：Postfix
- purpose：同步状态效果增删（含层数/持续）
- netEvent：OnStatusEffectAdded / OnStatusEffectRemoved
- guards：仅联机；回环抑制；必要时由权威端发送

### Actions
1) LBoL.Core.Battle.BattleActions.PlayCardAction(..)
- hook：Postfix（构造）或 Execute 边界
- purpose：同步卡牌使用的可观察副作用（卡牌移动、费用、目标）
- netEvent：OnPlayCard / RemoteCardUse
- guards：仅联机；回环抑制；防止重复结算

2) LBoL.Core.Battle.BattleActions.DamageAction(..)
- hook：Postfix（构造）
- purpose：捕获伤害动作的关键字段，辅助远端重放/展示
- netEvent：OnDamageAction
- guards：仅联机；避免高频重序列化

### Units
1) LBoL.Core.Units.Unit.set_Hp / set_Block / set_Shield / set_MaxHp
- hook：Postfix
- purpose：同步单位关键数值变更（本地玩家/敌人按需求区分）
- netEvent：UnitStateSync
- guards：仅联机；回环抑制；按对象类型过滤

2) LBoL.Core.Units.PlayerUnit.Status（或等价状态字段）
- hook：Postfix
- purpose：同步玩家状态变更（如死亡/不可操作等）
- netEvent：PlayerStatusSync
- guards：仅联机；回环抑制

### Map
1) LBoL.Presentation.UI.Panels.MapPanel.UpdateMapNodesStatus
- hook：Postfix
- purpose：同步地图节点状态/玩家位置展示（非权威逻辑，仅 UI 侧刷新）
- netEvent：PlayerLocationSync（若已有）
- guards：仅联机；节流；避免每帧发送

2) LBoL.Core.GameMap.EnterNode（或进入节点边界）
- hook：Postfix
- purpose：同步进入节点事件（推进一致性）
- netEvent：RoomEntrySync
- guards：仅联机；通常由权威端发送

### UI
1) LBoL.Presentation.MainMenuPanel.Awake/RefreshProfile（入口注入）
- hook：Postfix
- purpose：仅在联机可用时展示入口/状态，不改变单机行为
- netEvent：无
- guards：仅联机/或仅显示 UI；不做状态权威

### Network / SaveLoad
1) 存档/读档流程的关键边界（GameSave/GameLoad）
- hook：Prefix + Postfix
- purpose：保存/加载前后广播一致性事件，避免状态漂移
- netEvent：SaveSync / LoadSync
- guards：仅联机；房主权威；节流；失败回退策略
