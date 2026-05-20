using System;
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
using NetworkPlugin.UI.Rules;

namespace NetworkPlugin.UI.Dialogs;

// 卡牌/遗物选择器 —— 与 TradeDetailDialog 核心逻辑解耦
public sealed partial class TradeDetailDialog
{
    #region UI 辅助方法（选择器标签）

    private sealed class PickerListTag : MonoBehaviour
    {
    }

    private sealed class CardPickerPanelTag : MonoBehaviour
    {
        public TextMeshProUGUI HintText;
        public Transform RemoteSelectedList;
        public Transform LocalSelectedList;
        public Transform CandidateList;
        public CommonButtonWidget ConfirmButton;
        public int TargetSelectCount;
    }

    #endregion

    #region 卡牌/遗物选择器

    private void RebuildCardPicker()
    {
        if (_cardPickerRoot == null)
        {
            Plugin.Logger?.LogWarning($"[TradeDetailDialog] RebuildCardPicker aborted: picker root is null, tradeId={_tradeId ?? "<null>"}");
            return;
        }

        CardPickerPanelTag tag = _cardPickerRoot.GetComponent<CardPickerPanelTag>();
        if (tag == null)
        {
            Plugin.Logger?.LogWarning($"[TradeDetailDialog] RebuildCardPicker aborted: CardPickerPanelTag missing, tradeId={_tradeId ?? "<null>"}");
            return;
        }

        Transform candidateContainer = tag.CandidateList;
        Transform localContainer = tag.LocalSelectedList;
        Transform remoteContainer = tag.RemoteSelectedList;
        if (candidateContainer == null || localContainer == null || remoteContainer == null)
        {
            return;
        }

        // 可选卡牌列表改用 Grid Layout，更接近游戏原生卡库的观看方式。
        var vlg = candidateContainer.GetComponent<VerticalLayoutGroup>();
        if (vlg != null) DestroyImmediate(vlg);

        var glg = candidateContainer.GetComponent<GridLayoutGroup>();
        if (glg == null) glg = candidateContainer.gameObject.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(160, 100);
        glg.spacing = new Vector2(10, 10);
        glg.padding = new RectOffset(10, 10, 10, 10);
        glg.childAlignment = TextAnchor.UpperLeft;

        foreach (Transform c in candidateContainer)
        {
            Destroy(c.gameObject);
        }

        foreach (Transform c in localContainer)
        {
            Destroy(c.gameObject);
        }

        foreach (Transform c in remoteContainer)
        {
            Destroy(c.gameObject);
        }

        bool hasRun = GameStateUtils.TryGetCurrentGameRun(out GameRunController currentRun, out string runSource);
        List<Card> deck = currentRun?.BaseDeck?
            .Where(c => c != null)
            .OrderBy(c => c.Name)
            .ToList();

        string deckSource = "CurrentGameRun";

        if (deck == null || deck.Count == 0)
        {
            deck = _initialDeckCards
                .Where(c => c != null)
                .OrderBy(c => c.Name)
                .ToList();
            deckSource = "InitialDeckCards";
        }

        deck ??= new List<Card>();

        HashSet<int> selected = new HashSet<int>(_localCards.Where(c => c != null).Select(c => c.InstanceId));
        int candidateCount = deck.Count(card => card != null && !selected.Contains(card.InstanceId));

        int desiredCount = Mathf.Max(1, Mathf.Min(3, _maxSlots));
        int maxSelectableNow = Mathf.Max(1, Mathf.Min(desiredCount, _localCards.Count + candidateCount));
        tag.TargetSelectCount = maxSelectableNow;

        while (_localCards.Count > maxSelectableNow)
        {
            _localCards.RemoveAt(_localCards.Count - 1);
        }

        if (tag.HintText != null)
        {
            tag.HintText.text = $"请选择 {maxSelectableNow} 张卡牌（当前 {_localCards.Count}/{maxSelectableNow}）";
        }

        if (tag.ConfirmButton?.button != null)
        {
            tag.ConfirmButton.button.interactable = _localCards.Count == maxSelectableNow;
        }

        Plugin.Logger?.LogInfo(
            $"[TradeDetailDialog] RebuildCardPicker: tradeId={_tradeId ?? "<null>"}, hasRun={hasRun}, runSource={runSource ?? "<null>"}, currentDeckCount={(currentRun?.BaseDeck?.Count ?? 0)}, initialDeckCount={_initialDeckCards.Count}, selectedCount={selected.Count}, chosenDeckSource={deckSource}, chosenDeckCount={deck.Count}, candidateCount={candidateCount}, target={maxSelectableNow}, cardCellTemplate={(_cardCellTemplate != null)}, pickerActive={(_cardPickerRoot != null && _cardPickerRoot.activeSelf)}");

        // 先渲染"我方已选卡牌"（可点击移除）。
        if (_localCards.Count == 0)
        {
            CommonButtonWidget empty = CloneListItem(localContainer, "LocalEmpty", "(无)", false);
            empty.button.interactable = false;
        }
        else
        {
            for (int i = 0; i < _localCards.Count; i++)
            {
                Card selectedCard = _localCards[i];
                if (selectedCard == null)
                {
                    continue;
                }

                string label = $"{selectedCard.Name}{(selectedCard.IsUpgraded ? "+" : string.Empty)}  (点击移除)";
                CommonButtonWidget row = CloneListItem(localContainer, $"LocalSelected_{selectedCard.InstanceId}_{i}", label, false);
                row.button.onClick.RemoveAllListeners();
                row.button.onClick.AddListener(new UnityAction(() =>
                {
                    _localCards.Remove(selectedCard);
                    RebuildCardPicker();
                }));
            }
        }

        // 渲染"对方已选卡牌"（只读）。
        TradeSyncPatch.TradeSessionState state = TradeSyncPatch.GetLastKnown(_tradeId);
        bool localIsA = IsPlayerA(state);
        List<TradeSyncPatch.CardRef> remoteCards = state != null ? GetRemoteOffer(state, localIsA) : null;

        if (remoteCards == null || remoteCards.Count == 0)
        {
            CommonButtonWidget empty = CloneListItem(remoteContainer, "RemoteEmpty", "(无)", false);
            empty.button.interactable = false;
        }
        else
        {
            for (int i = 0; i < remoteCards.Count && i < 3; i++)
            {
                TradeSyncPatch.CardRef remoteCard = remoteCards[i];
                if (remoteCard == null)
                {
                    continue;
                }

                string label = $"{remoteCard.CardName}{(remoteCard.IsUpgraded ? "+" : string.Empty)}";
                CommonButtonWidget row = CloneListItem(remoteContainer, $"RemoteSelected_{remoteCard.InstanceId}_{i}", label, false);
                row.button.interactable = false;
            }
        }

        foreach (var card in deck)
        {
            if (card == null || selected.Contains(card.InstanceId))
            {
                continue;
            }

            if (_cardCellTemplate != null)
            {
                var cell = Instantiate(_cardCellTemplate, candidateContainer, false);
                cell.gameObject.SetActive(true);
                cell.Card = card;
                cell.SetNum(1);

                var btn = cell.gameObject.GetComponent<Button>();
                if (btn == null) btn = cell.gameObject.AddComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(new UnityAction(() =>
                {
                    if (_localCards.Count >= maxSelectableNow)
                    {
                        return;
                    }

                    AudioManager.Card(3); // 卡牌点击音效。
                    _localCards.Add(card);
                    RebuildCardPicker(); // 立即刷新，把刚选中的卡牌移出列表。
                }));
            }
            else
            {
                var row = CloneListItem(candidateContainer, $"Card_{card.InstanceId}", card.Name, false);
                row.button.onClick.AddListener(new UnityAction(() =>
                {
                    if (_localCards.Count >= maxSelectableNow)
                    {
                        return;
                    }

                    _localCards.Add(card);
                    RebuildCardPicker();
                }));
            }
        }

        if (deck.Count == 0)
        {
            Plugin.Logger?.LogWarning($"[TradeDetailDialog] RebuildCardPicker empty deck: tradeId={_tradeId ?? "<null>"}, hasRun={hasRun}, runSource={runSource ?? "<null>"}, initialDeckCount={_initialDeckCards.Count}");
            var empty = CloneText(candidateContainer as RectTransform, "Empty", 20, TextAlignmentOptions.Center);
            empty.text = "没有可交易的卡牌";
        }
    }

