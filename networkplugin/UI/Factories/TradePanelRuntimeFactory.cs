using System;
using System.Linq;
using System.Reflection;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Widgets;
using NetworkPlugin.Patch.UI;
using NetworkPlugin.UI.Components;
using NetworkPlugin.UI.Panels;
using NetworkPlugin.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Factories;

internal static class TradePanelRuntimeFactory
{
    private const string RuntimeRootName = "NetworkPlugin_TradePanel";
    private const string RuntimeUiVersion = "2026-08-25-ui-v27";

    internal static TradePanel GetOrCreate(Transform preferredParent)
    {
        try
        {

            TradePanel[] existingPanels = UnityEngine.Object.FindObjectsByType<TradePanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (existingPanels.Length > 0)
            {

                TradePanel prefabPanel = null;
                foreach (var p in existingPanels)
                {
                    if (!IsRuntimeCreatedPanel(p))
                    {
                        prefabPanel = p;
                        break;
                    }
                }

                TradePanel currentRuntime = null;
                foreach (var p in existingPanels)
                {
                    if (p != null)
                    {
                        var __marker = p.GetComponent<TradePanelRuntimeMarker>();
                        if (__marker != null && string.Equals(__marker.Version, RuntimeUiVersion, StringComparison.Ordinal))
                        {
                            currentRuntime = p;
                            break;
                        }
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

            GapSharedPanelTemplateFactory.GapRuntimePanelTemplate scaffold = GapSharedPanelTemplateFactory.Create(
                preferredParent,
                RuntimeRootName,
                "交易",
                "请选择交易对象",
                "确认交易",
                "取消",
                createFrame: true,
                layoutInFrame: true);
            if (scaffold == null)
            {
                TradeUiMessages.ShowTopMessage("交易界面不可用：未找到可复用的按钮模板。请先进入游戏内 UI（例如商店/间隙）。");
                return null;
            }

            Transform uiParent = scaffold.ContentRoot;
            TextMeshProUGUI textTemplate = scaffold.TextTemplate;

            var p1Name = GapSharedPanelTemplateFactory.CloneTextOrCreate(textTemplate, uiParent, "Player1Name");
            p1Name.text = "Player 1";
            p1Name.alignment = TextAlignmentOptions.Center;
            p1Name.fontSize = Mathf.Max(p1Name.fontSize, 20);
            GapSharedPanelTemplateFactory.ConfigureAnchors(p1Name.rectTransform, new Vector2(0.00f, 0.84f), new Vector2(0.46f, 0.98f));

            var p2Name = GapSharedPanelTemplateFactory.CloneTextOrCreate(textTemplate, uiParent, "Player2Name");
            p2Name.text = "Player 2";
            p2Name.alignment = TextAlignmentOptions.Center;
            p2Name.fontSize = Mathf.Max(p2Name.fontSize, 20);
            GapSharedPanelTemplateFactory.ConfigureAnchors(p2Name.rectTransform, new Vector2(0.54f, 0.84f), new Vector2(1.00f, 0.98f));

            var panel = scaffold.Root.AddComponent<TradePanel>();

            var marker = scaffold.Root.AddComponent<TradePanelRuntimeMarker>();
            marker.Version = RuntimeUiVersion;
            panel.BindRuntimeUi(
                scaffold.ContentRoot,
                scaffold.ConfirmButton,
                scaffold.CancelButton,
                scaffold.StatusText,
                p1Name,
                p2Name);

            scaffold.Root.SetActive(true);
            scaffold.Root.SetActive(false);

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

        return string.Equals(panel.gameObject.name, RuntimeRootName, StringComparison.Ordinal);
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

                if (IsUnderRuntimeTradePanel(c.transform))
                {
                    continue;
                }

                int buttons = c.GetComponentsInChildren<Button>(true).Length;
                int nodes = c.GetComponentsInChildren<Transform>(true).Length;

                if (buttons == 0)
                {
                    continue;
                }

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

    private static TradeSlotWidget[] CreateSlotColumn(RectTransform area, TextMeshProUGUI textTemplate, CommonButtonWidget buttonTemplate, int count, string prefix)
    {
        TradeSlotWidget[] slots = new TradeSlotWidget[count];
        for (int i = 0; i < count; i++)
        {

            var slotWidget = UnityEngine.Object.Instantiate(buttonTemplate, area, false);
            slotWidget.name = $"{prefix}_Slot_{i + 1}";
            if (slotWidget != null)
            {
                Button __keep = slotWidget.button;
                var __btns = slotWidget.GetComponentsInChildren<Button>(true);
                if (__btns.Length > 1)
                {
                    foreach (var __b in __btns)
                    {
                        if (__b != null && __b != __keep)
                        {
                            __b.enabled = false;
                            __b.interactable = false;
                        }
                    }
                }
            }
            if (slotWidget != null)
            {
                foreach (var __bh in slotWidget.GetComponentsInChildren<Behaviour>(true))
                {
                    if (__bh == null) continue;
                    var __n = __bh.GetType().Name;
                    if (__n != null && __n.IndexOf("Tooltip", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        __bh.enabled = false;
                    }
                }
            }

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

            var keepGraphic = keepBtn?.targetGraphic;
            Image keepImage = keepGraphic as Image;
            var images = slotWidget.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img == null)
                {
                    continue;
                }

                if (keepImage != null && ReferenceEquals(img, keepImage))
                {
                    continue;
                }

                if (string.Equals(img.gameObject.name, "selectedBorder", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(img.gameObject.name, "SelectedBorder", StringComparison.Ordinal))
                {
                    continue;
                }

                if (keepImage != null && ReferenceEquals(img.gameObject, keepImage.gameObject))
                {
                    continue;
                }

                UnityEngine.Object.Destroy(img);
            }

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

            slotWidget.enabled = false;

            var slotGo = slotWidget.gameObject;
            var rt = slotGo.GetComponent<RectTransform>();

            float height = 1f / count;
            float top = 1f - i * height;
            float bottom = top - height;
            rt.anchorMin = new Vector2(0f, bottom);
            rt.anchorMax = new Vector2(1f, top);
            rt.offsetMin = new Vector2(0f, 4f);
            rt.offsetMax = new Vector2(0f, -4f);

            var slot = slotGo.AddComponent<TradeSlotWidget>();

            slot.button = slotGo.GetComponentInChildren<Button>(true);

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

            TextMeshProUGUI label;
            if (textTemplate != null)
            {
                label = UnityEngine.Object.Instantiate(textTemplate, slotGo.transform, false);
            }
            else
            {
                GameObject __go = new GameObject("Name");
                __go.transform.SetParent(slotGo.transform, false);
                label = __go.AddComponent<TextMeshProUGUI>();
            }
            _ = label.rectTransform;
            label.name = "Name";

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
