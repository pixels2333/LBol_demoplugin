# 任务清单: trade_runtime_selection_panels

目录: `helloagents/plan/202604191820_trade-runtime-selection-panels/`

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
已完成: 6
完成率: 75%
```

---

## 任务列表

### 1. 方案与基础设施

- [√] 1.1 新增共享运行时选择面板构造器
  - 目标: 提供遮罩、对话框框架、滚动列表、按钮区的独立运行时结构
  - 验收: 不依赖 `HistoryPanel`

- [√] 1.2 为共享构造器提供与原版风格一致的模板复用策略
  - 目标: 复用 `MessageDialog` / `CommonButtonWidget` / `TextMeshProUGUI`
  - 验收: 不引入新 prefab 文件

### 2. TradePanel 改造

- [√] 2.1 改造 `PickCards`：改用独立运行时卡牌选择面板
  - 验收: 选择卡牌后刷新交易报价，并可关闭返回

- [√] 2.2 改造 `PickExhibits`：改用独立运行时展品选择面板
  - 验收: 可勾选/取消展品并正确同步本地报价

- [√] 2.3 移除 `TradePanel` 对 `HistoryPanel` picker 骨架的硬依赖
  - 验收: 相关警告日志不再因 picker 构建失败而出现

### 3. 治疗列表统一

- [√] 3.1 统一治疗面板运行时列表构建思路
  - 验收: `ResurrectPanelRuntimeFactory` 复用同类运行时列表结构，不再保留风格分裂

### 4. 回归与验证

- [ ] 4.1 构建验证：`dotnet build networkplugin/NetWorkPlugin.csproj -v minimal -p:LangVersion=preview -clp:ErrorsOnly`
  > 备注: 本轮会话的命令执行能力不可用，未能直接跑构建命令；已用工程级错误检查替代兜底。

- [ ] 4.2 手动验收：按 proposal.md 的“手动验证清单”逐项确认

---

## 备注

- 本方案优先达成“真正独立可用的运行时选择面板”，不追求一次性把整个交易系统完全迁移成新的 Screen 框架。
- 若本次共享构造器稳定，可在后续继续推广到更多运行时 UI 场景。
