using System;
using System.Linq;
using HarmonyLib;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Widgets;
using NetworkPlugin.UI.Dialogs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Factories;

internal static class TradeRequestConfirmDialogRuntimeFactory
{
    internal static TradeRequestConfirmDialog GetOrCreate()
    {
        try
        {
            var existing = UnityEngine.Object.FindAnyObjectByType<TradeRequestConfirmDialog>(FindObjectsInactive.Include);
            if (existing != null)
            {
                return existing;
            }

            if (!UiManager.IsInitialized || UiManager.Instance == null)
            {
                return null;
            }

            var framePrefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
            var textTemplate = framePrefab != null ? framePrefab.GetComponentInChildren<TextMeshProUGUI>(true) : null;

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

            GameObject root = new GameObject("NetworkPlugin_TradeRequestConfirmDialog");
            root.SetActive(false);
            root.transform.SetParent(parent, false);

            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var overlay = root.AddComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.75f);
            overlay.raycastTarget = true;

            RectTransform panelRt = null;
            try
            {
                if (framePrefab != null)
                {
                    var frame = UnityEngine.Object.Instantiate(framePrefab, root.transform, false);
                    frame.name = "TradeConfirmFrame";
                    frame.SetActive(true);

                    var frameRt = frame.GetComponent<RectTransform>();
                    if (frameRt != null)
                    {
                        frameRt.anchorMin = new Vector2(0.0f, 0.5f);
                        frameRt.anchorMax = new Vector2(1.0f, 0.5f);
                        frameRt.pivot = new Vector2(0.5f, 0.5f);
                        frameRt.anchoredPosition = new Vector2(0.0f, -10.0f);
                        frameRt.sizeDelta = new Vector2(0.0f, 840.0f);
                        frameRt.offsetMin = new Vector2(0.0f, -430.0f);
                        frameRt.offsetMax = new Vector2(0.0f, 410.0f);
                    }

                    var bgTransform = frame.transform.Find("Bg") as RectTransform;
                    if (bgTransform != null)
                    {
                        bgTransform.gameObject.SetActive(true);
                        bgTransform.anchorMin = Vector2.zero;
                        bgTransform.anchorMax = Vector2.one;
                        bgTransform.offsetMin = Vector2.zero;
                        bgTransform.offsetMax = Vector2.zero;
                        bgTransform.sizeDelta = Vector2.zero;
                        bgTransform.pivot = new Vector2(0.5f, 0.5f);
                        bgTransform.anchoredPosition = Vector2.zero;

                        var topBorder = bgTransform.Find("Border") as RectTransform;
                        if (topBorder != null)
                        {
                            topBorder.gameObject.SetActive(true);
                            topBorder.anchorMin = new Vector2(0.0f, 1.0f);
                            topBorder.anchorMax = new Vector2(1.0f, 1.0f);
                            topBorder.pivot = new Vector2(0.5f, 1.0f);
                            topBorder.anchoredPosition = Vector2.zero;
                            topBorder.sizeDelta = new Vector2(0.0f, 15.0f);
                        }

                        var bottomBorder = bgTransform.Find("Border (1)") as RectTransform;
                        if (bottomBorder != null)
                        {
                            bottomBorder.gameObject.SetActive(true);
                            bottomBorder.anchorMin = new Vector2(0.0f, 0.0f);
                            bottomBorder.anchorMax = new Vector2(1.0f, 0.0f);
                            bottomBorder.pivot = new Vector2(0.5f, 0.0f);
                            bottomBorder.anchoredPosition = Vector2.zero;
                            bottomBorder.sizeDelta = new Vector2(0.0f, 15.0f);
                        }
                    }

                    frame.transform.Find("Panel")?.gameObject.SetActive(false);
                    frame.transform.Find("Title")?.gameObject.SetActive(false);
                    frame.transform.Find("SingleConfirm")?.gameObject.SetActive(false);

                    var msgComponents = frame.GetComponentsInChildren<MessageDialog>(true);
                    foreach (var msg in msgComponents)
                    {
                        UnityEngine.Object.DestroyImmediate(msg);
                    }

                    panelRt = frameRt;
                }
            }
            catch
            {
                panelRt = null;
            }

            if (panelRt == null)
            {
                GameObject panelGo = new GameObject("Panel");
                panelGo.transform.SetParent(root.transform, false);
                panelRt = panelGo.AddComponent<RectTransform>();
                panelRt.anchorMin = new Vector2(0.0f, 0.5f);
                panelRt.anchorMax = new Vector2(1.0f, 0.5f);
                panelRt.pivot = new Vector2(0.5f, 0.5f);
                panelRt.anchoredPosition = new Vector2(0.0f, -10.0f);
                panelRt.sizeDelta = new Vector2(0.0f, 840.0f);

                var panelBg = panelGo.AddComponent<Image>();
                panelBg.color = new Color(0.06f, 0.06f, 0.06f, 0.92f);
                panelBg.raycastTarget = true;
            }

            var dialog = root.AddComponent<TradeRequestConfirmDialog>();
            dialog.BindRuntime(textTemplate, panelRt);

            root.SetActive(true);
            root.SetActive(false);

            return dialog;
        }
        catch
        {
            return null;
        }
    }
}
