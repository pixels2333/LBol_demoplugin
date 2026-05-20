using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Presentation;
using LBoL.Presentation.UI.Widgets;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Dialogs;

// UI 渲染与交互事件 —— 管理 TradeDetailDialog 的视觉布局和用户交互
public sealed partial class TradeDetailDialog
{
    #region UI 渲染

    private void RefreshLocalUi()
    {
        _localMoneyText.text = $"金币: {_localMoney}";
        _localExhibitsText.text = $"遗物: {_localExhibitIds.Count}";

        RebuildCardList(_localCardsList, _localCards, isLocal: true);
        RebuildExhibitList(_localExhibitsList, _localExhibitIds.ToList(), isLocal: true);
    }

    private void RefreshRemoteUi(TradeSyncPatch.TradeSessionState state)
    {
        if (state == null)
        {
            _remoteMoneyText.text = "金币: 0";
            _remoteExhibitsText.text = "遗物: 0";
            RebuildCardList(_remoteCardsList, (List<TradeSyncPatch.CardRef>)null);
            RebuildExhibitList(_remoteExhibitsList, (List<string>)null, isLocal: false);
            return;
        }

        bool localIsA = IsPlayerA(state);

        int theirMoney = GetRemoteMoney(state, localIsA);
        List<TradeSyncPatch.ExhibitRef> theirEx = GetRemoteExhibits(state, localIsA);

        _remoteMoneyText.text = $"金币: {Mathf.Max(0, theirMoney)}";
        _remoteExhibitsText.text = $"遗物: {theirEx?.Count ?? 0}";

        var theirCards = GetRemoteOffer(state, localIsA);
        RebuildCardList(_remoteCardsList, theirCards);
        RebuildExhibitList(_remoteExhibitsList, theirEx);
    }

