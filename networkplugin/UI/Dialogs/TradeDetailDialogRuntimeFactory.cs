using System;
using System.Linq;
using LBoL.Presentation;
using LBoL.Presentation.UI;
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
            var existing = UnityEngine.Object.FindObjectOfType<TradeDetailDialog>(true);
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

            var root = new GameObject("NetworkPlugin_TradeDetailDialog");
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

            var panelGo = new GameObject("Panel");
            panelGo.transform.SetParent(root.transform, false);
            var panelRt = panelGo.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.12f, 0.10f);
            panelRt.anchorMax = new Vector2(0.88f, 0.90f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            var panelBg = panelGo.AddComponent<Image>();
            try { panelBg.sprite = ResourcesHelper.LoadUiBackground("Adventure"); } catch { panelBg.sprite = null; }
            panelBg.color = new Color(0.05f, 0.05f, 0.05f, 0.92f);
            panelBg.raycastTarget = true;

            var dialog = root.AddComponent<TradeDetailDialog>();
            dialog.BindRuntime(buttonTemplate, textTemplate, panelRt);

            root.SetActive(true);
            root.SetActive(false);

            return dialog;
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
            return UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>(true)?.FirstOrDefault(t => t != null);
        }
        catch
        {
            return null;
        }
    }
}
