# 变更提案: trade_partner_picker_centered_ui

## 元信息
```yaml
类型: 改进
方案类型: implementation
优先级: P1
状态: 待执行
创建: 2026-02-07
```

---

## 1. 需求

### 背景
当前 TradePanel 内置的“选择交易对象（partner picker）”在视觉上仍偏“临时拼装 UI”：
- 缺少清晰的“居中窗口容器/边框层次”，更像全屏遮罩上的一组按钮。
- 虽已复用部分游戏资源（背景/按钮模板/字体），但整体观感与游戏常见的居中弹层/对话框仍有差距。

用户期望：当打开交易并需要选择交易对象时，partner picker **使用游戏里的 UI 资源**，并把玩家列表以**屏幕居中弹层窗口**展示（而不是右上角信息框/调试框风格）。

### 目标
- 打开交易（需要选择伙伴）时，TradePanel 的 partner picker 以**居中窗口**展示玩家列表：
  - 视觉资源尽量复用游戏 UI：窗口背景/边框风格、字体、按钮皮肤、列表间距。
  - 列表项包含头像/名字/所在地点信息（用于“同节点优先”的可解释性）。
- 弹层期间阻挡底层输入，并提供明确的取消/关闭路径。
- 与现有数据源兼容：列表仍来自“远程玩家快照”（含离线调试 `aidefault`）。

### 非目标（避免范围膨胀）
- 不重做交易面板整体布局与交易逻辑（仅优化“选择交易对象”的列表展示与交互）。
- 不引入新的 prefab 资源文件（优先运行时克隆/拼装，保持插件分发简单）。
- 不改变网络协议与玩家列表同步语义。

### 约束条件
```yaml
性能约束:
  - 不应在每帧频繁创建/销毁大量 UI 对象
  - 列表重建应可控（仅在打开/玩家列表变化/刷新按钮时）
兼容性约束:
  - 需要在未连接/离线调试模式也可展示
  - 不依赖 Unity Editor 资源或 Addressables 额外配置
交互约束:
  - 弹层期间应禁用底层 TradePanel 的交互
```

### 完成标准（验收）
- [ ] 在 TradePanel 触发“选择交易对象”时，玩家列表以**屏幕居中窗口弹层**展示。
- [ ] 弹层 UI 资源来源于游戏内模板/资源（窗口背景/按钮/字体），整体视觉风格与游戏一致。
- [ ] 弹层可取消关闭，且关闭后 TradePanel 不会遗留不可交互状态。
- [ ] 离线调试：开启虚拟玩家时，列表能显示 `aidefault`，并正确标注位置/同节点。
- [ ] `dotnet build networkplugin/NetWorkPlugin.csproj -c Release` 通过（允许既有 Harmony003 warnings）。

---

## 2. 现状分析（与目标差距）

### 2.1 交易对象选择列表（TradePanel 内）
- 模块：`networkplugin/UI/Panels/TradePanel.cs`
- 行为：当未指定 partner 时，TradePanel 内部会显示一个 partner picker overlay，并按 LocationName 做 shop-like 过滤。
- 问题：当前 partner picker 与游戏 UI 的“居中弹层窗口”一致性仍可进一步提升：缺少明显的窗口容器层次/边框感，列表区也缺少滚动与可预期的最大尺寸。

> 注：项目中另有“右上角常驻玩家信息框”（`OtherPlayersOverlayPatch`）。本方案**不以它为改造目标**，避免范围膨胀；如后续仍发生 UI 混淆，再单独开方案处理。

---

## 3. 方案选型

### 方案 A（推荐）：改造 TradePanel 现有 partner picker 为“居中窗口弹层”
直接在 `TradePanel.EnsurePartnerPickerOverlay()` 里把 overlay 的内容结构升级为：
- Fullscreen 遮罩（负责阻挡输入）
- CenterWindow（居中固定最大尺寸的窗口容器：背景/边框/标题/列表/底部按钮）
- 列表使用 ScrollRect（候选多时可滚动）

- 优点：改动集中在 TradePanel，语义正确（选择器就是 modal），不引入新 prefab。
- 缺点：TradePanel 文件较大，需注意封装与可读性。

