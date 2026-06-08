using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using NetworkPlugin.Patch.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Dialogs;

// 列表与子项构建 —— UI 列表/按钮/文本的工厂方法
public sealed partial class TradeDetailDialog
{
    #region 列表与子项构建

    private void RebuildCardList(Transform listRoot, List<Card> cards, bool isLocal)
    {
        if (!BeginRebuildList(listRoot, cards, out _)) return;

        foreach (var card in cards)
        {
            if (card == null) continue;
            var label = card.Name + (card.IsUpgraded ? "+" : "");
            var row = CloneListItem(listRoot, $"Card_{card.InstanceId}", label, false);
            if (isLocal)
            {
                row.button.onClick.AddListener(() =>
                {
                    AudioManager.Button(0);
                    _localCards.Remove(card);
                    RefreshLocalUi();
                    TrySendOfferUpdate();
                });
            }
            else
            {
                row.button.interactable = false;
            }
        }
    }

    private void RebuildCardList(Transform listRoot, List<TradeSyncPatch.CardRef> cardRefs)
    {
        if (!BeginRebuildList(listRoot, cardRefs, out _)) return;

        foreach (var cardRef in cardRefs)
        {
            if (cardRef == null) continue;
            var label = cardRef.CardName + (cardRef.IsUpgraded ? "+" : "");
            var row = CloneListItem(listRoot, $"RemoteCard_{cardRef.InstanceId}", label, false);
            row.button.interactable = false;
        }
    }

    private bool BeginRebuildList<T>(Transform listRoot, List<T> items, out List<T> outItems)
    {
        outItems = items;
        if (listRoot == null) return false;
        foreach (Transform c in listRoot) Destroy(c.gameObject);
        if (items == null || items.Count == 0)
        {
            var empty = CloneText(listRoot as RectTransform, "Empty", 18, TextAlignmentOptions.Center);
            empty.text = "(无)";
            return false;
        }
        return true;
    }

