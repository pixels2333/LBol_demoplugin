using System;
using System.Linq;
using System.Reflection;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Widgets;
using NetworkPlugin.Patch.UI;
using NetworkPlugin.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Panels;

internal static class TradePanelRuntimeFactory
{
    private const string RuntimeRootName = "NetworkPlugin_TradePanel";
    private const string RuntimeUiVersion = "2026-02-14-ui-v6";

    internal static TradePanel GetOrCreate(Transform preferredParent)
    {
        try
        {
            // Reuse any existing TradePanel instances (and keep only one active to avoid stacking).
            TradePanel[] existingPanels = null;
            try
            {
                existingPanels = UnityEngine.Object.FindObjectsByType<TradePanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            }
            catch
            {
                existingPanels = null;
            }

            if (existingPanels != null && existingPanels.Length > 0)
            {
                // Prefer a non-runtime (prefab-wired) TradePanel if present.
                TradePanel prefabPanel = null;
                foreach (var p in existingPanels)
                {
                    if (p == null)
                    {
                        continue;
                    }

                    if (!IsRuntimeCreatedPanel(p))
                    {
                        prefabPanel = p;
                        break;
                    }
                }

                // Otherwise reuse only the current runtime panel version.
                TradePanel currentRuntime = null;
                foreach (var p in existingPanels)
                {
                    if (p == null)
                    {
                        continue;
                    }

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
                        if (p == null || ReferenceEquals(p, prefabPanel))
                        {
                            continue;
                        }

                        try
                        {
                            if (IsRuntimeCreatedPanel(p))
                            {
                                p.gameObject.SetActive(false);
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                    }

                    Plugin.Logger?.LogInfo("[TradePanelRuntimeFactory] Reusing prefab-wired TradePanel.");
                    return prefabPanel;
                }

                if (currentRuntime != null)
                {
                    foreach (var p in existingPanels)
                    {
                        if (p == null || ReferenceEquals(p, currentRuntime))
                        {
                            continue;
                        }

                        try { p.gameObject.SetActive(false); } catch { }
                    }

                    Plugin.Logger?.LogInfo($"[TradePanelRuntimeFactory] Reusing runtime TradePanel (version={RuntimeUiVersion}).");
                    return currentRuntime;
                }

                // Old runtime panels from previous builds can remain in memory and keep the old "rectangle" look.
                // Destroy them so the next creation reflects the latest UI code.
                foreach (var p in existingPanels)
                {
                    if (p == null)
                    {
                        continue;
                    }

                    if (!IsRuntimeCreatedPanel(p))
                    {
                        continue;
                    }

                    try { UnityEngine.Object.Destroy(p.gameObject); } catch { }
                }

                Plugin.Logger?.LogInfo("[TradePanelRuntimeFactory] Destroyed old runtime TradePanel(s); rebuilding with latest UI.");
            }

            if (!UiManager.IsInitialized)
            {
                TradeUiMessages.ShowTradePanelMissing();
                return null;
            }

            // Find an in-game styled button template (CommonButtonWidget) to clone.
            // Prefer extracting from vanilla prefabs to avoid accidentally picking our own old runtime buttons.
            CommonButtonWidget confirmTemplate = TryPickButtonTemplate(preferConfirm: true);
            CommonButtonWidget cancelTemplate = TryPickButtonTemplate(preferConfirm: false);

            if (confirmTemplate == null)
            {
                TradeUiMessages.ShowTopMessage("交易界面不可用：未找到可复用的按钮模板。请先进入游戏内 UI（例如商店/间隙）。");
                return null;
            }

            // Fallback: if we can't locate a dedicated cancel template, reuse confirm template.
            cancelTemplate ??= confirmTemplate;

            TextMeshProUGUI textTemplate = TryPickTextTemplate();

            // Capture a lightweight visual style from the picked template (used for slot backgrounds).
            Image templateImage = null;
            try
            {
                templateImage = confirmTemplate.GetComponentInChildren<Image>(true);
            }
            catch
            {
                templateImage = null;
            }

            Transform parent = preferredParent;
            if (parent == null)
            {
                // Best-effort fallback: attach under any active Canvas so the UI can render.
                try
                {
                    var canvas = UnityEngine.Object.FindObjectOfType<Canvas>(true);
                    parent = canvas != null ? canvas.transform : null;
                }
                catch
                {
                    parent = null;
                }
            }

            var root = new GameObject(RuntimeRootName);
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

            // Raycast blocker.
            // Do NOT draw a full-screen semi-transparent rectangle here; if the sprite load fails at runtime,
            // Unity will render a plain colored quad which looks like a generic rectangle.
            // Visuals come from the in-game MessageDialog frame we clone below.
            var blocker = root.AddComponent<Image>();
            blocker.sprite = null;
            blocker.color = new Color(0f, 0f, 0f, 0f);
            blocker.raycastTarget = true;

            // Use an in-game authored dialog prefab as the main frame so the trade UI matches vanilla visuals.
            // We do NOT call UiDialog.Show() here; we only reuse the prefab's graphics/layout.
            RectTransform framePanelRect = null;
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
                        // Keep a margin so the frame doesn't touch screen edges.
                        frameRt.anchorMin = new Vector2(0.06f, 0.06f);
                        frameRt.anchorMax = new Vector2(0.94f, 0.94f);
                        frameRt.offsetMin = Vector2.zero;
                        frameRt.offsetMax = Vector2.zero;
                    }

                    var dialog = frame.GetComponentInChildren<MessageDialog>(true);
                    if (dialog != null)
                    {
                        var mainText = GetDialogField<TextMeshProUGUI>(dialog, "mainText");
                        var subText = GetDialogField<TextMeshProUGUI>(dialog, "subText");
                        var dlgSingleConfirm = GetDialogField<Button>(dialog, "singleConfirmButton");
                        var dlgConfirm = GetDialogField<Button>(dialog, "confirmButton");
                        var dlgCancel = GetDialogField<Button>(dialog, "cancelButton");

                        // Pick a TMP template from the dialog so any cloned labels inherit vanilla font/material.
                        frameTextTemplate = mainText != null ? mainText : subText;

                        // Hide built-in dialog texts/buttons; our panel provides its own header + actions.
                        HideDialogText(mainText);
                        HideDialogText(subText);
                        HideDialogButton(dlgSingleConfirm);
                        HideDialogButton(dlgConfirm);
                        HideDialogButton(dlgCancel);

                        // Disable dialog behavior to avoid input handling side effects.
                        dialog.enabled = false;

                        var cancelRt = dlgCancel != null ? dlgCancel.GetComponent<RectTransform>() : null;
                        framePanelRect = TryFindCommonAncestorRect(mainText != null ? mainText.rectTransform : null, cancelRt)
                                         ?? TryFindCommonAncestorRect(subText != null ? subText.rectTransform : null, cancelRt)
                                         ?? frameRt;
                    }
                }
            }
            catch
            {
                framePanelRect = null;
                frameTextTemplate = null;
            }

