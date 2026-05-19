using System;
using System.Collections.Generic;
using LBoL.Core.Cards;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Dialogs;

// 弹层创建 —— overlay/full overlay 的 UI 工厂方法
public sealed partial class TradeDetailDialog
{
    #region 弹层创建
    private GameObject CreateCardSelectionOverlay()
    {
        // 独立交易确认面板：上半显示对方卡牌，下半显示我方卡牌，底部为可选卡牌。
        try
        {
            GameObject root = new GameObject("CardPicker");
            root.transform.SetParent(transform, false);
            root.SetActive(false);

            RectTransform rt = root.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image blocker = root.AddComponent<Image>();
            blocker.color = new Color(0f, 0f, 0f, 0f);
            blocker.raycastTarget = true;

            GameObject prefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
            if (prefab == null)
            {
                return CreateFullOverlayFallback(root, rt, "交易确认面板");
            }

            GameObject frame = Instantiate(prefab, root.transform, false);
            frame.name = "Frame";
            frame.SetActive(true);

            RectTransform frameRt = frame.GetComponent<RectTransform>();
            if (frameRt != null)
            {
                frameRt.anchorMin = new Vector2(0.05f, 0.05f);
                frameRt.anchorMax = new Vector2(0.95f, 0.95f);
                frameRt.offsetMin = Vector2.zero;
                frameRt.offsetMax = Vector2.zero;
            }

            MessageDialog dialog = frame.GetComponentInChildren<MessageDialog>(true);
            if (dialog == null)
            {
                return CreateFullOverlayFallback(root, rt, "交易确认面板");
            }

            TextMeshProUGUI mainText = GetDialogField<TextMeshProUGUI>(dialog, "mainText");
            TextMeshProUGUI subText = GetDialogField<TextMeshProUGUI>(dialog, "subText");
            Button singleConfirm = GetDialogField<Button>(dialog, "singleConfirmButton");
            Button confirm = GetDialogField<Button>(dialog, "confirmButton");
            Button cancel = GetDialogField<Button>(dialog, "cancelButton");

            HideDialogButton(singleConfirm);

            if (mainText != null)
            {
                mainText.gameObject.SetActive(true);
                mainText.text = "交易确认面板";
                mainText.alignment = TextAlignmentOptions.Center;
                mainText.raycastTarget = false;
                Color c = mainText.color;
                c.a = 1f;
                mainText.color = c;
            }

            RectTransform panelRect = TryFindCommonAncestorRect(mainText?.rectTransform, cancel?.GetComponent<RectTransform>())
                ?? (frameRt != null ? frameRt : frame.GetComponent<RectTransform>());
            if (panelRect == null)
            {
                panelRect = rt;
            }

            if (subText != null)
            {
                subText.gameObject.SetActive(false);
            }

            TextMeshProUGUI hint = CloneText(panelRect, "CardPickerHint", 18, TextAlignmentOptions.Center);
            SetRect(hint.rectTransform, 0.08f, 0.84f, 0.92f, 0.89f);

            TextMeshProUGUI remoteTitle = CloneText(panelRect, "RemoteSelectedTitle", 20, TextAlignmentOptions.Left);
            remoteTitle.text = "对方已选卡牌";
            SetRect(remoteTitle.rectTransform, 0.08f, 0.77f, 0.92f, 0.82f);

            Transform remoteList = CreateScrollList(panelRect, "RemoteSelectedList", 0.08f, 0.60f, 0.92f, 0.76f);
            AddListBackground(remoteList);

            TextMeshProUGUI localTitle = CloneText(panelRect, "LocalSelectedTitle", 20, TextAlignmentOptions.Left);
            localTitle.text = "我方已选卡牌";
            SetRect(localTitle.rectTransform, 0.08f, 0.53f, 0.92f, 0.58f);

            Transform localList = CreateScrollList(panelRect, "LocalSelectedList", 0.08f, 0.36f, 0.92f, 0.52f);
            AddListBackground(localList);

            TextMeshProUGUI availableTitle = CloneText(panelRect, "AvailableCardTitle", 20, TextAlignmentOptions.Left);
            availableTitle.text = "可选卡牌";
            SetRect(availableTitle.rectTransform, 0.08f, 0.29f, 0.92f, 0.34f);

            Transform candidateList = CreateScrollList(panelRect, "AvailableCardList", 0.08f, 0.14f, 0.92f, 0.28f);
            AddListBackground(candidateList);

            CommonButtonWidget backButton = CloneButton(panelRect, "CardPickerBack", "返回");
            SetRect(backButton.GetComponent<RectTransform>(), 0.26f, 0.05f, 0.44f, 0.11f);
            backButton.button.onClick.RemoveAllListeners();
            backButton.button.onClick.AddListener(() => CloseCardPickerOverlay(applyChanges: false, closeOnly: false));

            CommonButtonWidget confirmButtonWidget = CloneButton(panelRect, "CardPickerConfirm", "确认选牌");
            SetRect(confirmButtonWidget.GetComponent<RectTransform>(), 0.56f, 0.05f, 0.74f, 0.11f);
            confirmButtonWidget.button.onClick.RemoveAllListeners();
            confirmButtonWidget.button.onClick.AddListener(new UnityAction(() =>
            {
                if (_cardPickerRoot == null)
                {
                    return;
                }

                CardPickerPanelTag tag = _cardPickerRoot.GetComponent<CardPickerPanelTag>();
                int targetCount = tag?.TargetSelectCount ?? 0;
                if (targetCount > 0 && _localCards.Count != targetCount)
                {
                    if (tag?.HintText != null)
                    {
                        tag.HintText.text = $"请选择 {targetCount} 张卡牌（当前 {_localCards.Count}/{targetCount}）";
                    }
                    return;
                }

                CloseCardPickerOverlay(applyChanges: true, closeOnly: false);
                TrySendOfferUpdate();
            }));

            CardPickerPanelTag panelTag = root.AddComponent<CardPickerPanelTag>();
            panelTag.HintText = hint;
            panelTag.RemoteSelectedList = remoteList;
            panelTag.LocalSelectedList = localList;
            panelTag.CandidateList = candidateList;
            panelTag.ConfirmButton = confirmButtonWidget;

            dialog.enabled = false;
            return root;
        }
        catch
        {
            return null;
        }
    }