    private void RebuildExhibitList(Transform listRoot, List<string> exhibitIds, bool isLocal)
    {
        if (!BeginRebuildList(listRoot, exhibitIds, out _)) return;

        foreach (var exId in exhibitIds)
        {
            if (string.IsNullOrEmpty(exId)) continue;

            var row = CloneListItem(listRoot, $"Ex_{exId}", exId, false);
            if (isLocal)
            {
                row.button.onClick.AddListener(() =>
                {
                    AudioManager.Button(0);
                    _localExhibitIds.Remove(exId);
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

    private void RebuildExhibitList(Transform listRoot, List<TradeSyncPatch.ExhibitRef> exhibits)
    {
        if (!BeginRebuildList(listRoot, exhibits, out _)) return;

        foreach (var ex in exhibits)
        {
            var exId = ex?.ExhibitId;
            if (string.IsNullOrEmpty(exId)) continue;

            var row = CloneListItem(listRoot, $"Ex_{exId}", exId, false);
            row.button.interactable = false;
        }
    }

    private void EnsureUi()
    {
        if (_panelRoot == null)
        {
            RectTransform panel = transform.Find("Panel") as RectTransform;
            _panelRoot = panel != null ? panel : GetComponent<RectTransform>();
        }

        var bg = _panelRoot.GetComponent<Image>();
        if (bg == null) bg = _panelRoot.gameObject.AddComponent<Image>();
        bg.color = new Color(0.04f, 0.04f, 0.04f, 0.85f);

        _titleText = CloneText(_panelRoot, "Title", 30, TextAlignmentOptions.Center);
        SetRect(_titleText.rectTransform, 0.06f, 0.90f, 0.94f, 0.98f);

        _statusText = CloneText(_panelRoot, "Status", 18, TextAlignmentOptions.Center);
        SetRect(_statusText.rectTransform, 0.06f, 0.85f, 0.94f, 0.90f);

        // --- Left Side (Local) ---
        _localCardsTitle = CloneText(_panelRoot, "LocalTitle", 22, TextAlignmentOptions.Left);
        _localCardsTitle.text = "我的报价";
        SetRect(_localCardsTitle.rectTransform, 0.06f, 0.80f, 0.46f, 0.85f);

        _localCardsList = CreateScrollList(_panelRoot, "LocalCards", 0.06f, 0.55f, 0.46f, 0.78f);
        AddListBackground(_localCardsList);

        _localExhibitsList = CreateScrollList(_panelRoot, "LocalExhibits", 0.06f, 0.35f, 0.46f, 0.53f);
        AddListBackground(_localExhibitsList);

        // 操作按钮放到报价区域附近，减少来回移动视线。
        _addCardButton = CloneButton(_panelRoot, "AddCard", "+ 卡牌");
        SetRect(_addCardButton.GetComponent<RectTransform>(), 0.06f, 0.28f, 0.18f, 0.33f);
        _addCardButton.button.onClick.RemoveAllListeners();
        _addCardButton.button.onClick.AddListener(new UnityAction(ShowCardPicker));

        _editExhibitButton = CloneButton(_panelRoot, "EditEx", "+ 遗物");
        SetRect(_editExhibitButton.GetComponent<RectTransform>(), 0.19f, 0.28f, 0.31f, 0.33f);
        _editExhibitButton.button.onClick.RemoveAllListeners();
        _editExhibitButton.button.onClick.AddListener(new UnityAction(ShowExhibitPicker));

        _localMoneyText = CloneText(_panelRoot, "LocalMoney", 18, TextAlignmentOptions.Left);
        SetRect(_localMoneyText.rectTransform, 0.06f, 0.23f, 0.22f, 0.27f);

        _moneyMinusButton = CloneButton(_panelRoot, "MoneyMinus", "-");
        SetRect(_moneyMinusButton.GetComponent<RectTransform>(), 0.22f, 0.23f, 0.26f, 0.27f);
        _moneyMinusButton.button.onClick.RemoveAllListeners();
        _moneyMinusButton.button.onClick.AddListener(new UnityAction(() => ChangeMoney(-10)));

        _moneyPlusButton = CloneButton(_panelRoot, "MoneyPlus", "+");
        SetRect(_moneyPlusButton.GetComponent<RectTransform>(), 0.27f, 0.23f, 0.31f, 0.27f);
        _moneyPlusButton.button.onClick.RemoveAllListeners();
        _moneyPlusButton.button.onClick.AddListener(new UnityAction(() => ChangeMoney(+10)));

        _localExhibitsText = CloneText(_panelRoot, "LocalEx", 18, TextAlignmentOptions.Left);
        SetRect(_localExhibitsText.rectTransform, 0.34f, 0.23f, 0.46f, 0.27f);

        // --- Right Side (Remote) ---
        _remoteCardsTitle = CloneText(_panelRoot, "RemoteTitle", 22, TextAlignmentOptions.Left);
        _remoteCardsTitle.text = "对方报价";
        SetRect(_remoteCardsTitle.rectTransform, 0.54f, 0.80f, 0.94f, 0.85f);

        _remoteCardsList = CreateScrollList(_panelRoot, "RemoteCards", 0.54f, 0.55f, 0.94f, 0.78f);
        AddListBackground(_remoteCardsList);

        _remoteExhibitsList = CreateScrollList(_panelRoot, "RemoteExhibits", 0.54f, 0.35f, 0.94f, 0.53f);
        AddListBackground(_remoteExhibitsList);

        _remoteMoneyText = CloneText(_panelRoot, "RemoteMoney", 18, TextAlignmentOptions.Left);
        SetRect(_remoteMoneyText.rectTransform, 0.54f, 0.23f, 0.78f, 0.27f);

        _remoteExhibitsText = CloneText(_panelRoot, "RemoteEx", 18, TextAlignmentOptions.Left);
        SetRect(_remoteExhibitsText.rectTransform, 0.78f, 0.23f, 0.94f, 0.27f);

        // --- Bottom Controls ---
        _confirmButton = CloneButton(_panelRoot, "Confirm", "确认交易");
        SetRect(_confirmButton.GetComponent<RectTransform>(), 0.32f, 0.08f, 0.48f, 0.16f);
        _confirmButton.button.onClick.RemoveAllListeners();
        _confirmButton.button.onClick.AddListener(new UnityAction(OnConfirmClick));

        _cancelButton = CloneButton(_panelRoot, "Cancel", "取消交易");
        SetRect(_cancelButton.GetComponent<RectTransform>(), 0.52f, 0.08f, 0.68f, 0.16f);
        _cancelButton.button.onClick.RemoveAllListeners();
        _cancelButton.button.onClick.AddListener(new UnityAction(() => { AudioManager.Button(0); OnCancel(); }));

        DisableTooltipBehaviours(gameObject);
    }

    private void AddListBackground(Transform listRoot)
    {
        if (listRoot == null || listRoot.parent == null) return;
        var parent = listRoot.parent;

        // 在 ScrollRect 背后补一个背景对象，没有就创建。
        var bgName = "ListBg_" + listRoot.name;
        var bgTransform = parent.Find(bgName);
        if (bgTransform == null)
        {
            GameObject go = new GameObject(bgName);
            go.transform.SetParent(parent, false);
            go.transform.SetAsFirstSibling();
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(-4, -4);
            rt.offsetMax = new Vector2(4, 4);

            var img = go.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.35f); // 简单的半透明深色底。
        }
    }

    private void OnConfirmClick()
    {
        try
        {
            TradeSyncPatch.TradeSessionState state = TradeSyncPatch.GetLastKnown(_tradeId);
            if (state == null)
            {
                return;
            }

            UpdateConfirmInteractable(state);
            if (_confirmButton?.button != null && !_confirmButton.button.interactable)
            {
                return;
            }

            // 播放确认音效。
            AudioManager.Button(0);

            TradeSyncPatch.RequestConfirm(_tradeId, _selfId);
            _statusText.text = "已确认，等待对方...";
        }
        catch
        {
            // 忽略
        }
    }

    private void ChangeMoney(int delta)
    {
        if (_isApplyingState)
        {
            return;
        }

        int current = CurrentGameRun?.Money ?? 0;

        int next = Mathf.Clamp(_localMoney + delta, 0, Mathf.Min(MaxMoneyOffer, current));
        if (next != _localMoney)
        {
            AudioManager.Button(0);
            _localMoney = next;
            RefreshLocalUi();
            TrySendOfferUpdate();
        }
    }

    private void ShowCardPicker()
    {
        if (_isApplyingState)
        {
            return;
        }

        bool hasRun = GameStateUtils.TryGetCurrentGameRun(out GameRunController currentRun, out string runSource);
        int currentDeckCount = currentRun?.BaseDeck?.Count ?? 0;
        Plugin.Logger?.LogInfo(
            $"[TradeDetailDialog] ShowCardPicker: tradeId={_tradeId ?? "<null>"}, localCards={_localCards.Count}, initialDeckCount={_initialDeckCards.Count}, hasRun={hasRun}, runSource={runSource ?? "<null>"}, currentDeckCount={currentDeckCount}, pickerExists={(_cardPickerRoot != null)}");

        AudioManager.Card(3);
        if (_cardPickerRoot == null)
        {
            _cardPickerRoot = CreateCardSelectionOverlay();
            Plugin.Logger?.LogInfo($"[TradeDetailDialog] EnsureCardPicker created: tradeId={_tradeId ?? "<null>"}, pickerExists={(_cardPickerRoot != null)}");
        }
        if (_cardPickerRoot == null)
        {
            TryShowTopMessage("卡牌选择界面不可用。");
            return;
        }
        _cardPickerOriginalCards.Clear();
        _cardPickerOriginalCards.AddRange(_localCards.Where(c => c != null));
        _cardPickerEditing = true;
        RebuildCardPicker();
        _cardPickerRoot.SetActive(true);
        _canvasGroup.interactable = false;
        StartCoroutine(CoRefreshCardPickerNextFrame());
    }

    private IEnumerator CoRefreshCardPickerNextFrame()
    {
        yield return null;

        if (_cardPickerRoot == null || !_cardPickerRoot.activeSelf)
        {
            Plugin.Logger?.LogInfo($"[TradeDetailDialog] CoRefreshCardPickerNextFrame skipped: tradeId={_tradeId ?? "<null>"}, pickerExists={(_cardPickerRoot != null)}, pickerActive={(_cardPickerRoot != null && _cardPickerRoot.activeSelf)}");
            yield break;
        }

        Plugin.Logger?.LogInfo($"[TradeDetailDialog] CoRefreshCardPickerNextFrame rebuilding: tradeId={_tradeId ?? "<null>"}");
        RebuildCardPicker();
    }

    private void ShowExhibitPicker()
    {
        if (_isApplyingState)
        {
            return;
        }

        AudioManager.Card(3);
        if (_exhibitPickerRoot == null)
        {
            _exhibitPickerRoot = CreateFullOverlay("ExhibitPicker", "选择要交易的遗物");
        }
        RebuildExhibitPicker();
        _exhibitPickerRoot.SetActive(true);
        _canvasGroup.interactable = false;
    }

    private void CloseCardPickerOverlay(bool applyChanges, bool closeOnly)
    {
        if (_cardPickerRoot == null || !_cardPickerRoot.activeSelf)
        {
            _cardPickerEditing = false;
            return;
        }

        if (!applyChanges)
        {
            _localCards.Clear();
            _localCards.AddRange(_cardPickerOriginalCards.Where(c => c != null));
        }

        _cardPickerRoot.SetActive(false);
        _canvasGroup.interactable = true;
        _cardPickerEditing = false;

        if (!closeOnly)
        {
            RefreshLocalUi();
            RefreshRemoteUi(TradeSyncPatch.GetLastKnown(_tradeId));
        }
    }

    #endregion
}