            // All trade UI content is attached to the frame panel if available.
            Transform uiParent = (framePanelRect != null ? framePanelRect.transform : root.transform);

            // Title / status / player names
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

            // Trade areas
            var p1AreaGo = new GameObject("Player1Area");
            p1AreaGo.transform.SetParent(uiParent, false);
            var p1Area = p1AreaGo.AddComponent<RectTransform>();
            ConfigureAnchors(p1Area, new Vector2(0.08f, 0.28f), new Vector2(0.46f, 0.72f));

            var p2AreaGo = new GameObject("Player2Area");
            p2AreaGo.transform.SetParent(uiParent, false);
            var p2Area = p2AreaGo.AddComponent<RectTransform>();
            ConfigureAnchors(p2Area, new Vector2(0.54f, 0.28f), new Vector2(0.92f, 0.72f));

            // Slots: clone the in-game button widget so slots use vanilla button visuals instead of plain rectangles.
            var slotTextTemplate = frameTextTemplate != null ? frameTextTemplate : textTemplate;
            var p1Slots = CreateSlotColumn(p1Area, slotTextTemplate, confirmTemplate, 5, "P1");
            var p2Slots = CreateSlotColumn(p2Area, slotTextTemplate, confirmTemplate, 5, "P2");

            // Confirm / cancel buttons at bottom.
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

            // Add TradePanel and bind fields.
            var panel = root.AddComponent<TradePanel>();

            // Mark as runtime-created so future calls can decide whether to rebuild.
            try
            {
                var marker = root.AddComponent<TradePanelRuntimeMarker>();
                marker.Version = RuntimeUiVersion;
            }
            catch
            {
                // ignored
            }
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