    private GameObject CreateFullOverlay(string name, string titleText)
    {
        // 优先复用游戏内 MessageDialog prefab，保证视觉风格一致。
        try
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(transform, false);
            root.SetActive(false);

            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // 透明遮罩层，避免 prefab 的 mask 失效时点击穿透到底层。
            var blocker = root.AddComponent<Image>();
            blocker.color = new Color(0f, 0f, 0f, 0f);
            blocker.raycastTarget = true;

            var prefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
            if (prefab == null)
            {
                return CreateFullOverlayFallback(root, rt, titleText);
            }

            var frame = Instantiate(prefab, root.transform, false);
            frame.name = "Frame";
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
            if (dialog == null)
            {
                return CreateFullOverlayFallback(root, rt, titleText);
            }

            var mainText = GetDialogField<TextMeshProUGUI>(dialog, "mainText");
            var subText = GetDialogField<TextMeshProUGUI>(dialog, "subText");
            var singleConfirm = GetDialogField<Button>(dialog, "singleConfirmButton");
            var confirm = GetDialogField<Button>(dialog, "confirmButton");
            var cancel = GetDialogField<Button>(dialog, "cancelButton");

            HideDialogButton(singleConfirm);

            if (mainText != null)
            {
                mainText.gameObject.SetActive(true);
                mainText.text = titleText;
                mainText.alignment = TextAlignmentOptions.Center;
                mainText.raycastTarget = false;
                var c = mainText.color;
                c.a = 1f;
                mainText.color = c;
            }

            // 保留 subText 参与布局，但把它隐藏掉，避免 prefab 排版塌掉。
            RectTransform panelRect = null;
            RectTransform subTextRect = subText?.rectTransform;
            if (subText != null)
            {
                subText.gameObject.SetActive(true);
                subText.text = string.Empty;
                subText.raycastTarget = false;
                var c = subText.color;
                c.a = 0f;
                subText.color = c;
            }

            panelRect = TryFindCommonAncestorRect(mainText?.rectTransform,
                cancel?.GetComponent<RectTransform>())
                ?? (frameRt != null ? frameRt : frame.GetComponent<RectTransform>());

            if (panelRect == null)
            {
                panelRect = rt;
            }

            // 把列表放到中间区域。
            var list = CreateScrollList(panelRect, "List", 0.10f, 0.18f, 0.90f, 0.86f);
            _ = list.gameObject.AddComponent<PickerListTag>();

            if (cancel != null)
            {
                cancel.onClick.RemoveAllListeners();
                cancel.gameObject.SetActive(true);
                SetButtonLabel(cancel, "取消");
                cancel.onClick.AddListener(() =>
                {
                    root.SetActive(false);
                    _canvasGroup.interactable = true;
                });
            }

            if (confirm != null)
            {
                confirm.onClick.RemoveAllListeners();
                confirm.gameObject.SetActive(true);
                SetButtonLabel(confirm, "确定");
                confirm.onClick.AddListener(() =>
                {
                    root.SetActive(false);
                    _canvasGroup.interactable = true;
                    RefreshLocalUi();
                    TrySendOfferUpdate();
                });
            }

            // 确保列表不会压在按钮上面。
            if (cancel != null)
            {
                cancel.transform.SetAsLastSibling();
            }
            if (confirm != null)
            {
                confirm.transform.SetAsLastSibling();
            }

            dialog.enabled = false;
            return root;
        }
        catch
        {
            // 如果构建原生风格 overlay 失败，就回退到旧的运行时实现。
            GameObject root = new GameObject(name);
            root.transform.SetParent(transform, false);
            root.SetActive(false);
            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return CreateFullOverlayFallback(root, rt, titleText);
        }
    }

    private GameObject CreateFullOverlayFallback(GameObject root, RectTransform rt, string titleText)
    {
        try
        {
            var bg = root.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);
            bg.raycastTarget = true;

            var title = CloneText(rt, "Title", 26, TextAlignmentOptions.Center);
            title.text = titleText;
            SetRect(title.rectTransform, 0.10f, 0.88f, 0.90f, 0.97f);

            var list = CreateScrollList(rt, "List", 0.10f, 0.18f, 0.90f, 0.86f);
            _ = list.gameObject.AddComponent<PickerListTag>();

            var cancel = CloneButton(rt, "Cancel", "取消");
            SetRect(cancel.GetComponent<RectTransform>(), 0.25f, 0.06f, 0.45f, 0.14f);
            cancel.button.onClick.RemoveAllListeners();
            cancel.button.onClick.AddListener(new UnityAction(() =>
            {
                root.SetActive(false);
                _canvasGroup.interactable = true;
            }));

            var ok = CloneButton(rt, "OK", "确定");
            SetRect(ok.GetComponent<RectTransform>(), 0.55f, 0.06f, 0.75f, 0.14f);
            ok.button.onClick.RemoveAllListeners();
            ok.button.onClick.AddListener(new UnityAction(() =>
            {
                root.SetActive(false);
                _canvasGroup.interactable = true;
                RefreshLocalUi();
                TrySendOfferUpdate();
            }));
        }
        catch
        {
            // ignored
        }

        return root;
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

    #endregion
}