    private void RebuildExhibitPicker()
    {
        if (_exhibitPickerRoot == null)
        {
            return;
        }

        var tag = _exhibitPickerRoot.GetComponentInChildren<PickerListTag>(true);
        if (tag == null)
        {
            return;
        }

        var container = tag.transform;

        // 遗物列表也使用 Grid Layout，和原生展示方式更接近。
        var vlg = container.GetComponent<VerticalLayoutGroup>();
        if (vlg != null) DestroyImmediate(vlg);

        var glg = container.GetComponent<GridLayoutGroup>();
        if (glg == null) glg = container.gameObject.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(100, 100); // 遗物格子保持方形。
        glg.spacing = new Vector2(20, 20);
        glg.padding = new RectOffset(20, 20, 20, 20);
        glg.childAlignment = TextAnchor.UpperLeft;

        foreach (Transform c in container)
        {
            Destroy(c.gameObject);
        }

        List<Exhibit> tradable = CurrentGameRun?.Player?.Exhibits?
            .Where(TradeExhibitRules.IsTradable)
            .OrderBy(e => e.Name)
            .ToList()
            ?? new List<Exhibit>();

        if (tradable.Count == 0)
        {
            var empty = CloneText(container as RectTransform, "Empty", 20, TextAlignmentOptions.Center);
            empty.text = "没有可交易的遗物";
            return;
        }

        foreach (var ex in tradable)
        {
            if (ex == null || string.IsNullOrWhiteSpace(ex.Id))
            {
                continue;
            }

            if (_exhibitTemplate != null)
            {
                var widget = Instantiate(_exhibitTemplate, container, false);
                widget.gameObject.SetActive(true);
                widget.Exhibit = ex;
                widget.ShowCounter = false;

                var isOn = _localExhibitIds.Contains(ex.Id);
                var img = widget.MainImage;
                if (img != null) img.color = isOn ? new Color(0.4f, 1f, 0.4f, 1f) : Color.white;

                var btn = widget.gameObject.GetComponent<Button>();
                if (btn == null) btn = widget.gameObject.AddComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(new UnityAction(() =>
                {
                    AudioManager.Button(2);
                    if (_localExhibitIds.Contains(ex.Id))
                    {
                        _localExhibitIds.Remove(ex.Id);
                    }
                    else
                    {
                        _localExhibitIds.Add(ex.Id);
                    }
                    RebuildExhibitPicker();
                }));
            }
            else
            {
                var isOn = _localExhibitIds.Contains(ex.Id);
                var row = CloneListItem(container, $"Ex_{ex.Id}", ex.Name, isOn);
                row.button.onClick.AddListener(new UnityAction(() =>
                {
                    if (_localExhibitIds.Contains(ex.Id))
                    {
                        _localExhibitIds.Remove(ex.Id);
                    }
                    else
                    {
                        _localExhibitIds.Add(ex.Id);
                    }

                    RefreshLocalUi();
                    TrySendOfferUpdate();
                    RebuildExhibitPicker();
                }));
            }
        }
    }

    private void TrySendOfferUpdate()
    {
        if (_isApplyingState)
        {
            return;
        }

        try
        {
            List<TradeSyncPatch.CardRef> refs = _localCards
                .Where(c => c != null)
                .Select(c => new TradeSyncPatch.CardRef
                {
                    CardId = c.Id,
                    InstanceId = c.InstanceId,
                    IsUpgraded = c.IsUpgraded,
                    UpgradeCounter = c.UpgradeCounter ?? 0,
                    DeckCounter = c.DeckCounter,
                    CardName = c.Name,
                    CardType = c.CardType.ToString(),
                })
                .ToList();

            TradeSyncPatch.RequestOfferUpdate(_tradeId, _selfId, refs, _localMoney, _localExhibitIds.ToList());

            UpdateConfirmInteractable(TradeSyncPatch.GetLastKnown(_tradeId));
        }
        catch
        {
            // ignored
        }
    }

    #endregion
}
