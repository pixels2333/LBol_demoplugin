# 任务清单: trade_partner_picker_centered_ui

目录: `helloagents/plan/202602071900_trade-partner-picker-centered-ui/`

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
总任务: 10
已完成: 2
完成率: 20%
```

---

## 任务列表

### 1. TradePanel partner picker UI 改造

- [√] 1.1 在 `networkplugin/UI/Panels/TradePanel.cs` 中改造 `EnsurePartnerPickerOverlay()`：新增居中 `CenterWindow` 容器
  - 结构: Fullscreen 遮罩（阻挡输入） + CenterWindow（窗口背景/标题/列表/底部按钮）
  - 验收: 列表视觉中心明确，尺寸可控（不再像全屏上一组按钮）

- [√] 1.2 将列表改为 ScrollRect + VerticalLayoutGroup（候选多时可滚动）
  - 验收: 玩家数量超过窗口高度时可滚动，不溢出屏幕

- [ ] 1.3 列表项样式：头像 + 名称 + 位置（同节点标记）
- [√] 1.3 列表项样式：头像 + 名称 + 位置（同节点标记）
- [√] 1.3 列表项样式：头像 + 名称 + 位置（同节点标记）
  - 头像: `ResourcesHelper.LoadCharacterAvatarSprite(characterId)`
  - 文案: locationName/Act/(x,y) 兜底

- [√] 1.5 严格模式：禁用 fallback/兜底策略，列表与空态均复用游戏 UI Prefab
  - 列表 ScrollRect: 复用 `UI/Panels/HistoryPanel` 的 `listScrollRect/listContent`
  - 行模板: 复用 `RecordRow` 模板
  - 空态提示: 复用 `MessageDialog` 的 subText

- [ ] 1.4 条目对象池（可选，但建议）
  - 验收: 多次打开/刷新 partner picker 不应产生明显 GC 峰值

### 2. 离线调试与可观测性

- [ ] 2.1 离线调试开关下：`aidefault` 必须出现在 partner picker 列表中

- [ ] 2.2 关键路径日志（Info 级别，少量）
  - partner picker 打开/关闭
  - 列表刷新次数与候选数量

### 3. 验证

- [√] 3.1 构建验证：`dotnet build networkplugin/NetWorkPlugin.csproj -c Release`
- [√] 3.1 构建验证：`dotnet build networkplugin/NetWorkPlugin.csproj -c Release`

- [√] 3.3 视觉一致性（严格）：partner picker 弹层使用游戏 Dialog Prefab（MessageDialog）作为遮罩/窗口框架
  - 验收: 不再出现运行时纯色遮罩 + Outline 边框的“调试风”窗口

- [ ] 3.2 手动验收：按 proposal.md 的“手动验证清单”逐项确认

---

## 备注

- 本方案只覆盖 TradePanel 内的 partner picker，不以右上角常驻 overlay 为改造目标。
