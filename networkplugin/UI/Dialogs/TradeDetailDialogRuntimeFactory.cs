using System;
using System.Linq;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Dialogs;

internal static class TradeDetailDialogRuntimeFactory
{
    internal static TradeDetailDialog GetOrCreate()
    {
        try
        {
            var existing = UnityEngine.Object.FindAnyObjectByType<TradeDetailDialog>();
            if (existing != null)
            {
                return existing;
            }

            if (!UiManager.IsInitialized || UiManager.Instance == null)
            {
                return null;
            }

            var buttonTemplate = TryPickButtonTemplate();
            var textTemplate = TryPickTextTemplate();
            GameObject rowTemplate = null;
            RecordCardCell cardCellTemplate = null;
            ExhibitWidget exhibitTemplate = null;

            try
            {
                var historyPrefab = Resources.Load<GameObject>("UI/Panels/HistoryPanel");
                if (historyPrefab != null)
                {
                    rowTemplate = historyPrefab.GetComponentInChildren<RecordRow>(true)?.gameObject;
                    cardCellTemplate = historyPrefab.GetComponentInChildren<RecordCardCell>(true);
                    exhibitTemplate = historyPrefab.GetComponentInChildren<ExhibitWidget>(true);
                }
            }
            catch { }

            // Parent under dialog layer (private) if possible.
            Transform parent = null;
            try
            {
                var traverse = HarmonyLib.Traverse.Create(UiManager.Instance);
                var layer = traverse.Field("dialogLayer").GetValue<RectTransform>();
                parent = layer != null ? layer.transform : UiManager.Instance.transform;
            }
            catch
            {
                parent = UiManager.Instance.transform;
            }

            var root = new GameObject("NetworkPlugin_TradeDetailDialog_v6");
            root.SetActive(false);
            root.transform.SetParent(parent, false);

            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var overlay = root.AddComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.55f);
            overlay.raycastTarget = true;

            // Prefer an in-game authored dialog prefab as the main frame so visuals match vanilla.
            // We do NOT call UiDialog.Show(); we only reuse the prefab's graphics and TMP/button styles.
            RectTransform panelRt = null;
            TextMeshProUGUI frameTextTemplate = null;
            CommonButtonWidget frameButtonTemplate = null;
            try
            {
                var framePrefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
                if (framePrefab != null)
                {
                    var frame = UnityEngine.Object.Instantiate(framePrefab, root.transform, false);
                    frame.name = "TradeDetailFrame";
                    frame.SetActive(true);

                    var frameRt = frame.GetComponent<RectTransform>();
                    if (frameRt != null)
                    {
                        frameRt.anchorMin = new Vector2(0.06f, 0.06f);
                        frameRt.anchorMax = new Vector2(0.94f, 0.94f);
                        frameRt.offsetMin = Vector2.zero;
                        frameRt.offsetMax = Vector2.zero;
                    }

                    var msg = frame.GetComponentInChildren<MessageDialog>(true);
                    if (msg != null)
                    {
                        var mainText = GetDialogField<TextMeshProUGUI>(msg, "mainText");
                        var subText = GetDialogField<TextMeshProUGUI>(msg, "subText");
                        var singleConfirm = GetDialogField<Button>(msg, "singleConfirmButton");
                        var confirm = GetDialogField<Button>(msg, "confirmButton");
                        var cancel = GetDialogField<Button>(msg, "cancelButton");

                        frameTextTemplate = mainText != null ? mainText : subText;

                        // Prioritize non-red buttons: singleConfirm -> confirm -> cancel as last resort.
                        frameButtonTemplate = TryFindButtonWidget(singleConfirm) 
                                           ?? TryFindButtonWidget(confirm) 
                                           ?? TryFindButtonWidget(cancel);

                        HideDialogText(mainText);
                        HideDialogText(subText);
                        HideDialogButton(singleConfirm);
                        HideDialogButton(confirm);
                        HideDialogButton(cancel);

                        msg.enabled = false;

                        var cancelRt = cancel != null ? cancel.GetComponent<RectTransform>() : null;
                        panelRt = TryFindCommonAncestorRect(mainText != null ? mainText.rectTransform : null, cancelRt)
                                  ?? TryFindCommonAncestorRect(subText != null ? subText.rectTransform : null, cancelRt)
                                  ?? frameRt;
                    }

                    if (panelRt == null)
                    {
                        panelRt = frameRt;
                    }
                }
            }
            catch
            {
                panelRt = null;
                frameTextTemplate = null;
                frameButtonTemplate = null;
            }

            // Fallback if prefab is unavailable: create a simple panel (still uses in-game background sprite).
            if (panelRt == null)
            {
                var panelGo = new GameObject("Panel");
                panelGo.transform.SetParent(root.transform, false);
                panelRt = panelGo.AddComponent<RectTransform>();
                panelRt.anchorMin = new Vector2(0.12f, 0.10f);
                panelRt.anchorMax = new Vector2(0.88f, 0.90f);
                panelRt.offsetMin = Vector2.zero;
                panelRt.offsetMax = Vector2.zero;

                var panelBg = panelGo.AddComponent<Image>();
                panelBg.sprite = null;
                panelBg.color = new Color(0.05f, 0.05f, 0.05f, 0.90f);
                panelBg.raycastTarget = true;
            }

            if (frameTextTemplate != null)
            {
                textTemplate = frameTextTemplate;
            }
            if (frameButtonTemplate != null)
            {
                buttonTemplate = frameButtonTemplate;
            }

            var dialog = root.AddComponent<TradeDetailDialog>();
            dialog.BindRuntime(buttonTemplate, textTemplate, rowTemplate, cardCellTemplate, exhibitTemplate, panelRt);

            root.SetActive(true);
            root.SetActive(false);

            return dialog;
        }
        catch
        {
            return null;
        }
    }

    private static void HideDialogText(TextMeshProUGUI tmp)
    {
        try
        {
            if (tmp != null)
            {
                tmp.text = string.Empty;
                tmp.raycastTarget = false;
                tmp.gameObject.SetActive(false);
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void HideDialogButton(Button b)
    {
        try
        {
            if (b != null)
            {
                b.onClick.RemoveAllListeners();
                b.gameObject.SetActive(false);
            }
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

            var fi = typeof(MessageDialog).GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
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

    private static CommonButtonWidget TryFindButtonWidget(Button b)
    {
        try
        {
            if (b == null)
            {
                return null;
            }

            var w = b.GetComponent<CommonButtonWidget>();
            if (w != null)
            {
                return w;
            }

            return b.GetComponentInParent<CommonButtonWidget>(true);
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

    private static CommonButtonWidget TryPickButtonTemplate()
    {
        try
        {
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

                int buttons = 0;
                int nodes = 0;
                try { buttons = c.GetComponentsInChildren<Button>(true)?.Length ?? 0; } catch { buttons = 0; }
                try { nodes = c.GetComponentsInChildren<Transform>(true)?.Length ?? 0; } catch { nodes = 0; }
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

    private static TextMeshProUGUI TryPickTextTemplate()
    {
        try
        {
            return UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None)?.FirstOrDefault(t => t != null);
        }
        catch
        {
            return null;
        }
    }
}
