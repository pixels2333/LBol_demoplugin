---
name: lbol-ui-handbook
description: LBoL (Library of Babel) 原生 UI 架构、设计语言与 MOD 原生风格 UI 页面开发指南。教 AI 如何复用原游戏 UI 组件与范式，设计出与原游戏风格完全一致的 MOD 页面。
---

# LBoL 原生 UI 架构与设计指南 (lbol-ui-handbook)

本 Skill 是针对 **LBoL (Library of Babel)** 游戏的专业 UI 指南，基于对原游戏源码镜像（`lbol` 目录下全部 287 个 UI 相关 C# 文件）的深入全量盘点编制。旨在指导 AI Agent 与 MOD 开发者理解原游戏的 UI 体系结构、组件模型与视觉风格，并利用原游戏 UI 组件与模板技术构建与原游戏高度一致（原汁原味）的 MOD UI 页面。

---

## 0. 原游戏 UI 代码文件全量分布盘点 (Full UI Directory Mapping)

原游戏 UI 相关的代码文件共计 **287 个**，分布在以下模块与子目录中：

| 源码目录路径 | 文件数量 | 模块职责与核心内容 |
| :--- | :--- | :--- |
| `LBoL.Presentation\UI\` | **28** | UI 核心架构、单例 `UiManager`、基类 `UiBase` / `UiPanelBase` / `UiDialogBase`、`PanelLayer`、`TooltipPositioner`、`LocalizationManager` |
| `LBoL.Presentation\UI\Panels\` | **63** | 所有游戏主界面/全屏/半屏面板（如 `PlayBoard`, `MapPanel`, `ShopPanel`, `SelectCardPanel`, `RewardPanel`, `VnPanel`, `SettingPanel` 等） |
| `LBoL.Presentation\UI\Dialogs\` | **9** | 所有弹窗与二次确认框（如 `MessageDialog`, `UpgradeCardDialog`, `RemoveCardDialog`, `TransformCardDialog` 等） |
| `LBoL.Presentation\UI\Widgets\` | **68** | 核心 UI 小部件与控件库（`CardWidget`, `HealthBar`, `CommonButtonWidget`, `TargetSelector`, `IntentionWidget`, `StatusEffectWidget` 等） |
| `LBoL.Presentation\UI\ExtraWidgets\` | **19** | 扩展控件与卡牌专有视图（`HandCard`, `ShopCard`, `ShowingCard`, `DamagePopup`, `TooltipSource` 接口体系） |
| `LBoL.Presentation\UI\Transitions\` | **9** | 页面/弹窗过渡动画（`AnimationTransition`, `SimpleTransition`, `MapTransition`, `GameResultTransition` 等） |
| `LBoL.Presentation\InputSystemExtend\` | **8** | 手柄导航与按键绑定 UI 辅件（`GamepadNavigationManager`, `GamepadButtonTip`, `GamepadCardCursor` 等） |
| `LBoL.Presentation\Effect\` | **5** | UI 飘字、法力飞向与卡牌特效控件（`ManaFlyEffect`, `EffectWidget`, `DamagePopup`, `ExileCoverEffect`） |
| `LBoL.Core\Dialogs\` | **12** | Core 层剧情事件/VN 节点引擎（`DialogRunner`, `DialogProgram`, `DialogOption`, `DialogPhase` 等） |

---

## 1. 原生 UI 视觉规范与设计语言 (Design System & Style)

### 1.1 Canvas 层级管理体系 (`PanelLayer`)
LBoL 使用 `UiManager` 统一管理 Canvas 层级，所有面板与弹窗按 `PanelLayer` 决定渲染与交互顺序：

| 层级名称 (`PanelLayer`) | 适用场景 | 经典代表面板 |
| :--- | :--- | :--- |
| `Base` | 底层全屏背景/视效 | `BackgroundPanel` |
| `Bottom` | 场景底层 UI | `GameRunVisualPanel` |
| `Normal` | 游戏常规全屏/半屏主面板 | `PlayBoard`, `MapPanel`, `ShopPanel`, `StartGamePanel`, `SelectCardPanel`, `RewardPanel` |
| `VisualNovel` | 剧情与事件对话面板 | `VnPanel` |
| `Top` | 阻塞性对话框与二级弹窗 | `MessageDialog`, `UpgradeCardDialog`, `RemoveCardDialog`, `CardDetailPanel` |
| `Topmost` | 顶层通知与遮罩 | `TopMessagePanel`, `LoadingScreen` |
| `Tooltip` | 鼠标悬停悬浮提示 | `TooltipsLayer`, `TooltipWidget`, `EntityTooltipWidget` |

### 1.2 视觉外观与主题规范
- **暗色背景遮罩 (Raycast Blocker)**: 全屏或弹窗背后统一使用 `Color(0f, 0f, 0f, 0.6f ~ 0.85f)` 的半透明黑色遮罩。
- **头像与实体图标规范**:
  - 头像/实体图标采用**圆形/圆角遮罩** (`AvatarMask` + `Mask` 组件)。
  - 边框规范：本地玩家/当前选中目标/高亮实体采用**金色圆角边框**；远程玩家/普通实体采用**白色/灰白圆角边框**。
- **字体与排版**:
  - 统一采用 `TextMeshProUGUI` 控件。
  - 渲染标题时，建议使用 `FontAsset` 预设，配以适度的文字阴影与描边，确保在复杂背景上的可读性。
- **布局锚点规范**:
  - 元素居中锚定使用 `anchorMin = (0.5, 0.5)`, `anchorMax = (0.5, 0.5)`。在居中锚定下，两轴 `sizeDelta` 必须明确指定（例如 `sizeDelta = (100, 100)`），不可置零。

### 1.3 资源加载规范 (`ResourcesHelper`)
原游戏的 UI 资源加载采用内建的 `ResourcesHelper` 静态辅助类：

- **Panel 预制体**: `ResourcesHelper.LoadUiPanelAsync(folder, name)`（对应路径 `Resources/UI/Panels/{folder}/{name}`）
- **Dialog 预制体**: `ResourcesHelper.LoadUiDialogAsync(folder, name)`（对应路径 `Resources/UI/Dialogs/{folder}/{name}`）
- **背景图像 (Addressables)**: `ResourcesHelper.LoadUiBackground(bgName)`（对应 Addressable 路径 `Assets/AA/UI/Background/{bgName}.png`）
- **卡牌背景贴图**: `Resources.Load<Texture2D>("Sprite/Card/Bg/" + rarity + manaColor)`

---

## 2. 核心 UI 框架 API

### 2.1 UiManager 单例 (`LBoL.Presentation.UI.UiManager`)
`UiManager` 是整个 UI 系统的中心大脑，控制面板的异步加载、显示、隐藏和生命周期。

```csharp
// 1. 检查 UI 管理器是否就绪
if (UiManager.IsInitialized)
{
    // 2. 异步加载面板
    var panel = await UiManager.LoadPanelAsync<MyCustomPanel>(show: true);
    
    // 3. 显示/隐藏指定面板
    UiManager.Show<ShopPanel>();
    UiManager.Hide<ShopPanel>(transition: true);
    
    // 4. 获取已加载面板实例
    var selectCardPanel = UiManager.GetPanel<SelectCardPanel>();
    
    // 5. 显示全屏加载指示器
    await UiManager.ShowLoading(duration: 0.5f);
    await UiManager.HideLoading(duration: 0.5f);
}
```

### 2.2 UiPanelBase 基类
所有主界面面板均继承自 `UiPanelBase`（派生自 `UiBase`），它提供了完整的游戏流程生命周期回调：

```csharp
public class MyPanel : UiPanelBase
{
    // 决定该面板挂载在 Canvas 的哪一层
    public override PanelLayer Layer => PanelLayer.Normal;

