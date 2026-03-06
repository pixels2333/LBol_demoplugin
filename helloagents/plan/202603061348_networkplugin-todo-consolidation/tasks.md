# 任务清单: networkplugin-todo-consolidation

目录: `helloagents/plan/202603061348_networkplugin-todo-consolidation/`

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
已完成: 5
已跳过: 0
待确认: 3
完成率: 62.5%
扫描范围: `networkplugin/**`
命中 TODO: 1
```

---

## 任务列表

### 1. `ShopTradeIconPatch` 文案目标收敛（P1）

- [√] 1.1 在 `networkplugin/Patch/UI/ShopTradeIconPatch.cs` 的 `EnsureUi(ShopPanel shopPanel)` 中移除默认的“双 `foreach` 全量改文案”实现
  - 目标: 删除对 `midGo.GetComponentsInChildren<TMP_Text>(true)` 与 `midGo.GetComponentsInChildren<Text>(true)` 的无差别遍历写入
  - 验证: 已改为 `ApplyTradeButtonLabel(midGo, tradeButton, TradeButtonLabelText)` 单点调用，源码中不再存在对应全量覆盖逻辑

- [√] 1.2 在 `networkplugin/Patch/UI/ShopTradeIconPatch.cs` 中新增主标签解析 helper
  - 目标: 新增类似 `TryResolveTradeLabel(...)` 的私有方法，优先从 `tradeButton` 子树解析主标题 `TMP_Text`，找不到时再回退到 `Text`
  - 验证: 已新增 `TryResolveTradeLabel(...)`、`SelectTradeTmpLabel(...)`、`SelectTradeLegacyLabel(...)`

- [√] 1.3 在 `networkplugin/Patch/UI/ShopTradeIconPatch.cs` 中新增文案应用 helper，并把 `玩家交易` 写入动作迁入该 helper
  - 目标: 将“主路径写文案”和“fallback 写文案”统一封装，避免 `EnsureUi(...)` 内继续散落文本改写逻辑
  - 依赖: 1.2
  - 验证: 已新增 `ApplyTradeButtonLabel(...)`，并将按钮文案常量提取为 `TradeButtonLabelText`

- [√] 1.4 将 legacy `Text` 支持从默认路径降级为 fallback 路径
  - 目标: 只有在未找到有效 `TMP_Text` 时，才尝试命中 `UnityEngine.UI.Text`；fallback 触发时记录 warning
  - 依赖: 1.2
  - 验证: 仅在 `TMP_Text` 未命中时才使用 `legacyLabel`，并输出 `[ShopTradeIcon] 使用 legacy Text 回退...` warning

- [√] 1.5 调整 `LogHierarchy(midGo.transform, 0)` 的触发时机
  - 目标: 将层级打印改为“标签解析失败 / 多候选冲突 / 显式调试开关”时触发
  - 依赖: 1.2
  - 验证: `LogHierarchy(...)` 已移出正常主路径，仅在 fallback / 多候选 / 未命中时触发

### 2. UI 行为回归验证（P1）

- [?] 2.1 在商店界面验证克隆交易按钮的标题文案
  - 文件: `networkplugin/Patch/UI/ShopTradeIconPatch.cs`
  - 目标: 确认按钮文案仍显示 `玩家交易`，按钮样式沿用 `CardService` 克隆样式
  - 验证: 打开商店 UI，观察按钮标题和点击行为
  - 备注: 当前会话无游戏内商店运行环境，待手工回归

- [?] 2.2 验证原生按钮未被误改
  - 文件: `networkplugin/Patch/UI/ShopTradeIconPatch.cs`
  - 目标: 确认 `CardService` 与 `ReturnButton` 原有文本保持不变，未被新的标签解析逻辑误伤
  - 依赖: 2.1
  - 验证: 同屏检查三个按钮的最终显示文本
  - 备注: 当前会话无游戏内商店运行环境，待手工回归

- [?] 2.3 验证日志收敛效果
  - 文件: `networkplugin/Patch/UI/ShopTradeIconPatch.cs`
  - 目标: 正常路径不再刷出完整 hierarchy dump；异常路径仍能留下可排障日志
  - 依赖: 1.5
  - 验证: 观察商店 UI 构建期日志，仅在异常场景命中 warning / hierarchy 输出
  - 备注: 已完成静态验证与构建验证，运行时日志形态待手工回归

---

## 执行备注

> 执行过程中的重要记录

| 任务 | 状态 | 备注 |
|------|------|------|
| 扫描 | completed | 已用精确 `//TODO:` 搜索确认 `networkplugin/` 下仅 1 处命中 |
| 1.x | completed | `ShopTradeIconPatch` 已改为“主标签解析 + legacy fallback + 条件 hierarchy dump”结构，源码中已无该 TODO |
| 2.x | uncertain | 需要在游戏内打开商店 UI 做最终回归，静态检查与构建成功无法替代运行时层级验证 |
| 验证 | completed | `dotnet build networkplugin/NetWorkPlugin.csproj -v minimal` 通过（257 warnings，0 errors） |
