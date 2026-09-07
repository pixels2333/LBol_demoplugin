using System;
using System.Collections;
using System.Linq;
using HarmonyLib;
using LBoL.Core.Cards;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Widgets;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.Patch.UI;
using NetworkPlugin.UI.Factories;
using NetworkPlugin.UI.Panels;
using NetworkPlugin.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Dialogs;

public sealed class TradeRequestConfirmDialog : UiDialog<TradeSyncPatch.TradeSessionState>, IInputActionHandler
{
    private CanvasGroup _canvasGroup;
    private RectTransform _panelRoot;

    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _messageText;
    private TextMeshProUGUI _timerText;

    private TradeSyncPatch.TradeSessionState _sessionState;
    private string _tradeId;
    private string _selfId;
    private string _initiatorId;
    private string _initiatorName;

    private bool _actionHandlerPushed;
    private bool _handled;
    private Coroutine _countdownCoroutine;
    private const int TimeoutSeconds = 30;

    public void BindRuntime(TextMeshProUGUI textTemplate, RectTransform panelRoot)
    {
        _panelRoot = panelRoot;
        EnsureUiComponents(textTemplate);
    }

    public void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (_panelRoot == null)
        {
            RectTransform panel = transform.Find("TradeConfirmFrame") as RectTransform;
            if (panel == null)
            {
                panel = transform.Find("Panel") as RectTransform;
            }
            _panelRoot = panel != null ? panel : GetComponent<RectTransform>();
        }
    }

    protected override void OnShowing(TradeSyncPatch.TradeSessionState state)
    {
        _sessionState = state;
        _handled = false;

        if (state == null || string.IsNullOrWhiteSpace(state.TradeId))
        {
            Hide(false);
            return;
        }

        gameObject.SetActive(true);

        _tradeId = state.TradeId;
        _selfId = NetworkIdentityTracker.GetSelfPlayerId();
        _initiatorId = state.PlayerAId;
        _initiatorName = OtherPlayersOverlayPatch.ResolveDisplayName(_initiatorId, state.PlayerAName, isLocal: false);
        if (string.IsNullOrWhiteSpace(_initiatorName))
        {
            _initiatorName = "未知玩家";
        }

        _canvasGroup.interactable = true;

        if (UiManager.IsInitialized)
        {
            UiManager.PushActionHandler(this);
            _actionHandlerPushed = true;
        }

        if (_titleText != null)
        {
            _titleText.text = "交易请求";
        }

        if (_messageText != null)
        {
            _messageText.text = $"玩家 【{_initiatorName}】 请求与您发起交易，是否接受？";
        }

        if (_timerText != null)
        {
            _timerText.text = $"({TimeoutSeconds}秒后自动拒绝)";
        }

        EnsureSubscribed();

        if (_countdownCoroutine != null)
        {
            StopCoroutine(_countdownCoroutine);
            _countdownCoroutine = null;
        }
        _countdownCoroutine = StartCoroutine(CoCountdown(TimeoutSeconds));
    }

    protected override void OnHiding()
    {
        _canvasGroup.interactable = false;

        if (_actionHandlerPushed)
        {
            if (UiManager.IsInitialized)
            {
                UiManager.PopActionHandler(this);
            }
            _actionHandlerPushed = false;
        }

        if (_countdownCoroutine != null)
        {
            StopCoroutine(_countdownCoroutine);
            _countdownCoroutine = null;
        }

        TradeSyncPatch.OnTradeStateUpdated -= OnTradeStateUpdated;

        if (!_handled && !string.IsNullOrWhiteSpace(_tradeId) && !string.IsNullOrWhiteSpace(_selfId))
        {
            _handled = true;
            TradeSyncPatch.RequestCancel(_tradeId, _selfId);
        }
    }

    private void EnsureSubscribed()
    {
        TradeSyncPatch.OnTradeStateUpdated -= OnTradeStateUpdated;
        TradeSyncPatch.OnTradeStateUpdated += OnTradeStateUpdated;
    }

    private void OnTradeStateUpdated(TradeSyncPatch.TradeSessionState state)
    {
        if (state == null || !string.Equals(state.TradeId, _tradeId, StringComparison.Ordinal))
        {
            return;
        }

        if (state.Status == TradeSyncPatch.TradeStatus.Canceled ||
            state.Status == TradeSyncPatch.TradeStatus.Failed ||
            state.Status == TradeSyncPatch.TradeStatus.Completed)
        {
            _handled = true;
            Hide();
        }
    }

    private IEnumerator CoCountdown(int totalSeconds)
    {
        int remaining = totalSeconds;
        while (remaining > 0)
        {
            if (_timerText != null)
            {
                _timerText.text = $"({remaining}秒后自动拒绝)";
            }
            yield return new WaitForSecondsRealtime(1f);
            remaining--;
        }

        Plugin.Logger?.LogInfo($"[TradeRequestConfirmDialog] Trade request timed out after {totalSeconds}s, auto-declining: tradeId={_tradeId}");
        OnDecline();
    }

    public void OnAccept()
    {
        if (_handled) return;
        _handled = true;

        AudioManager.Button(0);
        Plugin.Logger?.LogInfo($"[TradeRequestConfirmDialog] Trade request accepted by self ({_selfId}): tradeId={_tradeId}, partner={_initiatorId}");

        Hide();

        Plugin.RunOnMainThread(() =>
        {
            var tradePanel = TradePanelRuntimeFactory.GetOrCreate(UiManager.Instance?.transform);
            if (tradePanel != null)
            {
                tradePanel.Show(new UI.Payloads.TradePayload
                {
                    TradeId = _tradeId,
                    Player1Id = _selfId,
                    Player2Id = _initiatorId,
                    Player1Name = OtherPlayersOverlayPatch.ResolveDisplayName(_selfId, NetworkIdentityTracker.GetSelfPlayerId(), isLocal: true),
                    Player2Name = _initiatorName,
                    MaxTradeSlots = _sessionState?.MaxTradeSlots > 0 ? _sessionState.MaxTradeSlots : 3,
                    CanCancel = true
                });
            }
            else
            {
                Plugin.Logger?.LogWarning("[TradeRequestConfirmDialog] Failed to open TradePanel after accept.");
            }
        });
    }

    public void OnDecline()
    {
        if (_handled) return;
        _handled = true;

        AudioManager.Button(0);
        Plugin.Logger?.LogInfo($"[TradeRequestConfirmDialog] Trade request declined by self ({_selfId}): tradeId={_tradeId}");

        if (!string.IsNullOrWhiteSpace(_tradeId) && !string.IsNullOrWhiteSpace(_selfId))
        {
            TradeSyncPatch.RequestCancel(_tradeId, _selfId);
        }

        Hide();
    }

    public void OnConfirm()
    {
        OnAccept();
    }

    public void OnCancel()
    {
        OnDecline();
    }

    private void EnsureUiComponents(TextMeshProUGUI textTemplate)
    {
        if (_panelRoot == null) return;

        textTemplate ??= _panelRoot.GetComponentInChildren<TextMeshProUGUI>(true);

        _titleText = _panelRoot.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
        if (_titleText == null && textTemplate != null)
        {
            _titleText = Instantiate(textTemplate, _panelRoot);
            _titleText.name = "TitleText";
            foreach (Transform child in _titleText.transform)
            {
                Destroy(child.gameObject);
            }
        }
        if (_titleText != null)
        {
            _titleText.gameObject.SetActive(true);
            _titleText.alignment = TextAlignmentOptions.Center;
            _titleText.fontSize = 42;
            _titleText.color = new Color(1f, 0.85f, 0.3f, 1f);
            _titleText.text = "交易请求";
            SetCenteredRect(_titleText.rectTransform, new Vector2(0f, 280f), new Vector2(1000f, 80f));
        }

        _messageText = _panelRoot.Find("MessageText")?.GetComponent<TextMeshProUGUI>();
        if (_messageText == null && textTemplate != null)
        {
            _messageText = Instantiate(textTemplate, _panelRoot);
            _messageText.name = "MessageText";
            foreach (Transform child in _messageText.transform)
            {
                Destroy(child.gameObject);
            }
        }
        if (_messageText != null)
        {
            _messageText.gameObject.SetActive(true);
            _messageText.alignment = TextAlignmentOptions.Center;
            _messageText.fontSize = 32;
            _messageText.color = Color.white;
            SetCenteredRect(_messageText.rectTransform, new Vector2(0f, 60f), new Vector2(1200f, 120f));
        }

        _timerText = _panelRoot.Find("TimerText")?.GetComponent<TextMeshProUGUI>();
        if (_timerText == null && textTemplate != null)
        {
            _timerText = Instantiate(textTemplate, _panelRoot);
            _timerText.name = "TimerText";
            foreach (Transform child in _timerText.transform)
            {
                Destroy(child.gameObject);
            }
        }
        if (_timerText != null)
        {
            _timerText.gameObject.SetActive(true);
            _timerText.alignment = TextAlignmentOptions.Center;
            _timerText.fontSize = 22;
            _timerText.color = new Color(0.8f, 0.8f, 0.8f, 0.9f);
            _timerText.text = "";
            SetCenteredRect(_timerText.rectTransform, new Vector2(0f, -40f), new Vector2(600f, 50f));
        }

        Transform cancelTr = _panelRoot.Find("Cancel");
        if (cancelTr != null)
        {
            cancelTr.gameObject.SetActive(true);
            var cancelRt = cancelTr.GetComponent<RectTransform>();
            SetCenteredRect(cancelRt, new Vector2(-350f, -240f), new Vector2(596f, 152f));
            cancelTr.localScale = Vector3.one;

            var cancelTmp = cancelTr.Find("Layout/Text (TMP)")?.GetComponent<TextMeshProUGUI>();
            if (cancelTmp != null)
            {
                cancelTmp.text = "拒绝";
            }

            var cancelBtn = cancelTr.GetComponent<Button>();
            if (cancelBtn != null)
            {
                cancelBtn.onClick = new Button.ButtonClickedEvent();
                cancelBtn.onClick.AddListener(new UnityAction(OnDecline));
            }
        }

        Transform confirmTr = _panelRoot.Find("Confirm");
        if (confirmTr != null)
        {
            confirmTr.gameObject.SetActive(true);
            var confirmRt = confirmTr.GetComponent<RectTransform>();
            SetCenteredRect(confirmRt, new Vector2(350f, -240f), new Vector2(596f, 152f));
            confirmTr.localScale = Vector3.one;

            var confirmTmp = confirmTr.Find("Layout/Text (TMP)")?.GetComponent<TextMeshProUGUI>();
            if (confirmTmp != null)
            {
                confirmTmp.text = "接受";
            }

            var confirmBtn = confirmTr.GetComponent<Button>();
            if (confirmBtn != null)
            {
                confirmBtn.onClick = new Button.ButtonClickedEvent();
                confirmBtn.onClick.AddListener(new UnityAction(OnAccept));
            }
        }
    }

    private static void SetCenteredRect(RectTransform rt, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        if (rt == null) return;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta = sizeDelta;
    }
}
