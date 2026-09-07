using System;
using System.Collections;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Configuration;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.UI.Components;
using NetworkPlugin.UI.Models;
using NetworkPlugin.UI.Payloads;
using NetworkPlugin.UI.State;
using NetworkPlugin.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Panels;

public class ResurrectPanel : UiPanel<ResurrectPayload>, IInputActionHandler
{
	#region 常量定义
		private const float ResurrectCompleteWaitTime = 2f;
	#endregion

	#region UI组件引用
		[SerializeField]
	private Transform deadPlayersContainer;

		[SerializeField]
	private CommonButtonWidget resurrectButton;

		[SerializeField]
	private CommonButtonWidget cancelButton;

		[SerializeField]
	private TextMeshProUGUI statusText;
	#endregion

	#region 复活数据
		private readonly List<DeadPlayerEntry> _deadPlayers = [];

		private readonly List<Button> _playerButtons = new List<Button>();

		private TextMeshProUGUI _textTemplate;

		private DeadPlayerEntry _selectedPlayer;

	private string _selfPlayerId;
	private string _pendingRequestId;

		private ResurrectPayload _payload;

		private CanvasGroup _canvasGroup;

		private bool _canCancel = true;

		private bool _isActionHandlerPushed;

		private bool _isListeningSyncResult;

		public bool DidCompleteTreatment { get; private set; }
	#endregion

	#region 属性
		public override PanelLayer Layer => PanelLayer.Top;
	#endregion

	#region Unity生命周期
	internal void BindRuntimeUi(
		Transform runtimeDeadPlayersContainer,
		TextMeshProUGUI runtimeTextTemplate,
		CommonButtonWidget runtimeResurrectButton,
		CommonButtonWidget runtimeCancelButton,
		CanvasGroup runtimeCanvasGroup,
		TextMeshProUGUI runtimeStatusText = null)
	{
		deadPlayersContainer = runtimeDeadPlayersContainer;
		_textTemplate = runtimeTextTemplate;
		resurrectButton = runtimeResurrectButton;
		cancelButton = runtimeCancelButton;
		if (runtimeStatusText != null)
		{
			statusText = runtimeStatusText;
		}

		_canvasGroup = runtimeCanvasGroup;
		if (_canvasGroup == null)
		{
			_canvasGroup = GetComponent<CanvasGroup>();
		}
		if (_canvasGroup == null)
		{
			_canvasGroup = gameObject.AddComponent<CanvasGroup>();
		}

		if (resurrectButton?.button != null)
		{
			resurrectButton.button.onClick.RemoveAllListeners();
			resurrectButton.button.onClick.AddListener(OnResurrectPlayer);
		}

		if (cancelButton?.button != null)
		{
			cancelButton.button.onClick.RemoveAllListeners();
			cancelButton.button.onClick.AddListener(OnCancelResurrect);
		}
	}

		public void Awake()
	{

		_canvasGroup = GetComponent<CanvasGroup>();
		if (_canvasGroup == null)
		{

			_canvasGroup = gameObject.AddComponent<CanvasGroup>();
		}

		if (resurrectButton?.button != null)
		{
			resurrectButton.button.onClick.AddListener(OnResurrectPlayer);
		}

		if (cancelButton?.button != null)
		{
			cancelButton.button.onClick.AddListener(OnCancelResurrect);
		}
	}
	#endregion

	#region 本地化处理
		public override void OnLocaleChanged()
	{
		if (_payload != null)
		{

			UpdateUIStrings();
		}
	}
	#endregion

