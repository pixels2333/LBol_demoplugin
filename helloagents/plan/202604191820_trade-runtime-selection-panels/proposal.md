# 变更提案: trade_runtime_selection_panels

## 元信息
```yaml
类型: 改进
方案类型: implementation
优先级: P1
状态: 待执行
创建: 2026-04-19
```

---

## 1. 需求

### 背景
当前 `TradePanel` 的 `PickCards` / `PickExhibits` 入口已经接线，但其具体选择界面仍依赖 `HistoryPanel` 作为滚动列表骨架。一旦运行环境里拿不到 `UI/Panels/HistoryPanel` 资源，也找不到场景里的现成实例，交易选择界面就会直接提示“不可用”，无法稳定工作。

用户要求：
- 为 `PickCards` 和 `PickExhibits` 改成**真正独立可用**的运行时选择面板；
- 整体设计尽量参考 `TogetherInSpire` 的独立选择屏幕思路；
- 视觉尽量贴近游戏原版；
- 顺手统一治疗列表的运行时风格与结构。

### 目标
- `PickCards` 打开独立运行时卡牌选择面板，不依赖 `HistoryPanel`。
- `PickExhibits` 打开独立运行时展品选择面板，不依赖 `HistoryPanel`。
- 选择面板具备明确的遮罩、标题、滚动列表、关闭/确认路径，并能稳定恢复 `TradePanel` 交互状态。
- 治疗列表构建方式与交易运行时列表共享一致的 UI 思路与基础构件。

### 非目标
- 不重写交易网络协议与确认逻辑。
- 不引入新的外部 prefab / 资源文件。
- 不把整个交易系统彻底迁移到新的 `UiDialog`/Screen 体系。

### 约束条件
```yaml
交互约束:
  - 选择面板必须阻断底层 TradePanel 细节区交互
  - 关闭/确认后必须恢复主面板可交互状态
兼容性约束:
  - 不能依赖 HistoryPanel 资源存在
  - 仍需兼容当前运行时克隆 MessageDialog/CommonButton/TMP 的策略
视觉约束:
  - 优先复用游戏内 MessageDialog、TMP、CommonButtonWidget 风格
```

### 完成标准（验收）
- [ ] 点击 `PickCards` 时稳定弹出卡牌选择面板，并能选择卡牌加入交易。
- [ ] 点击 `PickExhibits` 时稳定弹出展品选择面板，并能选择/取消展品报价。
- [ ] 不再出现 `未找到 HistoryPanel 资源或场景实例` 导致的选择器不可用日志。
- [ ] 关闭/确认选择面板后，`TradePanel` 主界面与按钮恢复正常交互。
- [ ] 治疗列表使用与交易运行时列表一致的面板/列表构造思路。
- [ ] `dotnet build networkplugin/NetWorkPlugin.csproj -v minimal -p:LangVersion=preview -clp:ErrorsOnly` 通过。

---

## 2. 现状分析

### 2.1 TradePanel 当前问题
- 文件：`networkplugin/UI/Panels/TradePanel.cs`
- `EnsureCardPickerOverlay()` 与 `EnsureExhibitPickerOverlay()` 当前都调用 `TryAttachHistoryListWithRecordRow()` 来获取列表骨架。
- 这条链路对 `HistoryPanel` 有硬依赖，因此不满足“独立运行时面板”的要求。

### 2.2 TogetherInSpire 的可借鉴点
- 交易与治疗入口走 `ScreenManager.Open(...)`，核心优点是：
  - 面板职责独立；
  - 选择器不依赖某个临时场景 prefab 是否存在；
  - 打开/关闭路径明确。
- 本次不完全照搬其 Screen 框架，但借鉴其“独立选择界面”的职责划分。

---

## 3. 方案选型

### 方案 A（推荐）：在 `TradePanel` 内引入独立运行时选择面板构造器
新增一套通用运行时选择面板构建逻辑（遮罩 + MessageDialog 风格窗口 + ScrollRect + 行模板），让卡牌、展品、治疗列表都能基于它创建独立面板。

- 优点：
  - 不依赖 `HistoryPanel`；
  - 可在当前 `TradePanel` / `ResurrectPanelRuntimeFactory` 结构内最小接入；
  - 风格可复用 `MessageDialog`、`TMP`、`CommonButtonWidget`，贴近原版视觉。
- 缺点：
  - 需要补一层运行时 UI 工具代码；
  - 需梳理 TradePanel 内现有 picker 生命周期。

### 方案 B：完全抽成新的独立 `UiDialog` / Screen
为卡牌和展品分别创建独立 dialog 类，主面板只负责打开与接收结果。

- 优点：职责最清晰，更接近 `TogetherInSpire`。
- 缺点：改动范围更大，当前任务会明显扩散到 dialog 生命周期与更多跨类同步。

**决策**：采用方案 A。
原因：它已经满足“真正独立可用的运行时选择面板”目标，同时保持对现有 `TradePanel` 交互链路的兼容，风险最低。

---

## 4. 技术设计

### 4.1 新的运行时选择面板基础结构
建议新增一个共享运行时工厂/构造器，产出以下结构：
- Fullscreen Root（半透明遮罩，阻挡输入）
- Dialog Frame（优先克隆 `UI/Dialogs/MessageDialog`）
- Title / Optional Status
- ScrollRect + Viewport + Content
- Footer Buttons（关闭 / 确认，可按场景裁剪）

### 4.2 Card / Exhibit picker 的接入方式
- `TradePanel` 保留 `ShowCardPickerOverlay()` / `ShowExhibitPickerOverlay()` 公开行为；
- 但其内部不再尝试挂 `HistoryPanel`；
- 改为基于共享运行时选择面板直接创建 Content 容器与行条目。

### 4.3 行条目策略
- 卡牌条目：复用 TMP + Button + 简单图像/文本布局，展示 `Name / Id`；
- 展品条目：展示 `Name / Id`，有图标时显示图标；
- 治疗条目：继续使用现有“玩家名 + HP + 治疗收益”文案，但列表容器改为共享运行时风格。

### 4.4 生命周期
- 打开 picker：
  - 面板置顶；
  - 隐藏主交易详情区；
  - 只允许当前 picker 接收输入。
- 关闭 picker：
  - 恢复主交易详情区；
  - 恢复 `TradePanel` 交互；
  - 根据场景决定是否发送 `TrySendOfferUpdate()`。

---

## 5. 风险与对策

| 风险 | 等级 | 描述 | 对策 |
|------|------|------|------|
| 运行时按钮模板多态 | 中 | `CommonButtonWidget` 模板层级复杂，直接克隆可能带出多余子按钮 | 复用现有 `DisableExtraButtons` / `DisableTooltipBehaviours` 逻辑 |
| 列表样式不一致 | 中 | 交易/治疗原本构造方式不同 | 抽共享面板骨架，允许各自保留条目内容差异 |
| 主面板交互残留 | 中 | 关闭 picker 后可能忘记恢复主界面 | 统一封装 show/hide 生命周期与恢复逻辑 |

---

## 6. 手动验证清单

- 场景 1：Gap 进入交易 -> 打开 `PickCards`
  - 期望：弹出独立卡牌选择面板，能加入卡牌并返回交易主面板。

- 场景 2：Gap 进入交易 -> 打开 `PickExhibits`
  - 期望：弹出独立展品选择面板，能勾选/取消展品并返回交易主面板。

- 场景 3：关闭路径
  - 期望：取消/关闭后主交易面板恢复正常，不残留不可交互状态。

- 场景 4：治疗面板
  - 期望：治疗玩家列表仍能正常显示，并维持统一的运行时列表风格。
