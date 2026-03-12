using System;
using System.Linq;
using System.Reflection;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Widgets;
using NetworkPlugin.Patch.UI;
using NetworkPlugin.UI.Panels;
using NetworkPlugin.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Factories;

internal static class TradePanelRuntimeFactory
{
    private const string RuntimeRootName = "NetworkPlugin_TradePanel";
    private const string RuntimeUiVersion = "2026-02-14-ui-v6";

    internal static TradePanel GetOrCreate(Transform preferredParent)
    {
        try
        {
            // 复用现有的TradePanel实例（仅保留一个激活实例，避免界面堆叠）。
            TradePanel[] existingPanels = UnityEngine.Object.FindObjectsByType<TradePanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (existingPanels.Length > 0)
            {
                // 如果存在非运行时创建（prefab已绑定）的TradePanel，则优先复用它。
                TradePanel prefabPanel = null;
                foreach (var p in existingPanels)
                {
                    if (!IsRuntimeCreatedPanel(p))
                    {
                        prefabPanel = p;
                        break;
                    }
                }

                // 否则仅复用当前版本的运行时TradePanel。
                TradePanel currentRuntime = null;
                foreach (var p in existingPanels)
                {
                    if (IsCurrentRuntimePanel(p))
                    {
                        currentRuntime = p;
                        break;
                    }
                }

                if (prefabPanel != null)
                {
                    foreach (var p in existingPanels)
                    {
                        if (ReferenceEquals(p, prefabPanel))
                        {
                            continue;
                        }

                        if (IsRuntimeCreatedPanel(p))
                        {
					p.gameObject.SetActive(false);
                        }
                    }

                    Plugin.Logger?.LogInfo("[TradePanelRuntimeFactory] Reusing prefab-wired TradePanel.");
                    return prefabPanel;
                }

                if (currentRuntime != null)
                {
                    foreach (var p in existingPanels)
                    {
                        if (ReferenceEquals(p, currentRuntime))
                        {
                            continue;
                        }

                        p.gameObject.SetActive(false);
                    }

                    Plugin.Logger?.LogInfo($"[TradePanelRuntimeFactory] Reusing runtime TradePanel (version={RuntimeUiVersion}).");
                    return currentRuntime;
                }

                // 旧版本运行时面板可能仍留在内存中，并保留旧的“矩形”外观。
                // 先销毁它们，确保下一次创建时使用最新的UI代码。
                foreach (var p in existingPanels)
                {
                    if (!IsRuntimeCreatedPanel(p))
                    {
                        continue;
                    }

                    UnityEngine.Object.Destroy(p.gameObject);
                }

                Plugin.Logger?.LogInfo("[TradePanelRuntimeFactory] Destroyed old runtime TradePanel(s); rebuilding with latest UI.");
            }

            if (!UiManager.IsInitialized)
            {
                TradeUiMessages.ShowTradePanelMissing();
                return null;
            }

            // 查找一个游戏内风格的按钮模板（CommonButtonWidget）用于克隆。
            // 优先从原生prefab中提取，避免误选到我们自己旧的运行时按钮。
            CommonButtonWidget confirmTemplate = TryPickButtonTemplate(preferConfirm: true);
            CommonButtonWidget cancelTemplate = TryPickButtonTemplate(preferConfirm: false);

            if (confirmTemplate == null)
            {
                TradeUiMessages.ShowTopMessage("交易界面不可用：未找到可复用的按钮模板。请先进入游戏内 UI（例如商店/间隙）。");
                return null;
            }

            // 降级处理：如果找不到专用的取消按钮模板，就复用确认按钮模板。
            cancelTemplate ??= confirmTemplate;

            TextMeshProUGUI textTemplate = TryPickTextTemplate();

            Transform parent = preferredParent;
            if (parent == null)
            {
                // 尽力降级：挂到任意激活Canvas下，确保UI可以正常渲染。
                var canvas = UnityEngine.Object.FindObjectOfType<Canvas>(true);
                parent = canvas?.transform;
            }

            GameObject root = new GameObject(RuntimeRootName);
            root.SetActive(false);
            if (parent != null)
            {
                root.transform.SetParent(parent, false);
            }

            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var canvasGroup = root.AddComponent<CanvasGroup>();
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            // 射线阻挡层。
            // 不要在这里绘制全屏半透明矩形；如果运行时sprite加载失败，
            // Unity会渲染出一个纯色四边形，看起来像普通矩形遮罩。
            // 视觉外观由下面克隆的游戏内MessageDialog边框提供。
            var blocker = root.AddComponent<Image>();
            blocker.color = new Color(0f, 0f, 0f, 0f);
            blocker.raycastTarget = true;

            // 恢复游戏内对话框边框作为纯视觉背景。
            // 这里只需要它的图形与布局；所有可交互内容仍挂在root上，保证z顺序正确。
            TextMeshProUGUI frameTextTemplate = null;
            try
            {
                var framePrefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
                if (framePrefab != null)
                {
                    var frame = UnityEngine.Object.Instantiate(framePrefab, root.transform, false);
                    frame.name = "TradeFrame";
                    frame.SetActive(true);

                    var frameRt = frame.GetComponent<RectTransform>();
                    if (frameRt != null)
                    {
                        frameRt.anchorMin = new Vector2(0.06f, 0.06f);
                        frameRt.anchorMax = new Vector2(0.94f, 0.94f);
                        frameRt.offsetMin = Vector2.zero;
                        frameRt.offsetMax = Vector2.zero;
                    }

                    var dialog = frame.GetComponentInChildren<MessageDialog>(true);
                    if (dialog != null)
                    {
                        var mainText = GetDialogField<TextMeshProUGUI>(dialog, "mainText");
                        var subText  = GetDialogField<TextMeshProUGUI>(dialog, "subText");
                        var dlgSingleConfirm = GetDialogField<Button>(dialog, "singleConfirmButton");
                        var dlgConfirm = GetDialogField<Button>(dialog, "confirmButton");
                        var dlgCancel  = GetDialogField<Button>(dialog, "cancelButton");

                        frameTextTemplate = mainText != null ? mainText : subText;

                        // 隐藏内置文本和按钮；当前面板会提供自己的内容。
                        HideDialogText(mainText);
                        HideDialogText(subText);
                        HideDialogButton(dlgSingleConfirm);
                        HideDialogButton(dlgConfirm);
                        HideDialogButton(dlgCancel);

                        dialog.enabled = false;
                    }

                    // 把边框推到最底层，确保控件绘制在上方。
                    frame.transform.SetAsFirstSibling();
                }
            }
            catch
            {
                // 忽略边框构建失败；它只影响视觉效果，不影响核心功能。
            }

            // 所有交易UI内容都直接挂在root上（位于边框之上）。
            Transform uiParent = root.transform;

            // 标题 / 状态 / 玩家名称
            var title = CloneTextOrCreate(frameTextTemplate != null ? frameTextTemplate : textTemplate, uiParent, "Title");
            title.text = "交易";
            title.alignment = TextAlignmentOptions.Center;
            title.fontSize = Mathf.Max(title.fontSize, 34);
            ConfigureAnchors(title.rectTransform, new Vector2(0.2f, 0.88f), new Vector2(0.8f, 0.96f));

            var status = CloneTextOrCreate(frameTextTemplate != null ? frameTextTemplate : textTemplate, uiParent, "Status");
            status.text = "请选择交易对象";
            status.alignment = TextAlignmentOptions.Center;
            status.fontSize = Mathf.Max(status.fontSize, 22);
            ConfigureAnchors(status.rectTransform, new Vector2(0.15f, 0.82f), new Vector2(0.85f, 0.88f));

            var p1Name = CloneTextOrCreate(frameTextTemplate != null ? frameTextTemplate : textTemplate, uiParent, "Player1Name");
            p1Name.text = "Player 1";
            p1Name.alignment = TextAlignmentOptions.Center;
            p1Name.fontSize = Mathf.Max(p1Name.fontSize, 20);
            ConfigureAnchors(p1Name.rectTransform, new Vector2(0.08f, 0.74f), new Vector2(0.46f, 0.80f));

            var p2Name = CloneTextOrCreate(frameTextTemplate != null ? frameTextTemplate : textTemplate, uiParent, "Player2Name");
            p2Name.text = "Player 2";
            p2Name.alignment = TextAlignmentOptions.Center;
            p2Name.fontSize = Mathf.Max(p2Name.fontSize, 20);
            ConfigureAnchors(p2Name.rectTransform, new Vector2(0.54f, 0.74f), new Vector2(0.92f, 0.80f));

            // 交易区域
            GameObject p1AreaGo = new GameObject("Player1Area");
            p1AreaGo.transform.SetParent(uiParent, false);
            var p1Area = p1AreaGo.AddComponent<RectTransform>();
            ConfigureAnchors(p1Area, new Vector2(0.08f, 0.28f), new Vector2(0.46f, 0.72f));

            GameObject p2AreaGo = new GameObject("Player2Area");
            p2AreaGo.transform.SetParent(uiParent, false);
            var p2Area = p2AreaGo.AddComponent<RectTransform>();
            ConfigureAnchors(p2Area, new Vector2(0.54f, 0.28f), new Vector2(0.92f, 0.72f));

            // 槽位：克隆游戏内按钮组件，让槽位使用原生按钮外观而不是普通矩形。
            var slotTextTemplate = frameTextTemplate != null ? frameTextTemplate : textTemplate;
            var p1Slots = CreateSlotColumn(p1Area, slotTextTemplate, confirmTemplate, 5, "P1");
            var p2Slots = CreateSlotColumn(p2Area, slotTextTemplate, confirmTemplate, 5, "P2");

            // 底部的确认 / 取消按钮。
            var confirm = UnityEngine.Object.Instantiate(confirmTemplate, uiParent, false);
            confirm.name = "Confirm";
            SetButtonLabel(confirm, "确认交易");
            DisableExtraButtons(confirm);
            DisableTooltipBehaviours(confirm.gameObject);
            ConfigureAnchors(confirm.GetComponent<RectTransform>(), new Vector2(0.22f, 0.10f), new Vector2(0.48f, 0.18f));

            var cancel = UnityEngine.Object.Instantiate(cancelTemplate, uiParent, false);
            cancel.name = "Cancel";
            SetButtonLabel(cancel, "取消");
            DisableExtraButtons(cancel);
            DisableTooltipBehaviours(cancel.gameObject);
            ConfigureAnchors(cancel.GetComponent<RectTransform>(), new Vector2(0.52f, 0.10f), new Vector2(0.78f, 0.18f));

            // 添加TradePanel并绑定字段。
            var panel = root.AddComponent<TradePanel>();

            // 标记为运行时创建，便于后续调用判断是否需要重建。
            var marker = root.AddComponent<TradePanelRuntimeMarker>();
            marker.Version = RuntimeUiVersion;
            panel.BindRuntimeUi(
                p1Area,
                p2Area,
                p1Slots,
                p2Slots,
                confirm,
                cancel,
                status,
                p1Name,
                p2Name);

            // 确保Awake()能够挂接按钮监听。
            root.SetActive(true);
            root.SetActive(false);

            return panel;
        }
        catch (Exception ex)
        {
            TradeUiMessages.ShowTopMessage($"交易界面初始化失败：{ex.Message}");
            return null;
        }
    }