    // 异步初始化
    public override async UniTask InitializeAsync()
    {
        await base.InitializeAsync();
        // 绑定按钮点击事件、组件查找等
    }

    // 显示与隐藏事件
    protected override void OnShown() { base.OnShown(); }
    protected override void OnHided() { base.OnHided(); }

    // 游戏单局 (GameRun) 生命周期绑定
    protected override void OnEnterGameRun()
    {
        base.OnEnterGameRun();
        // 注册游戏事件 HandleGameRunEvent(GameRun.CardUsed, OnCardUsed);
    }
    
    protected override void OnLeaveGameRun()
    {
        base.OnLeaveGameRun();
        // 自动清理事件句柄
    }
}
```

### 2.3 UiDialogBase 弹窗基类
`UiDialogBase` 用于弹窗式交互（如 `MessageDialog`），支持按键/手柄导航输入栈（`GamepadNavigationManager`）：

- 弹窗显示时会自动被推入 UI 输入焦点栈 (`UiManager.PushActionHandler`)。
- 弹窗关闭时自动恢复下层面板的焦点与交互能力 (`UiManager.PopActionHandler`)。

---

## 3. 全量组件与控件库解析 (Widgets, ExtraWidgets & Effects)

### 3.1 卡牌控件体系 (`CardWidget`, `HandCard`, `ShopCard`, `ShowingCard`)
- **CardWidget**: 负责卡牌外观的完整渲染（包括牌面插画、法术费用、卡牌名字、类型描述、稀有度边框、克隆标记、闪卡特效）。
  - `CardWidget.SetCard(Card card)`：绑定逻辑卡牌。
  - `CardWidget.SetHighlight(bool highlight)`：边缘发光高亮。
- **HandCard**: 专用于战斗中手牌的手牌弧形扇形排列与拖拽投掷控制。
- **ShopCard**: 包含金币价格标签与已售罄 (`SoldOut`) 覆盖层标记。

### 3.2 基础交互控件 (`CommonButtonWidget` & `CommonToggleWidget`)
- **CommonButtonWidget**: 原生精美按钮，自带音效、悬停缩放、点击动画与 Disabled 灰化状态。
  - `button.OnClick.AddListener(Action action)`
  - `button.SetText(string text)`
  - `button.Interactable = bool`
- **CommonToggleWidget**: 原生开关控件，包含 Checkmark 状态切换动画与文本 Label。

### 3.3 实体 HUD 与战斗控件 (`HealthBar`, `StatusEffectWidget`, `IntentionWidget`, `TargetSelector`)
- **HealthBar**: 支持生命值/护盾值/格挡值的平滑过渡进度条，支持伤害数值漂浮。
- **StatusEffectWidget**: 显示 Buff/Debuff 图标与层数数字 (`CountText`)。
- **IntentionWidget**: 敌方单位意图（攻击、防御、施法）图标与数值浮标。
- **TargetSelector**: 选卡或指向性技能时的弧形贝塞尔拉线 targeting 线。

### 3.4 UI 动画与飞向特效 (`UiTransition` & `Effect`)
- **Transitions 模块 (9 文件)**: `AnimationTransition`, `SimpleTransition`, `MapTransition` 等，负责面板/弹窗切入切出的 DoTween / Scale / Alpha 动画。
- **ManaFlyEffect / DamagePopup**: 负责能量球吸收飞向 UI、伤害数字弹出的物理轨迹。

### 3.5 自动对齐 Tooltip 提示系统 (`TooltipWidget` & `TooltipPositioner`)
- 实现 `ICardTooltipSource` 或 `ITooltipSource` 接口即可自动触发 `TooltipsLayer`。
- `TooltipPositioner` 自动计算 `Up`, `Down`, `Left`, `Right` 最佳展示方位，自动适应屏幕边界。

---

## 4. 剧情与事件对话系统 (`LBoL.Core.Dialogs` & `VnPanel`)

LBoL 拥有独立的事件对话/视觉小说 (VN) 系统：
- **LBoL.Core.Dialogs (12 文件)**: 包含 `DialogRunner`（剧本执行器）、`DialogProgram`（剧情程序）、`DialogOption`（选项）、`DialogPhase`（相位状态机）。
- **Presentation 层 `VnPanel`**: 负责立绘淡入淡出、对话文本逐字打印、分支选项按钮组（`DialogButtons`）渲染与配音播放。

---

## 5. 经典页面与弹窗蓝图范式 (UI Blueprints)

### 5.1 全屏选择/交易/展示面板蓝图 (Full-Screen Panel Blueprint)
结构参考 `ShopPanel` / `SelectCardPanel`：
```
[Root: RectTransform (Full Canvas)]
  ├── RaycastBlocker (Image, Color: Black 70%)
  ├── MainFrame (Image Border + Title Bar)
  │     ├── TitleText (TextMeshProUGUI, Size 28, Gold)
  │     ├── SubTitleText/StatusText (TextMeshProUGUI, Size 18)
  │     ├── ContentContainer (Grid/Horizontal Layout)
  │     │     ├── LeftArea (Items/Cards)
  │     │     └── RightArea (Details/Preview)
  │     └── ActionButtonGroup (Horizontal Layout)
  │           ├── ConfirmButton (CommonButtonWidget, Gold)
  │           └── CancelButton (CommonButtonWidget, Gray)
  └── TooltipLayerHolder