### 方案 B：把 partner picker 抽成独立 Dialog/Factory（后续演进）
新增一个 runtime factory（例如 `TradePartnerPickerDialogRuntimeFactory`），把创建/刷新/销毁迁出 TradePanel。

- 优点：复用性更好，TradePanel 更干净。
- 缺点：初期工程量略大，且需要处理与 TradeDetailDialog 的协同。

**决策**：采用方案 A；在代码层尽量把 UI 构建拆成小函数，为未来升级到方案 B 做准备。

---

## 4. 技术设计（方案 B）

> 本章节的技术设计以“方案 A（推荐）”为准：改造 TradePanel 现有 partner picker。

### 4.1 结构设计（TradePanel 内）
保留 `_partnerPickerRoot` 作为 Fullscreen 遮罩层（用于阻挡输入），并新增一个子节点 `CenterWindow`：

- `TradePartnerPicker`（fullscreen overlay）
  - Image：半透明遮罩（raycastTarget=true）
  - Child：`CenterWindow`
    - 背景/边框：优先复用游戏资源；失败则用半透明底色 + Outline
    - Title：复用 TMP 模板
    - ScrollRect + Content（VerticalLayoutGroup）
    - Footer：取消按钮（复用 `CommonButtonWidget` 模板）

数据源仍来自：`OtherPlayersOverlayPatch.SnapshotPlayersDetailed()`（含 `CharacterId/LocationName/坐标`）。

### 4.2 UI 资源复用策略
优先级从高到低：
1) **克隆现成面板上的控件模板**
   - 按钮：`CommonButtonWidget`（TradePanel 已有 confirm/cancel，可作为模板）
   - 文本：`TextMeshProUGUI`（从面板已有文字控件取字体/材质）
2) **加载游戏内背景资源**
   - 背景图：`ResourcesHelper.LoadUiBackground("Adventure")` 或与 Shop/Gap 统一的背景 key
3) **兜底**
   - 若资源加载失败：使用半透明遮罩 + 简单窗口底色

### 4.3 布局要点
- `CenterWindow`：anchor/pivot 在屏幕中心；尺寸建议 900x650，并限制最大宽高（小分辨率下按比例缩放）。
- List：ScrollRect（避免玩家多时越界）；Content 使用 VerticalLayoutGroup。
- Item：复用 `CommonButtonWidget` 皮肤，但内部内容改为“头像 + 两行文字”。

### 4.4 生命周期
- 打开 partner picker 时：TradePanel 底层细节隐藏/禁用交互（保持现有 `SetTradeDetailsVisible(false)` 和 `CanvasGroup.interactable=false`）。
- 关闭 partner picker 时：恢复 TradePanel 可交互状态。

### 4.5 列表刷新策略
- 默认只在打开 overlay 时 rebuild。
- 可选：增加“刷新”按钮（在列表顶部/右上角），用于手动重建候选。
- 对象池：条目复用，避免频繁 Destroy/Instantiate。

---

## 5. 风险与对策

| 风险 | 等级 | 描述 | 对策 |
|------|------|------|------|
| UI 模板在不同场景不可用 | 中 | TradePanel 可能为运行时工厂创建，某些模板为空 | 资源获取多级降级；最差用纯运行时 UI 兜底 |
| 右上角 overlay 与 modal 争抢层级 | 中 | 同时显示导致遮挡/点击穿透 | 引入 suppress 标记；modal 使用更高层级 parent（topmostLayer） |
| 玩家位置字段不全 | 低 | LocationName/坐标可能为 -1/null | UI 文案兜底（“位置未知”），同节点标记仅在可比较时展示 |

---

## 6. 手动验证清单

- 场景 1：联机、在商店打开交易
  - 期望：出现居中玩家选择弹层；列表包含远端玩家；选择后进入交易详情。

- 场景 2：离线调试（虚拟玩家）
  - 开启调试开关后进入商店打开交易
  - 期望：列表出现 `aidefault`；显示头像（同角色）与位置；同节点标记正确。

- 场景 3：取消/关闭路径
  - 点击取消/返回
  - 期望：弹层关闭，TradePanel 正常退出或回到可交互状态；无残留 UI。
