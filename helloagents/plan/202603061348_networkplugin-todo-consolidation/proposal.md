# 变更提案: networkplugin-todo-consolidation

## 元信息
```yaml
类型: TODO治理 / UI行为修正
方案类型: implementation
优先级: P1
状态: 已执行（待商店 UI 手工回归）
创建: 2026-03-06
完成: 2026-03-06
来源: `~plan` - 扫描 `networkplugin/` 下全部精确 `//TODO:` 注释
扫描结果: 1 处（已完成代码收敛）
```

---

## 1. 需求

### 背景
本次扫描 `networkplugin/` 后，仅命中 1 处精确 `//TODO:` 注释：

| 位置 | TODO 原文 | 当前上下文 |
|------|-----------|-----------|
| `networkplugin/Patch/UI/ShopTradeIconPatch.cs`（`EnsureUi(ShopPanel shopPanel)` 内） | `检查这里是否冗余，如果克隆的层级结构和原来完全一样，理论上不应该有额外的 Text 组件了。` | 当前实现会在克隆 `CardService` 容器后，对 `midGo` 下所有 `TMP_Text` 和所有 `UnityEngine.UI.Text` 统一写入 `玩家交易`，并无差别覆盖整个子树中的文本组件。 |

这处 TODO 暗示当前逻辑存在两个潜在问题：

1. **目标不够精确**：代码没有只修改“交易按钮实际展示的标题节点”，而是粗暴扫描全部文本组件。
2. **兼容路径过宽**：`TMP_Text` 与 `Text` 双分支同时全量覆盖，可能会误改未来新增的计数、提示、阴影文本或辅助节点。
3. **诊断日志过重**：在完成文本覆盖后又无条件调用 `LogHierarchy(midGo.transform, 0)`，一旦 `EnsureUi` 频繁重建 UI，调试日志会持续刷屏。

### 目标
1. 将 `ShopTradeIconPatch` 的文本替换逻辑收敛为“只命中主标题节点”的确定性实现。
2. 保留对异常层级的兜底兼容，但把 fallback 从“默认路径”降级为“告警路径”。
3. 让按钮克隆后的文字、样式、点击行为仍保持正确，不影响 `CardService` 和 `Return` 两个原生按钮。
4. 将该处 TODO 转化为可执行、可验证、可归档的一组任务，便于后续 `~exec` 落地。

### 约束条件
```yaml
时间约束: 本轮聚焦 `ShopTradeIconPatch` 的单处 TODO，不扩展到其他 UI 补丁
性能约束: 后续实现不能在 `GameDirector.Update` 高频路径上引入新的全量层级扫描开销
兼容性约束: 必须保留当前“克隆 CardService 容器样式 + 重绑点击事件 + 固定坐标布局”的总体行为
业务约束: 按钮最终展示文案仍为 `玩家交易`，且 TradePanel 打开链路不变
```

### 验收标准
- [√] `networkplugin/Patch/UI/ShopTradeIconPatch.cs` 中不再对 `midGo` 全量遍历所有 `TMP_Text` 与 `Text` 并无差别改文案。
- [√] 新实现能够优先锁定“克隆交易按钮的主显示标签”，并只更新该目标节点。
- [√] 当克隆层级与预期不一致时，系统会走显式 fallback / warning 路径，而不是静默改写整棵子树。
- [√] `LogHierarchy(midGo.transform, 0)` 仅在诊断需要时触发，不再作为默认正常路径日志。
- [?] 商店 UI 中“玩家交易”按钮仍能正常显示并点击打开交易面板；`CardService` 与 `Return` 原生按钮文本不被误伤。
  - 备注: 当前环境缺少游戏内商店手工回归条件，已用源码检查 + `dotnet build networkplugin/NetWorkPlugin.csproj -v minimal` 替代静态验证。

---

## 2. 方案

### 技术方案
围绕唯一命中的 TODO，后续实现分为 4 个收敛动作：

1. **把文本改写从“全量扫描”改为“精确解析”**  
  在 `ShopTradeIconPatch` 内新增私有 helper，用于从克隆后的 `midGo` / `tradeButton` 分支中定位主标题文本节点，优先匹配 `TMP_Text`，必要时再回退到 `UnityEngine.UI.Text`。

