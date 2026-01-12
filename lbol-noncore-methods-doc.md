# LBoL 非核心模块速查（LBoL.Base / LBoL.ConfigData / LBoL.EntityLib / LBoL.Presentation）

> 目标：给 Mod/联机补丁开发提供“去哪里找代码、从哪条链路入手”的快速索引。
> 本文偏导航，不展开贴大量源码。

---

## 1) LBoL.Base（基础类型与通用工具）

主要放“跨模块都会用到”的底层类型/扩展方法/辅助工具。

- 常见用途
  - 数值/随机/集合/序列化等工具方法
  - 扩展方法（例如各种 `ToXxx()`、`TryGetXxx()`）
  - 跨模块共享的轻量基础结构
- 开发建议
  - 这里的类型通常被大量引用：改动影响面大
  - Mod 更推荐“新增/扩展”而不是改动现有行为

---

## 2) LBoL.ConfigData（配置数据）

主要承载“数据驱动”的部分：卡牌/状态/展品/敌人等实体的配置结构与数据表。

- 常见用途
  - 修改卡牌/状态/掉落/数值：优先找对应 `Config` / `Table` / `ConfigData` 定义
  - 定位某个实体的数据来源：从 `*.Config`、`*.Table`、或加载器入口追
- 开发建议
  - 如果你要做“平衡调整/数值改动”，多数时候应先改配置而不是改逻辑

---

## 3) LBoL.EntityLib（实体库：卡牌/状态/敌人/展品/冒险等）

这是内容体量最大的模块之一，主要是“具体实现”：大量卡牌、状态效果、敌人 AI、展品、冒险事件等。

### 3.1 卡牌（Cards）

- 定位路径：`lbol/LBoL.EntityLib/Cards/`
- 常见关注点
  - 继承体系：`Card`（在 `LBoL.Core`）+ 具体卡牌实现（在 `EntityLib`）
  - 结算链路：通常通过 `BattleAction` / `BattleController` 推进

### 3.2 状态效果（StatusEffects）

- 定位路径：`lbol/LBoL.EntityLib/StatusEffects/`
- 常见关注点
  - 多数状态效果会通过事件（Damage/Turn/CardUsed 等）挂钩逻辑
  - 视觉表现常由 `StatusEffect.UnitEffectName` / `Config.VFX` 驱动（具体见 `LBoL.Presentation`）

### 3.3 敌人（EnemyUnits / Intentions）

- 定位路径：`lbol/LBoL.EntityLib/EnemyUnits/`、`lbol/LBoL.EntityLib/Intentions/`（若存在）
- 常见关注点
  - 敌人行动/意图：通常是“意图对象 + 每回合决策 + 行动队列”

### 3.4 展品（Exhibits）与冒险（Adventures）

- 定位路径：`lbol/LBoL.EntityLib/Exhibits/`、`lbol/LBoL.EntityLib/Adventures/`
- 常见关注点
  - 展品：常在战斗/事件/回合钩子触发
  - 冒险：通常是 UI 选择 + 结果动作（奖励、战斗、状态变化等）

---

## 4) LBoL.Presentation（表现层：UI/单位视图/特效/弹幕）

这是“看得见”的部分：UI 面板、单位 `UnitView`、特效系统、弹幕系统、动画与交互等。

### 4.1 UnitView 与状态特效渲染（非常重要）

- `UnitView` 管理单位身上的循环特效：
  - `TryPlayEffectLoop(effectName)`：创建并缓存 loop 特效
  - `SendEffectMessage(effectName, message, args)`：向特效对象发消息（Unity SendMessage）
  - `EndEffectLoop(effectName, instant)`：结束并移除 loop 特效
- 状态效果与特效的关联点：
  - `StatusEffect.UnitEffectName` 非空时，添加状态会触发 `TryPlayEffectLoop`，移除状态会触发 `EndEffectLoop`

### 4.2 EffectManager / EffectWidget

- `EffectManager.CreateEffect(...)`：创建一次性或循环特效（挂到某个 `Transform` 根节点）
- `EffectWidget`：特效实体/行为脚本的承载（可接收 `SendMessage`）

### 4.3 UI 结构

- `UI/Panels/`：大面板（主菜单、地图、战斗面板、商店、奖励等）
- `UI/Widgets/`：小组件（状态图标、提示、数值显示等）
- `UiManager`：UI 管理入口（面板切换、阻塞输入、加载界面等）

---

## 5) 从“现象”反推代码的实用路径

- 看到“战斗结算问题”：优先从 `LBoL.Core/Battle`（Action / Viewer / Controller）追
- 看到“卡牌效果问题”：先找 `EntityLib` 具体卡牌，再回到 `Core` 的 `Card/BattleAction` 链路
- 看到“状态效果/特效不显示”：
  - 先看 `StatusEffect.UnitEffectName` 是否设置
  - 再看 `UnitView.TryPlayEffectLoop/EndEffectLoop/SendEffectMessage`
  - 再看 `EffectManager.CreateEffect` 是否被调用、挂载根节点是否正确

---

## 6) 相关文档

- `lbol-complete-project-doc.md`：全项目目录总览
- `lbol-methods-doc.md`：LBoL.Core 详细目录与入口点

