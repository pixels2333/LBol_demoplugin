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
using NetworkPlugin.UI.Models;
using NetworkPlugin.UI.Payloads;
using NetworkPlugin.UI.State;
using NetworkPlugin.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Panels;

/// <summary>
/// Gap 治疗面板类。
/// </summary>
public class ResurrectPanel : UiPanel<ResurrectPayload>, IInputActionHandler
{
	#region 常量定义
	/// <summary>
	/// 治疗完成后等待时间（秒）
	/// </summary>
	private const float ResurrectCompleteWaitTime = 2f;
	#endregion

	#region UI组件引用
	/// <summary>
	/// 目标玩家列表容器
	/// </summary>
	[SerializeField]
	private Transform deadPlayersContainer;

	/// <summary>
	/// 治疗按钮
	/// </summary>
	[SerializeField]
	private CommonButtonWidget resurrectButton;

	/// <summary>
	/// 取消按钮
	/// </summary>
	[SerializeField]
	private CommonButtonWidget cancelButton;

	/// <summary>
	/// 状态文本显示
	/// </summary>
	[SerializeField]
	private TextMeshProUGUI statusText;
	#endregion

	#region 复活数据
	/// <summary>
	/// 可治疗玩家列表
	/// </summary>
	private readonly List<DeadPlayerEntry> _deadPlayers = [];

	/// <summary>
	/// 可点击文字列表（每行对应一个玩家）
	/// </summary>
	private readonly List<Button> _playerButtons = new List<Button>();

	/// <summary>
	/// 当前选中行的 Button
	/// </summary>

	/// <summary>
	/// 可点击行文字模板（从工厂传入）
	/// </summary>
	private TextMeshProUGUI _textTemplate;

	/// <summary>
	/// 当前选中的目标玩家
	/// </summary>
	private DeadPlayerEntry _selectedPlayer;

	private string _selfPlayerId;
	private string _pendingRequestId;

	/// <summary>
	/// 面板负载数据
	/// </summary>
	private ResurrectPayload _payload;

	/// <summary>
	/// 画布组组件（用于控制透明度和交互）
	/// </summary>
	private CanvasGroup _canvasGroup;

	/// <summary>
	/// 是否允许取消操作
	/// </summary>
	private bool _canCancel = true;

	/// <summary>
	/// 本次打开治疗面板期间是否成功完成过一次治疗。
	/// </summary>
	public bool DidCompleteTreatment { get; private set; }
	#endregion

	#region 属性
	/// <summary>
	/// 面板层级（置顶显示）
	/// </summary>
	public override PanelLayer Layer => PanelLayer.Top;
	#endregion

	#region Unity生命周期
	internal void BindRuntimeUi(
		Transform runtimeDeadPlayersContainer,
		TextMeshProUGUI runtimeTextTemplate,
		CommonButtonWidget runtimeResurrectButton,
		CommonButtonWidget runtimeCancelButton,
		TextMeshProUGUI runtimeStatusText,
		CanvasGroup runtimeCanvasGroup)
	{
		deadPlayersContainer = runtimeDeadPlayersContainer;
		_textTemplate = runtimeTextTemplate;
		resurrectButton = runtimeResurrectButton;
		cancelButton = runtimeCancelButton;
		statusText = runtimeStatusText;

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

	/// <summary>
	/// Unity初始化方法
	/// 获取或添加CanvasGroup组件，并注册按钮事件监听器
	/// </summary>
	public void Awake()
	{
		// 获取CanvasGroup组件
		_canvasGroup = GetComponent<CanvasGroup>();
		if (_canvasGroup == null)
		{
			// 如果不存在则添加
			_canvasGroup = gameObject.AddComponent<CanvasGroup>();
		}

		// 注册治疗按钮点击事件
		if (resurrectButton?.button != null)
		{
			resurrectButton.button.onClick.AddListener(OnResurrectPlayer);
		}

		// 注册取消按钮点击事件
		if (cancelButton?.button != null)
		{
			cancelButton.button.onClick.AddListener(OnCancelResurrect);
		}
	}
	#endregion

	#region 本地化处理
	/// <summary>
	/// 本地化更改时的回调
	/// 更新界面文本以适应新的语言设置
	/// </summary>
	public override void OnLocaleChanged()
	{
		if (_payload != null)
		{
			// 更新UI字符串
			UpdateUIStrings();
		}
	}
	#endregion

	#region 面板显示/隐藏
	/// <summary>
	/// 面板显示前回调。
	/// 初始化面板数据，加载可治疗玩家列表并创建 UI。
	/// </summary>
	/// <param name="payload">面板负载数据，包含目标玩家信息。</param>
	protected override void OnShowing(ResurrectPayload payload)
	{
		EnsurePopupTopmost();

		// 保存负载数据
		_payload = payload;
		// 获取是否允许取消
		_canCancel = payload?.CanCancel ?? true;

		// 重置面板数据
		ResetResurrectData();

		var list = payload?.Players;
		if (list != null && list.Count > 0)
		{
			_deadPlayers.AddRange(list);
		}

		// 记录 self PlayerId
		INetworkClient client = ModService.ServiceProvider?.GetService<INetworkClient>();
		if (client != null)
		{
			NetworkIdentityTracker.EnsureSubscribed(client);
		}
		_selfPlayerId = NetworkIdentityTracker.GetSelfPlayerId();

		_pendingRequestId = null;
		DidCompleteTreatment = false;

		// 如果没有可治疗玩家，显示提示并自动隐藏
		if (_deadPlayers.Count == 0)
		{
			UpdateUIStatus("当前没有可治疗的玩家");
			_canvasGroup.alpha = 0f;
			StartCoroutine(HideAfterDelay(2f));
			return;
		}

		// 设置取消按钮可见性
		cancelButton?.gameObject.SetActive(_canCancel);
		resurrectButton?.gameObject.SetActive(false);

		// 启用交互
		_canvasGroup.interactable = true;

		// 创建UI条目
		// 清除现有条目
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
				Plugin.Logger?.LogDebug("[ResurrectPanel] 创建可点击文字行: index={i}, playerId={player?.PlayerId}, name={player?.PlayerName}");
			}
		}