	#region 面板显示/隐藏
		protected override void OnShowing(ResurrectPayload payload)
	{
		EnsurePopupTopmost();

		_payload = payload;

		_canCancel = payload?.CanCancel ?? true;

		ResetResurrectData();

		var list = payload?.Players;
		if (list != null && list.Count > 0)
		{
			_deadPlayers.AddRange(list);
		}

		INetworkClient client = ModService.ServiceProvider?.GetService<INetworkClient>();
		if (client != null)
		{
			NetworkIdentityTracker.EnsureSubscribed(client);
		}
		_selfPlayerId = NetworkIdentityTracker.GetSelfPlayerId();

		_pendingRequestId = null;
		DidCompleteTreatment = false;

		if (_deadPlayers.Count == 0)
		{
			UpdateUIStatus("当前没有可治疗的玩家");
			if (_canvasGroup != null)
			{
				_canvasGroup.alpha = 0f;
				_canvasGroup.interactable = false;
			}
			Hide();
			return;
		}

		cancelButton?.gameObject.SetActive(_canCancel);
		resurrectButton?.gameObject.SetActive(false);

		if (_canvasGroup != null)
		{
			_canvasGroup.alpha = 1f;
			_canvasGroup.interactable = true;
		}

		if (deadPlayersContainer != null)
		{
			foreach (Transform child in deadPlayersContainer)
			{
				Destroy(child.gameObject);
			}
		}
		_playerButtons.Clear();

		for (int i = 0; i < _deadPlayers.Count; i++)
		{
			var player = _deadPlayers[i];
			var btn = CreatePlayerRow(player, i);
			if (btn != null)
			{
				_playerButtons.Add(btn);
				Plugin.Logger?.LogDebug($"[ResurrectPanel] 创建可点击文字行: index={i}, playerId={player?.PlayerId}, name={player?.PlayerName}");
			}
		}

		UpdateUIStrings();

		if (!_isListeningSyncResult)
		{
			ResurrectSyncPatch.OnResurrectResult += OnResurrectResult;
			_isListeningSyncResult = true;
		}

		if (!_isActionHandlerPushed)
		{
			UiManager.PushActionHandler(this);
			_isActionHandlerPushed = true;
		}

		try
		{
			if (transform.parent != null)
			{
				Plugin.Logger?.LogInfo("[ResurrectPanelDump] Start dumping from parent:");
				DumpHierarchy(transform.parent);
			}
			else
			{
				Plugin.Logger?.LogInfo("[ResurrectPanelDump] Start dumping from self:");
				DumpHierarchy(transform);
			}
		}
		catch (Exception ex)
		{
			Plugin.Logger?.LogError($"[ResurrectPanelDump] Error during hierarchy dump: {ex.Message}");
		}
	}

		protected override void OnShown()
	{

		EnsurePopupTopmost();
	}

		protected override void OnHiding()
	{

		if (_canvasGroup != null)
		{
			_canvasGroup.interactable = false;
		}

		if (_isListeningSyncResult)
		{
			ResurrectSyncPatch.OnResurrectResult -= OnResurrectResult;
			_isListeningSyncResult = false;
		}

		if (_isActionHandlerPushed)
		{
			try
			{
				UiManager.PopActionHandler(this);
			}
			catch (Exception ex)
			{
				Plugin.Logger?.LogWarning($"[ResurrectPanel] PopActionHandler 异常: {ex.Message}");
			}
			finally
			{
				_isActionHandlerPushed = false;
			}
		}
	}

		protected override void OnHided()
	{
		if (_isListeningSyncResult)
		{
			ResurrectSyncPatch.OnResurrectResult -= OnResurrectResult;
			_isListeningSyncResult = false;
		}

		if (_isActionHandlerPushed)
		{
			try
			{
				UiManager.PopActionHandler(this);
			}
			catch (Exception ex)
			{
				Plugin.Logger?.LogWarning($"[ResurrectPanel] OnHided PopActionHandler 异常: {ex.Message}");
			}
			finally
			{
				_isActionHandlerPushed = false;
			}
		}

		ResetResurrectData();

		_payload = null;
	}
	#endregion

	#region UI更新方法
		private void UpdateUIStrings()
	{
		if (!string.IsNullOrWhiteSpace(_pendingRequestId))
		{
			return;
		}

		if (_selectedPlayer == null)
		{
			UpdateUIStatus("点击玩家立即治疗");
		}
	}

		private void ResetResurrectData()
	{

		_deadPlayers.Clear();

		_selectedPlayer = null;

		if (deadPlayersContainer != null)
		{
			foreach (Transform child in deadPlayersContainer)
			{
				Destroy(child.gameObject);
			}
		}

		_playerButtons.Clear();

		if (resurrectButton?.button != null)
		{
			resurrectButton.button.interactable = false;
		}
	}

	private void UpdateUIStatus(string message)
	{
		if (statusText != null)
		{
			statusText.text = message ?? string.Empty;
			statusText.gameObject.SetActive(!string.IsNullOrEmpty(message));
		}
	}

