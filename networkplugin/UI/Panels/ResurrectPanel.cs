using System;
using System.Collections;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.UI.Models;
using NetworkPlugin.UI.Payloads;
using NetworkPlugin.UI.State;
using NetworkPlugin.UI.Widgets;
using NetworkPlugin.Utils;
using TMPro;
using UnityEngine;

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
	/// 玩家条目预制体模板
	/// </summary>
	[SerializeField]
	private DeadPlayerEntryWidget deadPlayerEntryTemplate;

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

	/// <summary>
	/// 治疗信息文本显示
	/// </summary>
	[SerializeField]
	private TextMeshProUGUI costText;

	/// <summary>
	/// 辅助说明文本显示
	/// </summary>
	[SerializeField]
	private TextMeshProUGUI goldAmount;
	#endregion

	#region 复活数据
	/// <summary>
	/// 可治疗玩家列表
	/// </summary>
	private readonly List<DeadPlayerEntry> _deadPlayers = [];

	/// <summary>
	/// 玩家UI组件列表
	/// </summary>
	private readonly List<DeadPlayerEntryWidget> _playerWidgets = [];

	/// <summary>
	/// 当前选中的目标玩家
	/// </summary>
	private DeadPlayerEntry _selectedPlayer;

	/// <summary>
	/// 治疗量
	/// </summary>
	private int _healingAmount = 0;

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
		DeadPlayerEntryWidget runtimeDeadPlayerEntryTemplate,
		CommonButtonWidget runtimeResurrectButton,
		CommonButtonWidget runtimeCancelButton,
		TextMeshProUGUI runtimeStatusText,
		TextMeshProUGUI runtimeCostText,
		TextMeshProUGUI runtimeGoldAmount,
		CanvasGroup runtimeCanvasGroup)
	{
		deadPlayersContainer = runtimeDeadPlayersContainer;
		deadPlayerEntryTemplate = runtimeDeadPlayerEntryTemplate;
		resurrectButton = runtimeResurrectButton;
		cancelButton = runtimeCancelButton;
		statusText = runtimeStatusText;
		costText = runtimeCostText;
		goldAmount = runtimeGoldAmount;

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

		// 启用交互
		_canvasGroup.interactable = true;

		// 创建UI条目
		CreatePlayerEntries();

		// 更新辅助说明
		UpdateHintDisplay();

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
		// 面板显示完成后的处理
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
		DidCompleteTreatment = false;
	}
	#endregion

	#region UI更新方法
	/// <summary>
	/// 更新UI字符串
	/// 根据当前选择状态更新界面文本
	/// </summary>
	private void UpdateUIStrings()
	{
		if (_selectedPlayer == null)
		{
			UpdateUIStatus("请选择要治疗的玩家");
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
		// 重置治疗量
		_healingAmount = 0;

		// 清空列表槽位
		if (deadPlayersContainer != null)
		{
			foreach (Transform child in deadPlayersContainer)
			{
				if (deadPlayerEntryTemplate != null && child == deadPlayerEntryTemplate.transform)
				{
					child.gameObject.SetActive(false);
					continue;
				}

				// 销毁所有子对象
				Destroy(child.gameObject);
			}
		}

		// 清空UI组件列表
		_playerWidgets.Clear();

		// 禁用确认按钮
		if (resurrectButton?.button != null)
		{
			resurrectButton.button.interactable = false;
		}

		if (costText != null)
		{
			costText.text = string.Empty;
		}

		if (goldAmount != null)
		{
			goldAmount.text = string.Empty;
		}
	}

	/// <summary>
	/// 更新UI状态文本
	/// </summary>
	/// <param name="message">要显示的消息</param>
        private void UpdateUIStatus(string message)
        {
                statusText?.text = message;
        }

	/// <summary>
	/// 创建玩家条目
	/// 为所有目标玩家创建 UI 条目。
	/// </summary>
	private void CreatePlayerEntries()
	{
		// 清除现有条目
		if (deadPlayersContainer != null)
		{
			foreach (Transform child in deadPlayersContainer)
			{
				if (deadPlayerEntryTemplate != null && child == deadPlayerEntryTemplate.transform)
				{
					child.gameObject.SetActive(false);
					continue;
				}

				Destroy(child.gameObject);
			}
		}

		// 清空UI组件列表
		_playerWidgets.Clear();

		// 创建新条目
		for (int i = 0; i < _deadPlayers.Count; i++)
		{
			var player = _deadPlayers[i];
			var entry = CreatePlayerEntry(player, i);
			_playerWidgets.Add(entry);

			Plugin.Logger?.LogDebug($"[ResurrectPanel] 渲染治疗条目: index={i}, playerId={player?.PlayerId}, playerName={player?.PlayerName}, active={entry?.gameObject?.activeSelf}");
		}
	}

	/// <summary>
	/// 创建单个玩家条目
	/// </summary>
	/// <param name="player">玩家数据</param>
	/// <param name="index">条目索引</param>
	/// <returns>创建的玩家条目组件</returns>
	private DeadPlayerEntryWidget CreatePlayerEntry(DeadPlayerEntry player, int index)
	{
		// 检查模板是否存在
		if (deadPlayerEntryTemplate == null)
		{
			Debug.LogError("[ResurrectPanel] deadPlayerEntryTemplate is not assigned!");
			return null;
		}

		// 实例化预制体
		var entry = Instantiate(deadPlayerEntryTemplate, deadPlayersContainer, false);
		entry.name = $"PlayerEntry_{index}";
		entry.gameObject.SetActive(true);

		// 设置玩家数据
		entry.SetPlayer(player);

		// 注册点击事件
		if (entry.button != null)
		{
			entry.button.onClick.RemoveAllListeners();
			entry.button.onClick.AddListener(() => OnPlayerSelected(player, entry));
		}
		else
		{
			RecordRow row = entry.GetComponent<RecordRow>();
			if (row != null)
			{
				row.Click += () => OnPlayerSelected(player, entry);
			}
		}

		return entry;
	}
	#endregion

	#region 玩家选择处理
	/// <summary>
	/// 玩家选择事件处理
	/// 当玩家点击某个目标玩家条目时触发。
	/// </summary>
	/// <param name="player">被选中的死亡玩家</param>
	/// <param name="widget">对应的UI组件</param>
	private void OnPlayerSelected(DeadPlayerEntry player, DeadPlayerEntryWidget widget)
	{
		// 检查是否可以治疗
		if (!player.CanResurrect)
		{
			UpdateUIStatus("该玩家当前不需要治疗");
			return;
		}

		// 取消之前选择的玩家
		foreach (var w in _playerWidgets)
		{
			w?.SetSelected(false);
		}

		// 设置当前选中的玩家
		_selectedPlayer = player;
		// 计算治疗量：固定为目标最大生命值的 20%。
		_healingAmount = player.ActionValue > 0 ? player.ActionValue : CalculateHealingAmount(player.MaxHp);
		if (_payload?.CostCalculator != null)
		{
			_healingAmount = Math.Max(1, _payload.CostCalculator(_healingAmount));
		}

		// 更新选中状态
		widget?.SetSelected(true);

		// 更新UI显示
		int finalHp = Math.Min(player.MaxHp, player.CurrentHp + _healingAmount);
		UpdateUIStatus($"{player.PlayerName}：{player.CurrentHp}/{player.MaxHp} → {finalHp}/{player.MaxHp}");

		// 更新治疗信息文本
		costText?.text = $"治疗量：+{_healingAmount} HP";
		goldAmount?.text = $"治疗后生命：{finalHp}/{player.MaxHp}";

		if (resurrectButton?.button != null)
		{
			resurrectButton.button.interactable = true;
		}
	}

	/// <summary>
	/// 更新辅助说明。
	/// </summary>
	private void UpdateHintDisplay()
	{
		goldAmount?.text = "效果：回复目标 20% 最大生命值";
	}
	#endregion

	#region 按钮事件处理
	/// <summary>
	/// 治疗按钮点击事件。
	/// </summary>
	private void OnResurrectPlayer()
	{
		// 验证选中玩家是否有效
		if (_selectedPlayer == null || !_selectedPlayer.CanResurrect)
			return;

		// 先发请求，等待 Host 回包。
		_pendingRequestId = Guid.NewGuid().ToString("N");
		SendResurrectionEvent(_selectedPlayer);

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

	#region 复活执行
	/// <summary>
	/// 发送治疗事件到网络。
	/// </summary>
	/// <param name="player">被治疗的玩家。</param>
	private void SendResurrectionEvent(DeadPlayerEntry player)
	{
		try
		{
			INetworkClient client = ModService.ServiceProvider.GetService<INetworkClient>();
			if (client == null)
			{
				return;
			}

			ResurrectSyncPatch.EnsureSubscribed(client);
			NetworkIdentityTracker.EnsureSubscribed(client);
			string requesterId = _selfPlayerId ?? NetworkIdentityTracker.GetSelfPlayerId();
			string targetId = player.PlayerId;
			if (string.IsNullOrWhiteSpace(requesterId) || string.IsNullOrWhiteSpace(targetId))
			{
				return;
			}

			client.SendGameEventData(NetworkMessageTypes.OnGapHealRequest, new
			{
				RequestId = _pendingRequestId,
				RequesterPlayerId = requesterId,
				TargetPlayerId = targetId,
				Timestamp = DateTime.UtcNow.Ticks,
			});
		}
		catch (Exception ex)
		{
			Debug.LogError($"[ResurrectPanel] 发送治疗事件失败: {ex.Message}");
		}
	}

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
			UpdateHintDisplay();
			UpdateUIStatus("治疗完成：" + _selectedPlayer?.PlayerName);
			StartCoroutine(HideAfterDelay(ResurrectCompleteWaitTime));
		}
		else
		{
			UpdateUIStatus("治疗失败" + (string.IsNullOrWhiteSpace(reason) ? string.Empty : $"：{reason}"));
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
	#endregion

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
