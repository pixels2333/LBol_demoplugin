# task

目标：基于 lbol/ 原游戏源码，设计“补丁类方案库”，并产出可执行的任务清单（后续用 ~exec 落地）。

## 任务清单

### 1) 明确范围与约束
- [ ] 确认范围：仅 networkplugin，或同时覆盖 MyFirstPlugin。
目标：基于 lbol/ 原游戏源码，为 networkplugin 设计“补丁类方案库”（清单 + 约定），聚焦联机同步。
- [ ] 确认生成方式：仅输出方案库（清单+约定），或同时实现自动生成器。

 [ ] 范围固定：仅 networkplugin。
 [ ] 目标固定：联机同步为主（尽量不扩展到与联机无关的 UI 注入/平衡性改动）。
 [ ] 输出固定：仅输出方案库（清单 + 约定），不实现自动生成器。
  - 同步类：*SyncPatch
  - 兼容修复：*Patch 或 *FixPatch
  - 目录以场景归类，避免按作者/随意文件名堆叠

### 3) 从 lbol/ 提取“高价值补丁点”候选列表
- [ ] Battle 流程：BattleController（伤害/治疗/状态效果/回合）
 [ ] UI：只纳入“联机入口/安全退出/目标选择”等与联机体验相关的最小集合
 [ ] Save/Load：存档/读档前后关键边界（仅作为联机一致性保障）
- [ ] Map 与推进：进入节点、节点状态变化
- [ ] UI：主菜单入口、退出流程、目标选择器
- [ ] Save/Load：存档/读档前后关键边界

### 4) 形成 PatchCatalog（方案库核心）
- [ ] 定义字段：category/target/hook/purpose/netEvent/guards。
- [ ] 为每条记录补充：
  - Hook 类型建议（Prefix/Postfix 等）
### 5) 维护约定（方案库生命周期）
 - [ ] 新增/修改补丁前：先更新 PatchCatalog 条目，再进行代码变更。
 - [ ] 每次发布前：对照 lbol/ 源码与现有补丁点，检查是否存在失效/重复/回环风险。
- [ ] 选择输出目录：建议 networkplugin/Patch/Generated。
- [ ] 生成骨架包含：HarmonyPatch 标注、类名、空的 Prefix/Postfix、Guard 占位。
- [ ] 将生成产物与手写补丁分层，避免互相覆盖。

 [ ] 方案库内容与现有 networkplugin/Patch 风格一致，且条目能指导后续补丁实现/审计。
- [ ] 选 3-5 个最小闭环场景，用方案库指导新增补丁并通过编译验证。
- [ ] 约定后续增量维护流程：新增补丁必须先更新 catalog，再写补丁代码。
 本方案为“design-only”，不包含 ~exec 的实现落地步骤。
## 验收标准
- [ ] 形成可执行的补丁分类与命名约定。
- [ ] 形成一份可扩展的 PatchCatalog 模型与初始条目（覆盖核心联机同步点）。
- [ ] 给出可选的自动生成策略（不要求本阶段实现）。

## 执行
- 后续需要落地实现时，使用 ~exec 执行本方案包：helloagents/plan/202601181200_patchlibrary_from_lbol/