	private Button CreatePlayerRow(DeadPlayerEntry player, int index)
	{
		if (_textTemplate == null || deadPlayersContainer == null)
		{
			Plugin.Logger?.LogWarning("[ResurrectPanel] 文字模板或容器为空，无法创建行");
			return null;
		}

		string displayName = string.IsNullOrWhiteSpace(player?.PlayerName) ? player?.PlayerId : player.PlayerName;
		int actionValue = player.ActionValue > 0 ? player.ActionValue : player.ResurrectionCost;
		string actionText = player.CanResurrect && actionValue > 0 ? $"治疗+{actionValue}" : "生命已满";
		string label = $"{displayName}  HP {player.CurrentHp}/{player.MaxHp}  |  {actionText}";

		var tmp = UnityEngine.Object.Instantiate(_textTemplate, deadPlayersContainer, false);
		tmp.name = $"PlayerRow_{index}";

		RectTransform tmpRect = tmp.rectTransform;
		tmpRect.anchorMin = new Vector2(0f, 1f);
		tmpRect.anchorMax = new Vector2(1f, 1f);
		tmpRect.pivot = new Vector2(0.5f, 1f);
		tmpRect.sizeDelta = new Vector2(0f, 64f);
		tmpRect.anchoredPosition = new Vector2(0f, tmpRect.anchoredPosition.y);

		var fitter = tmp.GetComponent<ContentSizeFitter>();
		if (fitter != null)
		{
			UnityEngine.Object.DestroyImmediate(fitter);
		}

		tmp.text = label;
		tmp.alignment = TextAlignmentOptions.Center;
		tmp.raycastTarget = true;
		tmp.enableAutoSizing = false;
		tmp.enableWordWrapping = false;
		tmp.overflowMode = TextOverflowModes.Overflow;

		float fontSize = _textTemplate.fontSize > 0 ? _textTemplate.fontSize * 0.5f : 18f;
		tmp.fontSize = Mathf.Max(fontSize, 14f);
		tmp.fontSizeMin = 1f;
		tmp.fontSizeMax = tmp.fontSize;

		var localized = tmp.GetComponent<LBoL.Presentation.I10N.LocalizedText>();
		if (localized != null)
		{
			var type = typeof(LBoL.Presentation.I10N.LocalizedText);
			type.GetField("key", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?
				.SetValue(localized, null);
			type.GetField("_originSize", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?
				.SetValue(localized, tmp.fontSize);
		}

		Color baseColor = player.CanResurrect
			? new Color(0.92f, 0.92f, 0.92f, 1f)
			: new Color(0.5f, 0.5f, 0.5f, 0.6f);
		tmp.color = baseColor;

		var le = tmp.GetComponent<LayoutElement>();
		if (le == null)
		{
			le = tmp.gameObject.AddComponent<LayoutElement>();
		}
		le.ignoreLayout = false;
		le.preferredHeight = 64f;
		le.preferredWidth = -1f;
		le.minWidth = -1f;
		le.flexibleWidth = 1f;

		var btn = tmp.gameObject.AddComponent<Button>();
		btn.targetGraphic = tmp;
		btn.interactable = true;
		btn.transition = Selectable.Transition.ColorTint;

		var colors = btn.colors;
		colors.normalColor = Color.white;
		colors.highlightedColor = new Color(1f, 1f, 0.7f, 1f);
		colors.selectedColor = new Color(1f, 1f, 0.7f, 1f);
		colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
		colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		colors.fadeDuration = 0.08f;
		btn.colors = colors;

		var nav = btn.navigation;
		nav.mode = Navigation.Mode.None;
		btn.navigation = nav;

		tmp.gameObject.AddComponent<RowHoverScale>();

		var capturedPlayer = player;
		btn.onClick.RemoveAllListeners();
		btn.onClick.AddListener(() =>
		{
			if (!capturedPlayer.CanResurrect)
			{
				string tip = $"{displayName} 当前生命值已满，无需治疗";
				UpdateUIStatus(tip);
				TradeUiMessages.ShowTopMessage(tip);
				return;
			}

			if (!string.IsNullOrWhiteSpace(_pendingRequestId))
			{
				UpdateUIStatus("正在治疗中，请稍候...");
				return;
			}

			_selectedPlayer = capturedPlayer;

			if ((ModService.ServiceProvider?.GetService<ConfigManager>())?.DebugVirtualPlayerAiDefault?.Value == true
				&& !string.IsNullOrWhiteSpace(capturedPlayer.PlayerId)
				&& capturedPlayer.PlayerId.StartsWith("aidefault", StringComparison.OrdinalIgnoreCase))
			{
				_pendingRequestId = Guid.NewGuid().ToString("N");
				OnResurrectResult(_pendingRequestId, true, null);
				return;
			}

			OnResurrectPlayer();
		});

		tmp.gameObject.SetActive(true);
		return btn;
	}
	#endregion

	private void SetPlayerButtonsInteractable(bool interactable)
	{
		int count = Math.Min(_playerButtons.Count, _deadPlayers.Count);
		for (int i = 0; i < count; i++)
		{
			Button btn = _playerButtons[i];
			if (btn == null)
			{
				continue;
			}

			btn.interactable = interactable;
		}
	}

	#region 按钮事件处理
		private void OnResurrectPlayer()
	{

		if (_selectedPlayer == null || !_selectedPlayer.CanResurrect)
			return;

		if (!string.IsNullOrWhiteSpace(_pendingRequestId))
			return;

		_pendingRequestId = Guid.NewGuid().ToString("N");
		try
		{
			INetworkClient client = ModService.ServiceProvider.GetService<INetworkClient>();
			if (client != null)
			{
				ResurrectSyncPatch.EnsureSubscribed(client);
				NetworkIdentityTracker.EnsureSubscribed(client);
				string requesterId = _selfPlayerId ?? NetworkIdentityTracker.GetSelfPlayerId();
				string targetId = _selectedPlayer.PlayerId;
				if (!string.IsNullOrWhiteSpace(requesterId) && !string.IsNullOrWhiteSpace(targetId))
				{
					client.SendGameEventData(NetworkMessageTypes.OnGapHealRequest, new
					{
						RequestId = _pendingRequestId,
						RequesterPlayerId = requesterId,
						TargetPlayerId = targetId,
						Timestamp = DateTime.UtcNow.Ticks,
					});
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError($"[ResurrectPanel] 发送治疗事件失败: {ex.Message}");
		}
		SetPlayerButtonsInteractable(false);

		if (resurrectButton?.button != null)
		{
			resurrectButton.button.interactable = false;
		}
		if (cancelButton?.button != null)
		{
			cancelButton.button.interactable = false;
		}

		UpdateUIStatus("正在治疗：" + _selectedPlayer.PlayerName);
	}

		private void OnCancelResurrect()
	{

		_selectedPlayer = null;

		Hide();
	}

		public void OnCancel()
	{
		if (_canCancel)
		{
			OnCancelResurrect();
		}
	}
	#endregion

	private void OnResurrectResult(string requestId, bool success, string reason)
	{
		if (string.IsNullOrWhiteSpace(_pendingRequestId) || !string.Equals(_pendingRequestId, requestId, StringComparison.Ordinal))
		{
			return;
		}

		_pendingRequestId = null;

		if (success)
		{
			DidCompleteTreatment = true;
			UpdateUIStatus("治疗完成：" + _selectedPlayer?.PlayerName);
			StartCoroutine(HideAfterDelay(ResurrectCompleteWaitTime));
		}
		else
		{
			UpdateUIStatus("治疗失败" + (string.IsNullOrWhiteSpace(reason) ? string.Empty : $"：{reason}"));
			SetPlayerButtonsInteractable(true);
			if (resurrectButton?.button != null)
			{
				resurrectButton.button.interactable = _selectedPlayer != null && _selectedPlayer.CanResurrect;
			}
			if (cancelButton?.button != null)
			{
				cancelButton.button.interactable = true;
			}
		}
	}

	private static int CalculateHealingAmount(int maxHp)
	{
		if (maxHp <= 0)
		{
			return 1;
		}

		return Math.Max(1, Mathf.CeilToInt(maxHp * 0.2f));
	}

	private void EnsurePopupTopmost()
	{
		try
		{
			transform.SetAsLastSibling();
		}
		catch
		{

		}
	}

	private void DumpHierarchy(Transform t, int indent = 0)
	{
		if (t == null) return;
		string indentStr = new string(' ', indent * 2);
		var rt = t as RectTransform;
		string rectInfo = "";
		if (rt != null)
		{
			rectInfo = $"[Rect: sizeDelta={rt.sizeDelta}, anchoredPos={rt.anchoredPosition}, anchors=({rt.anchorMin}, {rt.anchorMax}), width={rt.rect.width}, height={rt.rect.height}]";
		}

		List<string> compNames = new List<string>();
		foreach (var c in t.GetComponents<Component>())
		{
			if (c != null) compNames.Add(c.GetType().Name);
		}
		string comps = string.Join(", ", compNames);
		Plugin.Logger?.LogInfo($"[ResurrectPanelDump] {indentStr}- {t.name} (activeSelf={t.gameObject.activeSelf}, activeInHierarchy={t.gameObject.activeInHierarchy}) {rectInfo} | Components: {comps}");

		for (int i = 0; i < t.childCount; i++)
		{
			DumpHierarchy(t.GetChild(i), indent + 1);
		}
	}

	#region 辅助方法
		private IEnumerator HideAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		Hide();
	}

		public IEnumerator ShowResurrectAsync(ResurrectPayload payload)
	{
		Show(payload);
		yield return new WaitWhile(() => IsVisible);
	}
	#endregion
}

internal sealed class RowHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	private RectTransform _rt;
	private bool _hovering;
	private bool _pressed;

	private void Awake()
	{
		_rt = transform as RectTransform;
		if (_rt != null) _rt.localScale = Vector3.one;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_hovering = true;
		Apply();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_hovering = false;
		_pressed = false;
		Apply();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		_pressed = true;
		Apply();
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		_pressed = false;
		Apply();
	}

	private void Apply()
	{
		if (_rt == null) return;
		float s = _pressed ? 1.02f : _hovering ? 1.08f : 1f;
		_rt.localScale = new Vector3(s, s, 1f);
	}
}