            // Make sure Awake() can hook up button listeners.
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
        try
        {
            if (panel == null)
            {
                return false;
            }

            if (panel.GetComponent<TradePanelRuntimeMarker>() != null)
            {
                return true;
            }

            // Back-compat: old runtime panels had a stable root name.
            return string.Equals(panel.gameObject != null ? panel.gameObject.name : null, RuntimeRootName, StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsCurrentRuntimePanel(TradePanel panel)
    {
        try
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
        catch
        {
            return false;
        }
    }

    private static bool IsUnderRuntimeTradePanel(Transform t)
    {
        try
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
        catch
        {
            return false;
        }
    }

    internal sealed class TradePanelRuntimeMarker : MonoBehaviour
    {
        public string Version;
    }

    private static void HideDialogButton(Button button)
    {
        try
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(false);
        }
        catch
        {
            // ignored
        }
    }

    private static void HideDialogText(TextMeshProUGUI text)
    {
        try
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
        catch
        {
            // ignored
        }
    }

    private static T GetDialogField<T>(MessageDialog dialog, string fieldName) where T : class
    {
        try
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
        catch
        {
            return null;
        }
    }

    private static RectTransform TryFindCommonAncestorRect(RectTransform a, RectTransform b)
    {
        try
        {
            if (a == null || b == null)
            {
                return null;
            }

            var ancestors = new System.Collections.Generic.HashSet<Transform>();
            Transform t = a;
            while (t != null)
            {
                ancestors.Add(t);
                t = t.parent;
            }

            Transform u = b;
            while (u != null)
            {
                if (ancestors.Contains(u))
                {
                    return u as RectTransform;
                }
                u = u.parent;
            }

            return null;
        }
        catch
        {
            return null;
        }
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
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            text = go.AddComponent<TextMeshProUGUI>();
        }

        // Ensure rect exists.
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
        try
        {
            var tmp = button != null ? button.GetComponentInChildren<TextMeshProUGUI>(true) : null;
            if (tmp != null)
            {
                tmp.text = label;
                tmp.alignment = TextAlignmentOptions.Center;
            }
        }
        catch
        {
            // ignored
        }
    }

    private static CommonButtonWidget TryPickButtonTemplate(bool preferConfirm)
    {
        try
        {
            // 1) Strong preference: extract from a known vanilla UI prefab.
            try
            {
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
            }
            catch
            {
                // ignored
            }

            var candidates = UnityEngine.Object.FindObjectsByType<CommonButtonWidget>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (candidates == null || candidates.Length == 0)
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

                // Avoid selecting our own runtime-generated buttons.
                if (IsUnderRuntimeTradePanel(c.transform))
                {
                    continue;
                }

                int buttons = 0;
                int nodes = 0;

                try { buttons = c.GetComponentsInChildren<Button>(true)?.Length ?? 0; } catch { buttons = 0; }
                try { nodes = c.GetComponentsInChildren<Transform>(true)?.Length ?? 0; } catch { nodes = 0; }

                // Prefer templates that are actually a single-button widget (avoids cloning whole bars/panels).
                if (buttons == 0)
                {
                    continue;
                }

                // Prefer non-cancel widgets when picking the confirm style.
                if (preferConfirm)
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(c.name)
                            && c.name.IndexOf("cancel", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            continue;
                        }
                    }
                    catch
                    {
                        // ignored
                    }
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
        try
        {
            if (target == null)
            {
                return null;
            }

            // Prefer the closest widget that explicitly references this Button.
            var widgets = target.GetComponentsInParent<CommonButtonWidget>(true);
            if (widgets != null)
            {
                foreach (var w in widgets)
                {
                    if (w == null)
                    {
                        continue;
                    }

                    if (ReferenceEquals(w.button, target))
                    {
                        return w;
                    }
                }
            }

            return target.GetComponentInParent<CommonButtonWidget>(true);
        }
        catch
        {
            return null;
        }
    }

