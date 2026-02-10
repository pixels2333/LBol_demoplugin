using System;
using System.Linq;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Widgets;
using NetworkPlugin.Patch.UI;
using NetworkPlugin.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Panels;

internal static class TradePanelRuntimeFactory
{
    internal static TradePanel GetOrCreate(Transform preferredParent)
    {
        try
        {
            // Reuse any existing TradePanel instances (and keep only one active to avoid stacking).
            TradePanel[] existingPanels = null;
            try
            {
                existingPanels = UnityEngine.Object.FindObjectsOfType<TradePanel>(true);
            }
            catch
            {
                existingPanels = null;
            }

            if (existingPanels != null && existingPanels.Length > 0)
            {
                for (int i = 1; i < existingPanels.Length; i++)
                {
                    try
                    {
                        if (existingPanels[i] != null)
                        {
                            existingPanels[i].gameObject.SetActive(false);
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }

                return existingPanels[0];
            }

            if (!UiManager.IsInitialized)
            {
                TradeUiMessages.ShowTradePanelMissing();
                return null;
            }

            // Find an in-game styled button template (CommonButtonWidget) to clone.
            // We intentionally clone from existing UI so we inherit the game's visuals and input behavior.
            CommonButtonWidget buttonTemplate = TryPickButtonTemplate();

            if (buttonTemplate == null)
            {
                TradeUiMessages.ShowTopMessage("交易界面不可用：未找到可复用的按钮模板。请先进入游戏内 UI（例如商店/间隙）。");
                return null;
            }

            TextMeshProUGUI textTemplate = null;
            try
            {
                textTemplate = UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>(true)
                    ?.FirstOrDefault(t => t != null);
            }
            catch
            {
                textTemplate = null;
            }

            // Capture a lightweight visual style from the picked template (used for slot backgrounds).
            Image templateImage = null;
            try
            {
                templateImage = buttonTemplate.GetComponentInChildren<Image>(true);
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

            var root = new GameObject("NetworkPlugin_TradePanel");
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

            // Background
            var bg = root.AddComponent<Image>();
            try
            {
                bg.sprite = ResourcesHelper.LoadUiBackground("Adventure");
            }
            catch
            {
                bg.sprite = null;
            }
            bg.color = new Color(0f, 0f, 0f, 0.65f);
            bg.raycastTarget = true;

            // Title / status / player names
            var title = CloneTextOrCreate(textTemplate, root.transform, "Title");
            title.text = "交易";
            title.alignment = TextAlignmentOptions.Center;
            title.fontSize = Mathf.Max(title.fontSize, 34);
            ConfigureAnchors(title.rectTransform, new Vector2(0.2f, 0.88f), new Vector2(0.8f, 0.96f));

            var status = CloneTextOrCreate(textTemplate, root.transform, "Status");
            status.text = "请选择交易对象";
            status.alignment = TextAlignmentOptions.Center;
            status.fontSize = Mathf.Max(status.fontSize, 22);
            ConfigureAnchors(status.rectTransform, new Vector2(0.15f, 0.82f), new Vector2(0.85f, 0.88f));

            var p1Name = CloneTextOrCreate(textTemplate, root.transform, "Player1Name");
            p1Name.text = "Player 1";
            p1Name.alignment = TextAlignmentOptions.Center;
            p1Name.fontSize = Mathf.Max(p1Name.fontSize, 20);
            ConfigureAnchors(p1Name.rectTransform, new Vector2(0.08f, 0.74f), new Vector2(0.46f, 0.80f));

            var p2Name = CloneTextOrCreate(textTemplate, root.transform, "Player2Name");
            p2Name.text = "Player 2";
            p2Name.alignment = TextAlignmentOptions.Center;
            p2Name.fontSize = Mathf.Max(p2Name.fontSize, 20);
            ConfigureAnchors(p2Name.rectTransform, new Vector2(0.54f, 0.74f), new Vector2(0.92f, 0.80f));

            // Trade areas
            var p1AreaGo = new GameObject("Player1Area");
            p1AreaGo.transform.SetParent(root.transform, false);
            var p1Area = p1AreaGo.AddComponent<RectTransform>();
            ConfigureAnchors(p1Area, new Vector2(0.08f, 0.28f), new Vector2(0.46f, 0.72f));

            var p2AreaGo = new GameObject("Player2Area");
            p2AreaGo.transform.SetParent(root.transform, false);
            var p2Area = p2AreaGo.AddComponent<RectTransform>();
            ConfigureAnchors(p2Area, new Vector2(0.54f, 0.28f), new Vector2(0.92f, 0.72f));

            // Slots: create lightweight slot rows (do NOT clone the full button hierarchy per slot).
            var p1Slots = CreateSlotColumn(p1Area, textTemplate, templateImage, 5, "P1");
            var p2Slots = CreateSlotColumn(p2Area, textTemplate, templateImage, 5, "P2");

            // Confirm / cancel buttons at bottom.
            var confirm = UnityEngine.Object.Instantiate(buttonTemplate, root.transform, false);
            confirm.name = "Confirm";
            SetButtonLabel(confirm, "确认");
            DisableExtraButtons(confirm);
            DisableTooltipBehaviours(confirm.gameObject);
            ConfigureAnchors(confirm.GetComponent<RectTransform>(), new Vector2(0.22f, 0.10f), new Vector2(0.48f, 0.18f));

            var cancel = UnityEngine.Object.Instantiate(buttonTemplate, root.transform, false);
            cancel.name = "Cancel";
            SetButtonLabel(cancel, "取消");
            DisableExtraButtons(cancel);
            DisableTooltipBehaviours(cancel.gameObject);
            ConfigureAnchors(cancel.GetComponent<RectTransform>(), new Vector2(0.52f, 0.10f), new Vector2(0.78f, 0.18f));

            // Add TradePanel and bind fields.
            var panel = root.AddComponent<TradePanel>();
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

    private static CommonButtonWidget TryPickButtonTemplate()
    {
        try
        {
            var candidates = UnityEngine.Object.FindObjectsOfType<CommonButtonWidget>(true);
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

                int buttons = 0;
                int nodes = 0;

                try { buttons = c.GetComponentsInChildren<Button>(true)?.Length ?? 0; } catch { buttons = 0; }
                try { nodes = c.GetComponentsInChildren<Transform>(true)?.Length ?? 0; } catch { nodes = 0; }

                // Prefer templates that are actually a single-button widget (avoids cloning whole bars/panels).
                if (buttons == 0)
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

    private static TradeSlotWidget[] CreateSlotColumn(RectTransform area, TextMeshProUGUI textTemplate, Image templateImage, int count, string prefix)
    {
        var slots = new TradeSlotWidget[count];
        for (int i = 0; i < count; i++)
        {
            // Slot rows are lightweight buttons (Image + Button + TMP). We intentionally do NOT clone
            // arbitrary existing button hierarchies, to avoid producing lots of nested duplicate buttons.
            var slotGo = new GameObject($"{prefix}_Slot_{i + 1}");
            slotGo.transform.SetParent(area, false);

            var bg = slotGo.AddComponent<Image>();
            if (templateImage != null)
            {
                bg.sprite = templateImage.sprite;
                bg.material = templateImage.material;
                bg.type = templateImage.type;
                bg.preserveAspect = templateImage.preserveAspect;
                bg.color = new Color(1f, 1f, 1f, 0.12f);
            }
            else
            {
                bg.color = new Color(1f, 1f, 1f, 0.12f);
            }

            var btn = slotGo.AddComponent<Button>();
            btn.targetGraphic = bg;

            var rt = slotGo.GetComponent<RectTransform>();
            if (rt == null)
            {
                rt = slotGo.AddComponent<RectTransform>();
            }

            // Manual vertical positioning (avoid LayoutGroup dependencies).
            float height = 1f / count;
            float top = 1f - i * height;
            float bottom = top - height;
            rt.anchorMin = new Vector2(0f, bottom);
            rt.anchorMax = new Vector2(1f, top);
            rt.offsetMin = new Vector2(0f, 4f);
            rt.offsetMax = new Vector2(0f, -4f);

            var slot = slotGo.AddComponent<TradeSlotWidget>();

            slot.button = btn;

            var label = CloneTextOrCreate(textTemplate, slotGo.transform, "Name");
            ConfigureAnchors(label.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f));
            label.alignment = TextAlignmentOptions.Center;

            slot.BindRuntime(label);
            slot.ClearSlot();
            slots[i] = slot;
        }

        return slots;
    }
}
