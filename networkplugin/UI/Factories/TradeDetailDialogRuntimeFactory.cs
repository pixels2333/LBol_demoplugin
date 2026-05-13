using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using NetworkPlugin.UI.Dialogs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Factories;

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

            CommonButtonWidget buttonTemplate;
            {
                var __candidates = UnityEngine.Object.FindObjectsByType<CommonButtonWidget>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                CommonButtonWidget __best = null;
                int __bestScore = int.MaxValue;
                if (__candidates != null)
                {
                    foreach (var __c in __candidates)
                    {
                        if (__c == null || __c.button == null) continue;
                        int __buttons = __c.GetComponentsInChildren<Button>(true).Length;
                        if (__buttons == 0) continue;
                        int __nodes = __c.GetComponentsInChildren<Transform>(true).Length;
                        int __score = (__buttons * 1000) + __nodes;
                        if (__score < __bestScore) { __bestScore = __score; __best = __c; }
                    }
                }
                buttonTemplate = __best;
            }
            var textTemplate = UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                ?.FirstOrDefault(t => t != null);
            GameObject rowTemplate = null;
            RecordCardCell cardCellTemplate = null;
            ExhibitWidget exhibitTemplate = null;

            var historyPrefab = Resources.Load<GameObject>("UI/Panels/HistoryPanel");
            if (historyPrefab != null)
            {
                rowTemplate = historyPrefab.GetComponentInChildren<RecordRow>(true)?.gameObject;
                cardCellTemplate = historyPrefab.GetComponentInChildren<RecordCardCell>(true);
                exhibitTemplate = historyPrefab.GetComponentInChildren<ExhibitWidget>(true);
            }

            // 尽量挂到对话框层（私有字段）下。
            Transform parent = null;
            try
            {
                Traverse traverse = HarmonyLib.Traverse.Create(UiManager.Instance);
                var layer = traverse.Field("dialogLayer").GetValue<RectTransform>();
                parent = layer != null ? layer.transform : UiManager.Instance.transform;
            }
            catch
            {
                parent = UiManager.Instance.transform;
            }

            GameObject root = new GameObject("NetworkPlugin_TradeDetailDialog_v6");
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

            // 优先复用游戏内现成的对话框 prefab，让外观尽量贴近原版。
            // 这里不调用 `UiDialog.Show()`，只借用 prefab 的图形、TMP 和按钮样式。
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

                        // 优先使用非红色按钮样式：singleConfirm -> confirm -> cancel。
                        frameButtonTemplate = TryFindButtonWidget(singleConfirm) 
                                           ?? TryFindButtonWidget(confirm) 
                                           ?? TryFindButtonWidget(cancel);

                        HideDialogText(mainText);
                        HideDialogText(subText);
                        HideDialogButton(singleConfirm);
                        HideDialogButton(confirm);
                        HideDialogButton(cancel);

                        msg.enabled = false;

                        var cancelRt = cancel?.GetComponent<RectTransform>();
                        panelRt = TryFindCommonAncestorRect(mainText?.rectTransform, cancelRt)
                                  ?? TryFindCommonAncestorRect(subText?.rectTransform, cancelRt)
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

            // 如果拿不到 prefab，就退化为一个简化面板。
            if (panelRt == null)
            {
                GameObject panelGo = new GameObject("Panel");
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
        if (tmp == null)
        {
            return;
        }

        tmp.text = string.Empty;
        tmp.raycastTarget = false;
        tmp.gameObject.SetActive(false);
    }

    private static void HideDialogButton(Button b)
    {
        if (b == null)
        {
            return;
        }

        b.onClick.RemoveAllListeners();
        b.gameObject.SetActive(false);
    }

    private static T GetDialogField<T>(MessageDialog dialog, string fieldName) where T : class
    {
        if (dialog == null || string.IsNullOrWhiteSpace(fieldName))
        {
            return null;
        }

        var fi = typeof(MessageDialog).GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        return fi?.GetValue(dialog) as T;
    }

    private static CommonButtonWidget TryFindButtonWidget(Button b)
    {
        if (b == null)
        {
            return null;
        }

        var w = b.GetComponent<CommonButtonWidget>();
        return w ?? b.GetComponentInParent<CommonButtonWidget>(true);
    }

    private static RectTransform TryFindCommonAncestorRect(RectTransform a, RectTransform b)
    {
        if (a == null || b == null)
        {
            return null;
        }

        HashSet<Transform> ancestors = new HashSet<Transform>();
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

}