```

### 5.2 二级对话框蓝图 (Dialog Blueprint)
结构参考 `MessageDialog` / `UpgradeCardDialog`：
```
[Root: RectTransform (Full Canvas, Top Layer)]
  ├── Blocker (Image, Color: Black 60%)
  └── DialogContainer (Size: 600x400, Centered)
        ├── Background (MessageDialog Frame Sprite)
        ├── Icon (Optional Warning/Info Sprite)
        ├── MessageText (TextMeshProUGUI, Centered)
        └── ButtonRow (Horizontal Layout)
              ├── Button1 (Confirm)
              └── Button2 (Cancel)
```

---

## 6. 动态创建原生风格 MOD 页面的模版技术 (Runtime Template Factory)

在纯代码 MOD 环境中，最佳实现方式是**从原游戏现有 Prefab 中复制提取 UI 模板组件**。

```csharp
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Widgets;

public static class NativeModUiFactory
{
    public static GameObject CreateNativeStylePanel(string panelName, string title, Action onConfirm, Action onCancel)
    {
        // 1. 从原游戏 MessageDialog 中提取最正统的 TextMeshProUGUI 字体与 Frame 预设
        GameObject messageDialogPrefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
        TextMeshProUGUI textTemplate = messageDialogPrefab?.GetComponentInChildren<TextMeshProUGUI>(true);
        
        // 2. 尝试提取原生的 CommonButtonWidget 按钮模板
        CommonButtonWidget buttonTemplate = UnityEngine.Object.FindObjectsByType<CommonButtonWidget>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(b => b != null && b.gameObject.activeInHierarchy);

        // 3. 构建 Root 容器
        GameObject root = new GameObject(panelName);
        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        // 4. 添加 Raycast 遮罩
        Image blocker = root.AddComponent<Image>();
        blocker.color = new Color(0f, 0f, 0f, 0.75f);
        blocker.raycastTarget = true;

        // 5. 实例化标题文本
        if (textTemplate != null)
        {
            TextMeshProUGUI titleText = UnityEngine.Object.Instantiate(textTemplate, root.transform);
            titleText.text = title;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontSize = 26;
            RectTransform titleRect = titleText.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 0.85f);
            titleRect.anchorMax = new Vector2(0.5f, 0.85f);
            titleRect.sizeDelta = new Vector2(600, 50);
        }