    private static bool IsRuntimeCreatedPanel(TradePanel panel)
    {
        if (panel == null)
        {
            return false;
        }

        if (panel.GetComponent<TradePanelRuntimeMarker>() != null)
        {
            return true;
        }

        // 向后兼容：旧版运行时面板使用固定的根节点名称。
        return string.Equals(panel.gameObject.name, RuntimeRootName, StringComparison.Ordinal);
    }

    private static bool IsCurrentRuntimePanel(TradePanel panel)
    {
        if (panel == null)
        {
            return false;
        }

        var marker = panel.GetComponent<TradePanelRuntimeMarker>();
        if (marker == null)
        {
            return false;
        }

        return string.Equals(marker.Version, RuntimeUiVersion, StringComparison.Ordinal);
    }

    private static bool IsUnderRuntimeTradePanel(Transform t)
    {
        Transform cur = t;
        while (cur != null)
        {
            if (string.Equals(cur.name, RuntimeRootName, StringComparison.Ordinal))
            {
                return true;
            }

            if (cur.GetComponent<TradePanelRuntimeMarker>() != null)
            {
                return true;
            }

            cur = cur.parent;
        }

        return false;
    }

    internal sealed class TradePanelRuntimeMarker : MonoBehaviour
    {
        public string Version;
    }