    private static TextMeshProUGUI TryPickTextTemplate()
    {
        try
        {
            // Prefer vanilla dialog prefab TMP so our cloned labels match game font/material.
            try
            {
                var dialogPrefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
                if (dialogPrefab != null)
                {
                    var tmp = dialogPrefab.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (tmp != null)
                    {
                        return tmp;
                    }
                }
            }
            catch
            {
                // ignored
            }

            var tmps = UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (tmps == null || tmps.Length == 0)
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
        try
        {
            if (widget == null)
            {
                return;
            }

            Button keep = widget.button;
            var buttons = widget.GetComponentsInChildren<Button>(true);
            if (buttons == null || buttons.Length <= 1)
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
        catch
        {
            // ignored
        }
    }

    private static void DisableTooltipBehaviours(GameObject root)
    {
        try
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
        catch
        {
            // ignored
        }
    }

    private static TradeSlotWidget[] CreateSlotColumn(RectTransform area, TextMeshProUGUI textTemplate, CommonButtonWidget buttonTemplate, int count, string prefix)
    {
        var slots = new TradeSlotWidget[count];
        for (int i = 0; i < count; i++)
        {
            // Clone an in-game authored button widget so the slot looks like vanilla UI.
            // Then attach TradeSlotWidget (derives from CommonButtonWidget) for card tooltip + remove behavior.
            var slotWidget = UnityEngine.Object.Instantiate(buttonTemplate, area, false);
            slotWidget.name = $"{prefix}_Slot_{i + 1}";
            DisableExtraButtons(slotWidget);
            DisableTooltipBehaviours(slotWidget.gameObject);

            // Slot widgets must not inherit template sub-buttons/icons (they can render as a stack of X markers).
            // Keep only the primary button object.
            try
            {
                var keepBtn = slotWidget.button;
                var buttons = slotWidget.GetComponentsInChildren<Button>(true);
                if (buttons != null)
                {
                    foreach (var b in buttons)
                    {
                        if (b == null || b == keepBtn)
                        {
                            continue;
                        }

                        try { UnityEngine.Object.Destroy(b.gameObject); } catch { }
                    }
                }

                // Also remove extra images/icons under the template (e.g., cancel/close icons).
                var keepGraphic = keepBtn != null ? keepBtn.targetGraphic : null;
                var keepImage = keepGraphic as Image;
                var images = slotWidget.GetComponentsInChildren<Image>(true);
                if (images != null)
                {
                    foreach (var img in images)
                    {
                        if (img == null)
                        {
                            continue;
                        }

                        // Keep the main background image (usually targetGraphic) and any selection border.
                        if (keepImage != null && ReferenceEquals(img, keepImage))
                        {
                            continue;
                        }

                        if (string.Equals(img.gameObject.name, "selectedBorder", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(img.gameObject.name, "SelectedBorder", StringComparison.Ordinal))
                        {
                            continue;
                        }

                        // If it's on the same object as the kept image, keep it.
                        if (keepImage != null && ReferenceEquals(img.gameObject, keepImage.gameObject))
                        {
                            continue;
                        }

                        // Otherwise remove; the slot content (card image/text) is provided by our runtime children.
                        try { UnityEngine.Object.Destroy(img); } catch { }
                    }
                }
            }
            catch
            {
                // ignored
            }

            // Some button templates carry default labels like "取消"/"确认".
            // Slots should not inherit those; we provide our own Name label instead.
            try
            {
                var tmps = slotWidget.GetComponentsInChildren<TextMeshProUGUI>(true);
                if (tmps != null)
                {
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
                }
            }
            catch
            {
                // ignored
            }

            // Disable the original CommonButtonWidget component to avoid double pointer handling.
            try { slotWidget.enabled = false; } catch { }

            var slotGo = slotWidget.gameObject;
            var rt = slotGo.GetComponent<RectTransform>();

            // Manual vertical positioning (avoid LayoutGroup dependencies).
            float height = 1f / count;
            float top = 1f - i * height;
            float bottom = top - height;
            rt.anchorMin = new Vector2(0f, bottom);
            rt.anchorMax = new Vector2(1f, top);
            rt.offsetMin = new Vector2(0f, 4f);
            rt.offsetMax = new Vector2(0f, -4f);

            var slot = slotGo.AddComponent<TradeSlotWidget>();
            // Wire the underlying UnityEngine.UI.Button from the cloned template.
            try
            {
                slot.button = slotGo.GetComponentInChildren<Button>(true);
            }
            catch
            {
                slot.button = null;
            }

            // Optional card image surface (RawImage) for vanilla card textures.
            var imgGo = new GameObject("CardImage");
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

            // Reuse the template's label if present; otherwise clone/create one.
            var label = CloneTextOrCreate(textTemplate, slotGo.transform, "Name");
            label.name = "Name";
            // Leave room for the card image.
            ConfigureAnchors(label.rectTransform, new Vector2(0.22f, 0f), new Vector2(1f, 1f));
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;

            try
            {
                var c = label.color;
                c.a = 1f;
                label.color = c;
            }
            catch
            {
                // ignored
            }

            slot.BindRuntime(label, raw);
            slot.ClearSlot();
            slots[i] = slot;
        }

        return slots;
    }
}