        // 6. 实例化原生按钮
        if (buttonTemplate != null)
        {
            CommonButtonWidget confirmBtn = UnityEngine.Object.Instantiate(buttonTemplate, root.transform);
            confirmBtn.SetText("确认");
            confirmBtn.OnClick.AddListener(() => onConfirm?.Invoke());
            RectTransform confirmRect = confirmBtn.GetComponent<RectTransform>();
            confirmRect.anchorMin = new Vector2(0.4f, 0.2f);
            confirmRect.anchorMax = new Vector2(0.4f, 0.2f);

            CommonButtonWidget cancelBtn = UnityEngine.Object.Instantiate(buttonTemplate, root.transform);
            cancelBtn.SetText("取消");
            cancelBtn.OnClick.AddListener(() => onCancel?.Invoke());
            RectTransform cancelRect = cancelBtn.GetComponent<RectTransform>();
            cancelRect.anchorMin = new Vector2(0.6f, 0.2f);
            cancelRect.anchorMax = new Vector2(0.6f, 0.2f);
        }

        return root;
    }
}
```

---

## 7. MOD UI 开发关键规则与避坑指南

> [!CAUTION]
> 违反以下规则可能导致 UI 闪烁、卡死、画面错位或跨线程崩溃：

1. **绝对主线程原则 (`Plugin.RunOnMainThread`)**:
   - Harmony 补丁或网络 UDP 消息到达时，可能在非 UI 线程触发回调。**所有 UI 创建、销毁、显显与赋值必须包裹在 `Plugin.RunOnMainThread(() => { ... })` 中**。

2. **居中锚定坐标逻辑 (`sizeDelta`)**:
   - 当 `anchorMin = (0.5, 0.5)` 且 `anchorMax = (0.5, 0.5)` 时，`sizeDelta.x` 和 `sizeDelta.y` 决定元素的真实宽高。**若宽高设为 0，UI 将完全不可见**。

3. **动态 ID 与缓存生命周期管理**:
   - 如果为多名玩家或动态实体生成头像/卡牌图标，在实体变化或离场时，**必须主动销毁（Destroy）旧节点并清理缓存引用**，防止内存泄漏和重叠错位。

4. **原生组件防破坏法则**:
   - 通过 Harmony 补丁修改原游戏 UI 时，优先采用追加子节点或调用原公开 API 的形式。**避免直接 `Destroy` 原游戏核心 UI 节点**，建议使用 `SetActive(false)` 或 `CanvasGroup.alpha = 0`。

5. **避免硬编码字面量**:
   - 按钮文本、提示语使用 `LocalizationManager` 或 MOD 自身的 `ConfigManager` 进行统一维护，以适应多语言。

---

## 8. 总结：AI MOD UI 设计三步法

1. **第一步（选型）**: 确定新 UI 的性质（全屏面板 / 阻塞弹窗 / 浮动 HUD），选择对应 `PanelLayer` 与继承基类。
2. **第二步（复用）**: 优先使用 `ResourcesHelper` 或运行时模板提取（`GapSharedPanelTemplateFactory`）复用原生的框架、遮罩、字体与 `CommonButtonWidget` / `CardWidget`。
3. **第三步（规范）**: 严格按照 `(0.5, 0.5)` 锚点、金色/白色圆角边框、主线程安全调度与响应式布局进行构建，实现 100% 原汁原味的 LBoL 视觉与交互体验。