2. **将 legacy `Text` 分支降级为 fallback，而不是默认并行逻辑**  
  保留对旧层级的兼容，但不再默认对全部 `Text` 组件覆盖 `玩家交易`；只有当主路径找不到有效 `TMP_Text` 时，才进入有日志标记的 legacy fallback。

3. **限制诊断日志触发条件**  
  当前 `LogHierarchy(midGo.transform, 0)` 适合作为排障工具，不适合作为稳定路径默认行为。后续应改成“仅在标签解析失败 / 出现多候选节点 / 显式调试开关启用时才输出”。

4. **把 UI 正确性验证聚焦到 3 个观察点**  
  - 克隆按钮标题是否正确显示 `玩家交易`
  - 原生 `CardService` / `ReturnButton` 文本是否保持原样
  - UI 重建时日志是否不再产生整棵层级 dump

### 文件级改动清单

| 位置 | 当前行为 | 计划改动 |
|------|----------|----------|
| `networkplugin/Patch/UI/ShopTradeIconPatch.cs` → `EnsureUi(ShopPanel shopPanel)` 中 `foreach (var label in midGo.GetComponentsInChildren<TMP_Text>(true))` | 将克隆节点下所有 `TMP_Text` 全部改成 `玩家交易` | 替换为调用新的主标签解析 helper，仅更新解析出的主标题节点 |
| `networkplugin/Patch/UI/ShopTradeIconPatch.cs` → `EnsureUi(ShopPanel shopPanel)` 中 `foreach (var t in midGo.GetComponentsInChildren<Text>(true))` | 将克隆节点下所有 legacy `Text` 全部改成 `玩家交易` | 移除默认全量覆盖；仅在主路径失败时作为 fallback 执行，并记录 warning |
| `networkplugin/Patch/UI/ShopTradeIconPatch.cs` → 文本替换代码块附近 | TODO 只提出“可能冗余”，尚未转成结构化实现 | 新增 `TryResolveTradeLabel(...)` / `ApplyTradeLabel(...)` 一类私有 helper，把 TODO 结论固化成代码路径 |
| `networkplugin/Patch/UI/ShopTradeIconPatch.cs` → `LogHierarchy(midGo.transform, 0)` 调用点 | 每次 UI 构建都输出完整层级 | 改为仅在主标签解析失败、命中多候选、或显式 debug 开关开启时输出 |
| `networkplugin/Patch/UI/ShopTradeIconPatch.cs` → 诊断日志与注释 | 诊断意图混在主流程里 | 补齐“主路径 / fallback / 诊断路径”的行为边界，减少后续再次出现同类 TODO |

### 影响范围
```yaml
涉及模块:
  - Patch/UI: `ShopTradeIconPatch` 的商店交易按钮克隆与文案重写逻辑
  - UI诊断日志: 层级日志输出条件调整
预计变更文件: 1
预计新增私有方法: 2-3 个
```

### 风险评估
| 风险 | 等级 | 应对 |
|------|------|------|
| 主标签解析过于严格，导致某些皮肤/层级下找不到文本节点 | 中 | 保留显式 fallback，并在 fallback 触发时输出警告和层级日志 |
| 只改主标签后遗漏描边/副标题等联动文本 | 中 | 以“可视主标题”作为唯一目标，若确有联动节点，再通过白名单补充而非恢复全量扫描 |
| 移除默认 `LogHierarchy` 后排障信息减少 | 低 | 将日志改成条件触发，不是完全删除；失败时仍保留完整诊断能力 |

---

## 3. 技术设计（可选）

> 本次不涉及外部 API 或数据模型变更，重点是 UI 克隆节点的内部解析策略。

### 结构调整

#### 3.1 `EnsureUi(...)` 内部流程收敛
后续建议把 `EnsureUi(...)` 的中段整理为以下顺序：

1. 克隆 `CardService` 容器并重绑 `Button.onClick`
2. 清理 tooltip / localization 组件
3. 调用 `TryResolveTradeLabel(...)` 锁定主标题文本节点
4. 主路径成功 → 只更新该节点文案为 `玩家交易`
5. 主路径失败 → 进入受控 fallback，并输出 warning + hierarchy 日志
6. 继续执行固定坐标布局和原生按钮容器位置调整

#### 3.2 建议新增的私有 helper