    private CommonButtonWidget CloneListItem(Transform parent, string name, string label, bool isActive = false)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0f, 44f); 

        var img = go.AddComponent<Image>();
        img.color = isActive ? new Color(0.12f, 0.36f, 0.12f, 0.7f) : new Color(0.1f, 0.1f, 0.1f, 0.5f);
        
        var text = CloneText(rt, "Label", 18, TextAlignmentOptions.Left);
        text.text = label;
        SetRect(text.rectTransform, 0.05f, 0f, 0.85f, 1f);

        var status = CloneText(rt, "Status", 16, TextAlignmentOptions.Right);
        status.text = isActive ? "●" : "";
        SetRect(status.rectTransform, 0.85f, 0f, 0.95f, 1f);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var cbw = go.AddComponent<CommonButtonWidget>();
        cbw.button = btn;
        
        DisableTooltipBehaviours(go);
        return cbw;
    }

    private CommonButtonWidget CloneButton(Transform parent, string name, string label)
    {
        CommonButtonWidget w = null;
        try
        {
            // 直接创建对象，避免克隆模板上的意外组件
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localScale = Vector3.one;
            var img = go.AddComponent<Image>();
            img.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            w = go.AddComponent<CommonButtonWidget>();
            w.button = btn;
        }
        catch (Exception ex) { Plugin.Logger?.LogWarning($"[TradeDetailDialog] 创建按钮失败: name={name}, {ex.Message}"); w = null; }

        try
        {
            SetButtonText(w, label);
            Traverse traverse = HarmonyLib.Traverse.Create(w);
            if (label.Contains("取消") || label.Contains("Cancel") || label.Contains("Back"))
            {
                traverse.Field("buttonBehavior").SetValue(1);
            }
            else
            {
                traverse.Field("buttonBehavior").SetValue(0);
            }
        }
        catch (Exception ex) { Plugin.Logger?.LogWarning($"[TradeDetailDialog] 设置按钮行为/文本失败: label={label}, {ex.Message}"); }

        DisableTooltipBehaviours(w.gameObject);
        return w;
    }

    private void SetButtonText(CommonButtonWidget button, string label)
    {
        if (button == null) return;
        var tmp = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp == null)
        {
            tmp = CloneText(button.transform as RectTransform, "Label", 18, TextAlignmentOptions.Center);
            SetRect(tmp.rectTransform, 0f, 0f, 1f, 1f);
        }
        tmp.text = label;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    private TextMeshProUGUI CloneText(RectTransform parent, string name, int fontSize, TextAlignmentOptions align)
    {
        TextMeshProUGUI t = null;

        try
        {
            // 直接创建对象，避免克隆模板上的意外组件
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localScale = Vector3.one;
            t = go.AddComponent<TextMeshProUGUI>();
            if (_textTemplate != null)
            {
                t.font = _textTemplate.font;
                t.fontSharedMaterial = _textTemplate.fontSharedMaterial;
                t.fontMaterial = _textTemplate.fontMaterial;
            }
        }
        catch
        {
            t = null;
        }

        if (t == null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            t = go.AddComponent<TextMeshProUGUI>();
        }

        t.fontSize = Mathf.Max(t.fontSize, fontSize);
        t.alignment = align;
        t.raycastTarget = false;
        _ = t.rectTransform;
        return t;
    }

    private Transform CreateScrollList(RectTransform parent, string name, float minX, float minY, float maxX, float maxY)
    {
        if (TryCreateInGameScrollList(parent, name, minX, minY, maxX, maxY, out Transform content))
        {
            return content;
        }

        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);

        var rootRt = root.AddComponent<RectTransform>();
        SetRect(rootRt, minX, minY, maxX, maxY);

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(root.transform, false);
        var vpRt = viewport.AddComponent<RectTransform>();
        SetRect(vpRt, 0f, 0f, 1f, 1f);

        var vpImg = viewport.AddComponent<Image>();
        vpImg.color = new Color(0f, 0f, 0f, 0.25f);

        var mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject contentGo = new GameObject("Content");
        contentGo.transform.SetParent(viewport.transform, false);
        var cRt = contentGo.AddComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0f, 1f);
        cRt.anchorMax = new Vector2(1f, 1f);
        cRt.pivot = new Vector2(0.5f, 1f);
        cRt.anchoredPosition = Vector2.zero;
        cRt.sizeDelta = new Vector2(0f, 0f);

        var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.spacing = 6f;
        vlg.padding = new RectOffset(8, 8, 8, 8);

        var fitter = contentGo.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var scroll = root.AddComponent<ScrollRect>();
        scroll.viewport = vpRt;
        scroll.content = cRt;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        return contentGo.transform;
    }

    private bool TryCreateInGameScrollList(
        RectTransform parent,
        string name,
        float minX,
        float minY,
        float maxX,
        float maxY,
        out Transform content)
    {
        content = null;
        GameObject historyInstance = null;

        try
        {
            var historyPrefab = Resources.Load<GameObject>("UI/Panels/HistoryPanel");
            if (historyPrefab == null)
            {
                return false;
            }

            historyInstance = Instantiate(historyPrefab);
            historyInstance.SetActive(false);

            ScrollRect listScrollRect = null;
            try
            {
                var historyPanel = historyInstance.GetComponentInChildren<HistoryPanel>(true);
                if (historyPanel != null)
                {
                    listScrollRect = GetPrivateFieldValue<ScrollRect>(historyPanel, "listScrollRect");
                }
            }
            catch
            {
                listScrollRect = null;
            }

            if (listScrollRect == null)
            {
                foreach (var sr in historyInstance.GetComponentsInChildren<ScrollRect>(true))
                {
                    if (sr == null || sr.content == null)
                    {
                        continue;
                    }

                    var rr = sr.content.GetComponentInChildren<RecordRow>(true);
                    if (rr == null)
                    {
                        continue;
                    }

                    listScrollRect = sr;
                    break;
                }
            }

            if (listScrollRect == null)
            {
                return false;
            }

            var scrollGo = listScrollRect.gameObject;
            scrollGo.name = name;
            listScrollRect.transform.SetParent(parent, false);
            listScrollRect.gameObject.SetActive(true);

            var scrollRt = listScrollRect.GetComponent<RectTransform>();
            if (scrollRt != null)
            {
                SetRect(scrollRt, minX, minY, maxX, maxY);
            }

            var oldContent = listScrollRect.content;
            if (oldContent != null)
            {
                try
                {
                    oldContent.gameObject.SetActive(false);
                }
                catch
                {
                    // ignored
                }
            }

            if (listScrollRect.viewport == null)
            {
                try
                {
                    var mask = listScrollRect.GetComponentInChildren<Mask>(true);
                    if (mask != null)
                    {
                        listScrollRect.viewport = mask.GetComponent<RectTransform>();
                    }
                }
                catch
                {
                    // ignored
                }
            }

            var viewport = listScrollRect.viewport != null ? listScrollRect.viewport : scrollRt;
            if (viewport == null)
            {
                return false;
            }

            GameObject contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewport, false);

            var cRt = contentGo.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0f, 1f);
            cRt.anchorMax = new Vector2(1f, 1f);
            cRt.pivot = new Vector2(0.5f, 1f);
            cRt.anchoredPosition = Vector2.zero;
            cRt.sizeDelta = new Vector2(0f, 0f);

            var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.spacing = 6f;
            vlg.padding = new RectOffset(8, 8, 8, 8);

            var fitter = contentGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            listScrollRect.content = cRt;
            listScrollRect.horizontal = false;
            listScrollRect.vertical = true;
            listScrollRect.movementType = ScrollRect.MovementType.Clamped;

            try
            {
                if (historyInstance != null)
                {
                    Destroy(historyInstance);
                }
            }
            catch
            {
                // ignored
            }

            content = contentGo.transform;
            return true;
        }
        catch
        {
            try
            {
                if (historyInstance != null)
                {
                    Destroy(historyInstance);
                }
            }
            catch
            {
                // ignored
            }

            content = null;
            return false;
        }
    }

    #endregion
}