		// 更新UI字符串
		UpdateUIStrings();
		ResurrectSyncPatch.OnResurrectResult += OnResurrectResult;
		// 注册输入处理器
		UiManager.PushActionHandler(this);
	}

	/// <summary>
	/// 面板显示完成回调
	/// 面板完全显示后的处理
	/// </summary>
	protected override void OnShown()
	{
		// 面板显示完成后再次置顶，避免与 GapOptionsPanel/OptionWidget sibling 顺序竞争。
		EnsurePopupTopmost();
	}

	/// <summary>
	/// 面板隐藏前回调
	/// 禁用交互并移除输入处理器
	/// </summary>
	protected override void OnHiding()
	{
		// 禁用交互
		_canvasGroup.interactable = false;
		ResurrectSyncPatch.OnResurrectResult -= OnResurrectResult;
		// 移除输入处理器
		UiManager.PopActionHandler(this);
	}

	/// <summary>
	/// 面板隐藏完成回调
	/// 清理面板数据
	/// </summary>
	protected override void OnHided()
	{
		// 重置面板数据
		ResetResurrectData();
		// 清空负载数据
		_payload = null;
	}
	#endregion

	#region UI更新方法
	/// <summary>
	/// 更新UI字符串
	/// 根据当前选择状态更新界面文本
	/// </summary>
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

	/// <summary>
	/// 重置面板数据。
	/// </summary>
	private void ResetResurrectData()
	{
		// 清空玩家列表
		_deadPlayers.Clear();
		// 清空选中玩家
		_selectedPlayer = null;

		// 销毁所有行条目
		if (deadPlayersContainer != null)
		{
			foreach (Transform child in deadPlayersContainer)
			{
				Destroy(child.gameObject);
			}
		}

		// 清空按钮列表
		_playerButtons.Clear();

		// 禁用确认按钮
		if (resurrectButton?.button != null)
		{
			resurrectButton.button.interactable = false;
		}
	}

	/// <summary>
	/// 更新UI状态文本
	/// </summary>
	/// <param name="message">要显示的消息</param>
        private void UpdateUIStatus(string message)
        {
                if (statusText != null) statusText.text = message;
        }



	/// <summary>
	/// 创建单行可点击文字条目（与 TradePanel CreateTextButton 相同风格）
	/// </summary>
	private Button CreatePlayerRow(DeadPlayerEntry player, int index)
	{
		if (_textTemplate == null || deadPlayersContainer == null)
		{
			Plugin.Logger?.LogWarning("[ResurrectPanel] 文字模板或容器为空，无法创建行");
			return null;
		}

		string displayName = string.IsNullOrWhiteSpace(player?.PlayerName) ? player?.PlayerId : player.PlayerName;
		int actionValue = player.ActionValue > 0 ? player.ActionValue : player.ResurrectionCost;
		string actionText = actionValue > 0 ? $"治疗+{actionValue}" : "不可治疗";
		string label = $"{displayName}  HP {player.CurrentHp}/{player.MaxHp}  |  {actionText}";

		// 克隆游戏内 TMP 模板，保留字体/材质
		var tmp = UnityEngine.Object.Instantiate(_textTemplate, deadPlayersContainer, false);
		tmp.name = $"PlayerRow_{index}";
		tmp.text = label;
		tmp.alignment = TextAlignmentOptions.Center;
		tmp.raycastTarget = true;
		tmp.enableAutoSizing = false;
		// 与 TradePanel partner picker 一致：使用模板字号的 50%
		float fontSize = _textTemplate.fontSize > 0 ? _textTemplate.fontSize * 0.5f : 18f;
		tmp.fontSize = Mathf.Max(fontSize, 14f);
		tmp.fontSizeMin = 1f;
		tmp.fontSizeMax = tmp.fontSize;

		Color baseColor = player.CanResurrect
			? new Color(0.92f, 0.92f, 0.92f, 1f)
			: new Color(0.5f, 0.5f, 0.5f, 0.6f);
		tmp.color = baseColor;

		var le = tmp.gameObject.AddComponent<LayoutElement>();
		le.preferredHeight = 40f;
		le.flexibleWidth = 1f;

		var btn = tmp.gameObject.AddComponent<Button>();
		btn.targetGraphic = tmp;
		btn.interactable = player.CanResurrect;
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

		// 悬停放大效果（与 TradePanel TextButtonHover 一致）
		tmp.gameObject.AddComponent<RowHoverScale>();

		var capturedPlayer = player;
		btn.onClick.RemoveAllListeners();
		btn.onClick.AddListener(() =>
		{
			// 检查是否可以治疗
			if (!capturedPlayer.CanResurrect)
			{
				UpdateUIStatus("该玩家当前不需要治疗");
				return;
			}

			if (!string.IsNullOrWhiteSpace(_pendingRequestId))
			{
				UpdateUIStatus("正在治疗中，请稍候...");
				return;
			}

			// 设置当前选中的玩家
			_selectedPlayer = capturedPlayer;

			if ((ModService.ServiceProvider?.GetService<ConfigManager>())?.DebugVirtualPlayerAiDefault?.Value == true
				&& (string.Equals(capturedPlayer.PlayerId, "aidefault", StringComparison.Ordinal)
					|| string.Equals(capturedPlayer.PlayerId, "aidefault2", StringComparison.Ordinal)))
			{
				_pendingRequestId = Guid.NewGuid().ToString("N");
				OnResurrectResult(_pendingRequestId, true, null);
				return;
			}

			// 点击玩家即治疗，无需展开详情与二次确认。
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
			DeadPlayerEntry player = _deadPlayers[i];
			if (btn == null)
			{
				continue;
			}

			btn.interactable = interactable && player != null && player.CanResurrect;
		}
	}

	#region 按钮事件处理
	/// <summary>
	/// 治疗按钮点击事件。
	/// </summary>
	private void OnResurrectPlayer()
	{
		// 验证选中玩家是否有效
		if (_selectedPlayer == null || !_selectedPlayer.CanResurrect)
			return;

		if (!string.IsNullOrWhiteSpace(_pendingRequestId))
			return;

		// 先发请求，等待 Host 回包。
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

		// 禁用按钮，等待结果
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

	/// <summary>
	/// 取消按钮点击事件。
	/// 清除选择并隐藏面板
	/// </summary>
	private void OnCancelResurrect()
	{
		// 清空选中玩家
		_selectedPlayer = null;
		// 隐藏面板
		Hide();
	}

	/// <summary>
	/// 输入取消事件处理（IInputActionHandler接口实现）
	/// 当允许取消时执行取消操作
	/// </summary>
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
			// ignored
		}
	}

	#region 辅助方法
	/// <summary>
	/// 延迟隐藏面板协程
	/// 等待指定时间后自动隐藏面板
	/// </summary>
	/// <param name="delay">延迟时间（秒）</param>
	private IEnumerator HideAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		Hide();
	}

	/// <summary>
	/// 显示复活 UI 的协程方法
	/// </summary>
	public IEnumerator ShowResurrectAsync(ResurrectPayload payload)
	{
		Show(payload);
		yield return new WaitWhile(() => IsVisible);
	}
	#endregion
}

/// <summary>
/// 悬停放大效果（与 TradePanel TextButtonHover 相同行为）
/// </summary>
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