| Helper | 作用 | 预期放置位置 |
|--------|------|--------------|
| `TryResolveTradeLabel(GameObject root, Button tradeButton, out TMP_Text tmpLabel, out Text legacyLabel)` | 从克隆按钮分支中解析主标题标签，优先 `TMP_Text`，再回退 `Text` | `EnsureUi(...)` 下方、`LogHierarchy(...)` 上方或附近 |
| `ApplyTradeLabel(GameObject root, Button tradeButton, string labelText)` | 封装“主路径改写 + fallback 告警 + 诊断日志” | 与 `TryResolveTradeLabel(...)` 相邻 |
| `ShouldDumpTradeHierarchyOnBuild(...)`（可选） | 明确层级日志的触发条件，避免主流程里散落条件判断 | 与诊断辅助方法放在一起 |

#### 3.3 标签选择策略

优先级建议如下：

1. 优先在 `tradeButton` 自身子树内查找 `TMP_Text`
2. 若命中多个 `TMP_Text`，优先选择激活、可见、文本非空的标题节点
3. 若 `TMP_Text` 不存在，再查找 `Text`
4. 若仍未命中，再进入 fallback：有限集合扫描 + warning + `LogHierarchy`

这样可以让 TODO 中提到的“理论上不应该有额外 `Text` 组件”变成可执行规则：

- **正常路径**：不会去碰额外 `Text`
- **异常路径**：只有诊断时才碰，并留下日志证据

---

## 4. 核心场景

> 执行完成后同步到对应模块文档

### 场景: 标准商店层级下的交易按钮文案替换
**模块**: `networkplugin/Patch/UI/ShopTradeIconPatch.cs`
**条件**: `ShopPanel` 已显示，`CardService` 容器可被成功克隆，且克隆层级包含主标题 `TMP_Text`
**行为**: 代码只锁定主标题节点并写入 `玩家交易`
**结果**: 按钮样式继承原始容器，文案正确，且不会误改同层级的其他文本组件

### 场景: 非标准层级或 legacy 文本组件存在
**模块**: `networkplugin/Patch/UI/ShopTradeIconPatch.cs`
**条件**: 主标题 `TMP_Text` 未找到，或克隆层级出现多个候选文本节点
**行为**: 进入受控 fallback，输出 warning，并按需打印层级结构供排障
**结果**: UI 仍有机会显示正确文案，同时后续开发者能从日志中定位层级偏差，而不是继续保留模糊 TODO

---

## 5. 技术决策

> 本方案涉及的技术决策，归档后成为决策的唯一完整记录

### networkplugin-todo-consolidation#D001: 用“目标节点解析”替代“全量文本覆盖”
**日期**: 2026-03-06
**状态**: ✅采纳
**背景**: 当前 TODO 已明确指出 `EnsureUi(...)` 中对全部 `Text` 组件的覆盖可能冗余，但代码尚未把这种怀疑转成正式实现。
**选项分析**:
| 选项 | 优点 | 缺点 |
|------|------|------|
| A: 保持现状，继续全量覆盖所有 `TMP_Text`/`Text` | 实现简单，短期内不容易“漏改标题” | 容易误改无关文本，难以证明 TODO 已真正解决 |
| B: 精确解析主标题节点，legacy `Text` 仅做 fallback | 行为边界清晰，能消化 TODO，并减少误改风险 | 需要补一个小型解析 helper，并做一次 UI 回归 |
**决策**: 选择方案 B
**理由**: 该 TODO 的本质不是“把文本改掉”，而是“不要再无差别改整棵子树”；因此必须把主路径与 fallback 路径拆开。
**影响**: 仅影响 `ShopTradeIconPatch` 的内部实现，不改变外部 API 和 TradePanel 入口

### networkplugin-todo-consolidation#D002: `LogHierarchy` 改为条件诊断工具
**日期**: 2026-03-06
**状态**: ✅采纳
**背景**: `LogHierarchy(midGo.transform, 0)` 当前位于正常构建路径，容易在商店 UI 重建时产生大量冗余日志。
**选项分析**:
| 选项 | 优点 | 缺点 |
|------|------|------|
| A: 保留默认打印 | 排障信息最全 | 正常运行日志噪音大，掩盖真正异常 |
| B: 仅在失败/异常层级时打印 | 正常路径更干净，失败时仍有证据 | 需要补充一层触发条件判断 |
**决策**: 选择方案 B
**理由**: 层级 dump 是排障工具，不应该占据主路径。
**影响**: 影响 `ShopTradeIconPatch` 构建时的日志可观测性策略，不影响 UI 功能本身