    private static void HideDialogButton(Button button)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.gameObject.SetActive(false);
    }

    private static void HideDialogText(TextMeshProUGUI text)
    {
        if (text == null)
        {
            return;
        }

        text.text = string.Empty;
        text.raycastTarget = false;
        var c = text.color;
        c.a = 0f;
        text.color = c;
    }

    private static T GetDialogField<T>(MessageDialog dialog, string fieldName) where T : class
    {
        if (dialog == null || string.IsNullOrWhiteSpace(fieldName))
        {
            return null;
        }

        FieldInfo fi = typeof(MessageDialog).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (fi == null)
        {
            return null;
        }

        return fi.GetValue(dialog) as T;
    }

    private static TextMeshProUGUI CloneTextOrCreate(TextMeshProUGUI template, Transform parent, string name)
    {
        TextMeshProUGUI text;
        if (template != null)
        {
            text = UnityEngine.Object.Instantiate(template, parent, false);
            text.name = name;
        }
        else
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            text = go.AddComponent<TextMeshProUGUI>();
        }

        // 确保存在rectTransform。
        _ = text.rectTransform;
        return text;
    }

    private static void ConfigureAnchors(RectTransform rt, Vector2 min, Vector2 max)
    {
        if (rt == null)
        {
            return;
        }

        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void SetButtonLabel(CommonButtonWidget button, string label)
    {
        var tmp = button?.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp != null)
        {
            tmp.text = label;
            tmp.alignment = TextAlignmentOptions.Center;
        }
    }

    private static CommonButtonWidget TryPickButtonTemplate(bool preferConfirm)
    {
        try
        {
            // 1) 强烈优先：从已知的原生UI prefab中提取。
            var dialogPrefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
            if (dialogPrefab != null)
            {
                var dialog = dialogPrefab.GetComponent<MessageDialog>();
                if (dialog != null)
                {
                    Button btn;
                    if (preferConfirm)
                    {
                        btn = GetDialogField<Button>(dialog, "singleConfirmButton")
                           ?? GetDialogField<Button>(dialog, "confirmButton");
                    }
                    else
                    {
                        btn = GetDialogField<Button>(dialog, "cancelButton")
                           ?? GetDialogField<Button>(dialog, "confirmButton")
                           ?? GetDialogField<Button>(dialog, "singleConfirmButton");
                    }

                    if (btn != null)
                    {
                        var w = TryResolveCommonButtonWidget(btn);
                        if (w != null)
                        {
                            return w;
                        }
                    }
                }
            }

            var candidates = UnityEngine.Object.FindObjectsByType<CommonButtonWidget>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (candidates.Length == 0)
            {
                return null;
            }

            CommonButtonWidget best = null;
            int bestScore = int.MaxValue;

            foreach (var c in candidates)
            {
                if (c == null || c.button == null)
                {
                    continue;
                }

                // 避免选中我们自己运行时生成的按钮。
                if (IsUnderRuntimeTradePanel(c.transform))
                {
                    continue;
                }

                int buttons = c.GetComponentsInChildren<Button>(true).Length;
                int nodes = c.GetComponentsInChildren<Transform>(true).Length;

                // 优先选择真正的单按钮组件模板，避免克隆整条按钮栏或整块面板。
                if (buttons == 0)
                {
                    continue;
                }

                // 选择确认按钮样式时，优先避开名称中带cancel的组件。
                if (preferConfirm
                    && !string.IsNullOrWhiteSpace(c.name)
                    && c.name.IndexOf("cancel", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }

                int score = (buttons * 1000) + nodes;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = c;
                }
            }

            return best;
        }
        catch
        {
            return null;
        }
    }

    private static CommonButtonWidget TryResolveCommonButtonWidget(Button target)
    {
        if (target == null)
        {
            return null;
        }

        // 优先选择最近且显式引用该 Button 的组件。
        var widgets = target.GetComponentsInParent<CommonButtonWidget>(true);
        foreach (var w in widgets)
        {
            if (ReferenceEquals(w.button, target))
            {
                return w;
            }
        }

        return target.GetComponentInParent<CommonButtonWidget>(true);
    }

    private static TextMeshProUGUI TryPickTextTemplate()
    {
        try
        {
            // 优先使用原生对话框prefab里的TMP，让克隆标签沿用游戏字体与材质。
            var dialogPrefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
            if (dialogPrefab != null)
            {
                var tmp = dialogPrefab.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp != null)
                {
                    return tmp;
                }
            }

            var tmps = UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (tmps.Length == 0)
            {
                return null;
            }

            foreach (var t in tmps)
            {
                if (t == null)
                {
                    continue;
                }

                if (IsUnderRuntimeTradePanel(t.transform))
                {
                    continue;
                }

                return t;
            }

            return tmps.FirstOrDefault(t => t != null);
        }
        catch
        {
            return null;
        }
    }

    private static void DisableExtraButtons(CommonButtonWidget widget)
    {
        if (widget == null)
        {
            return;
        }

        Button keep = widget.button;
        var buttons = widget.GetComponentsInChildren<Button>(true);
        if (buttons.Length <= 1)
        {
            return;
        }

        foreach (var b in buttons)
        {
            if (b == null || b == keep)
            {
                continue;
            }

            b.enabled = false;
            b.interactable = false;
        }
    }

    private static void DisableTooltipBehaviours(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        foreach (var behaviour in root.GetComponentsInChildren<Behaviour>(true))
        {
            if (behaviour == null)
            {
                continue;
            }

            var name = behaviour.GetType().Name;
            if (name != null && name.IndexOf("Tooltip", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                behaviour.enabled = false;
            }
        }
    }

    private static TradeSlotWidget[] CreateSlotColumn(RectTransform area, TextMeshProUGUI textTemplate, CommonButtonWidget buttonTemplate, int count, string prefix)
    {
        TradeSlotWidget[] slots = new TradeSlotWidget[count];
        for (int i = 0; i < count; i++)
        {
            // 克隆游戏内制作好的按钮组件，让槽位外观保持原生UI风格。
            // 然后再挂上TradeSlotWidget（继承自CommonButtonWidget），补上卡牌Tooltip与移除逻辑。
            var slotWidget = UnityEngine.Object.Instantiate(buttonTemplate, area, false);
            slotWidget.name = $"{prefix}_Slot_{i + 1}";
            DisableExtraButtons(slotWidget);
            DisableTooltipBehaviours(slotWidget.gameObject);

            // 槽位组件不能继承模板里的子按钮和图标，否则可能堆出一串X标记。
            // 这里只保留主按钮对象。
            var keepBtn = slotWidget.button;
            var buttons = slotWidget.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                if (b == null || b == keepBtn)
                {
                    continue;
                }

                UnityEngine.Object.Destroy(b.gameObject);
            }

            // 同时移除模板下多余的图像和图标（例如 cancel / close 图标）。
            var keepGraphic = keepBtn?.targetGraphic;
            Image keepImage = keepGraphic as Image;
            var images = slotWidget.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img == null)
                {
                    continue;
                }

                // 保留主背景图（通常是targetGraphic）以及选中边框。
                if (keepImage != null && ReferenceEquals(img, keepImage))
                {
                    continue;
                }

                if (string.Equals(img.gameObject.name, "selectedBorder", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(img.gameObject.name, "SelectedBorder", StringComparison.Ordinal))
                {
                    continue;
                }

                // 如果它和保留图像共用同一个对象，也一并保留。
                if (keepImage != null && ReferenceEquals(img.gameObject, keepImage.gameObject))
                {
                    continue;
                }

                // 其他图像全部移除；槽位内容（卡牌图像/文本）由运行时子节点提供。
                UnityEngine.Object.Destroy(img);
            }

            // 某些按钮模板自带“取消”或“确认”等默认文案。
            // 槽位不应继承这些文案；名称文本由我们自己的Name标签提供。
            var tmps = slotWidget.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps)
            {
                if (t == null)
                {
                    continue;
                }

                t.text = string.Empty;
                t.raycastTarget = false;
                var c = t.color;
                c.a = 0f;
                t.color = c;
            }

            // 禁用原始CommonButtonWidget，避免出现双重指针处理。
            slotWidget.enabled = false;

            var slotGo = slotWidget.gameObject;
            var rt = slotGo.GetComponent<RectTransform>();

            // 手动做垂直定位，避免依赖LayoutGroup。
            float height = 1f / count;
            float top = 1f - i * height;
            float bottom = top - height;
            rt.anchorMin = new Vector2(0f, bottom);
            rt.anchorMax = new Vector2(1f, top);
            rt.offsetMin = new Vector2(0f, 4f);
            rt.offsetMax = new Vector2(0f, -4f);

            var slot = slotGo.AddComponent<TradeSlotWidget>();
            // 连接克隆模板中的底层UnityEngine.UI.Button。
            slot.button = slotGo.GetComponentInChildren<Button>(true);

            // 可选的卡牌图像承载层（RawImage），用于显示原生卡牌纹理。
            GameObject imgGo = new GameObject("CardImage");
            imgGo.transform.SetParent(slotGo.transform, false);
            var raw = imgGo.AddComponent<RawImage>();
            raw.texture = null;
            raw.raycastTarget = false;
            var rawRt = imgGo.GetComponent<RectTransform>();
            if (rawRt == null)
            {
                rawRt = imgGo.AddComponent<RectTransform>();
            }
            ConfigureAnchors(rawRt, new Vector2(0.02f, 0.12f), new Vector2(0.20f, 0.88f));

            // 优先复用模板里的标签；没有的话再克隆或创建一个。
            var label = CloneTextOrCreate(textTemplate, slotGo.transform, "Name");
            label.name = "Name";
            // 给卡牌图像预留显示空间。
            ConfigureAnchors(label.rectTransform, new Vector2(0.22f, 0f), new Vector2(1f, 1f));
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;

            var labelColor = label.color;
            labelColor.a = 1f;
            label.color = labelColor;

            slot.BindRuntime(label, raw);
            slot.ClearSlot();
            slots[i] = slot;
        }

        return slots;
    }
}
