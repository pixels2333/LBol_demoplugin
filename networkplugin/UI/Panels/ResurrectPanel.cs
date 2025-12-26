using System;
using System.Collections;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Widgets;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Core;
using NetworkPlugin.Network;
using NetworkPlugin.UI.Widgets;
using TMPro;
using UnityEngine;

namespace NetworkPlugin.UI.Panels;

/// <summary>
/// 复活面板类
/// 处理玩家之间的复活功能，使用游戏内部的UI元素
/// </summary>
public class ResurrectPanel : UiPanel<ResurrectPayload>, IInputActionHandler
{
	#region 常量定义
	/// <summary>
	/// 复活完成后等待时间（秒）
	/// </summary>
	private const float ResurrectCompleteWaitTime = 2f;
	#endregion

	#region UI组件引用
	/// <summary>
	/// 死亡玩家列表容器
	/// </summary>
	[SerializeField]
	private Transform deadPlayersContainer;

	/// <summary>
	/// 死亡玩家条目预制体模板
	/// </summary>
	[SerializeField]
	private DeadPlayerEntryWidget deadPlayerEntryTemplate;

	/// <summary>
	/// 复活按钮
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
	/// 复活费用文本显示
	/// </summary>
	[SerializeField]
	private TextMeshProUGUI costText;

	/// <summary>
	/// 当前金币数量显示
	/// </summary>
	[SerializeField]
	private TextMeshProUGUI goldAmount;
	#endregion

	#region 复活数据
	/// <summary>
	/// 死亡玩家列表
	/// </summary>
	private readonly List<DeadPlayerEntry> _deadPlayers = [];

	/// <summary>
	/// 玩家UI组件列表
	/// </summary>
	private readonly List<DeadPlayerEntryWidget> _playerWidgets = [];

	/// <summary>
	/// 当前选中的死亡玩家
	/// </summary>
	private DeadPlayerEntry _selectedPlayer = null;

	/// <summary>
	/// 复活费用
	/// </summary>
	private int _resurrectionCost = 0;

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
	#endregion

	#region 属性
	/// <summary>
	/// 面板层级（置顶显示）
	/// </summary>
	public override PanelLayer Layer => PanelLayer.Top;
	#endregion

	#region Unity生命周期
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

		// 注册复活按钮点击事件
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
	/// 面板显示前回调
	/// 初始化面板数据，加载死亡玩家列表并创建UI
	/// </summary>
	/// <param name="payload">面板负载数据，包含死亡玩家信息</param>
	protected override void OnShowing(ResurrectPayload payload)
	{
		// 保存负载数据
		_payload = payload;
		// 获取是否允许取消
		_canCancel = payload?.CanCancel ?? true;

		// 重置复活数据
		ResetResurrectData();

		// 从payload加载死亡玩家列表
		if (payload?.DeadPlayers != null)
		{
			_deadPlayers.AddRange(payload.DeadPlayers);
		}

		// 如果没有死亡玩家，显示提示并自动隐藏
		if (_deadPlayers.Count == 0)
		{
			UpdateUIStatus("Resurrect.NoDead".Localize());
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

		// 更新金币显示
		UpdateGoldDisplay();

		// 更新UI字符串
		UpdateUIStrings();
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
		// 移除输入处理器
		UiManager.PopActionHandler(this);
	}

	/// <summary>
	/// 面板隐藏完成回调
	/// 清理面板数据
	/// </summary>
	protected override void OnHided()
	{
		// 重置复活数据
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
		if (_selectedPlayer == null)
		{
			// 未选择玩家时显示提示
			UpdateUIStatus("Resurrect.SelectPlayer".Localize());
		}
	}

	/// <summary>
	/// 重置复活数据
	/// 清空所有死亡玩家数据和UI组件
	/// </summary>
	private void ResetResurrectData()
	{
		// 清空死亡玩家列表
		_deadPlayers.Clear();
		// 清空选中玩家
		_selectedPlayer = null;
		// 重置复活费用
		_resurrectionCost = 0;

		// 清空复活槽位
		if (deadPlayersContainer != null)
		{
			foreach (Transform child in deadPlayersContainer)
			{
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
	/// 为所有死亡玩家创建UI条目
	/// </summary>
	private void CreatePlayerEntries()
	{
		// 清除现有条目
		if (deadPlayersContainer != null)
		{
			foreach (Transform child in deadPlayersContainer)
			{
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
		}
	}

	/// <summary>
	/// 创建单个玩家条目
	/// </summary>
	/// <param name="player">死亡玩家数据</param>
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

		// 设置玩家数据
		entry.SetPlayer(player);

		// 注册点击事件
		entry.button?.onClick.AddListener(() => OnPlayerSelected(player, entry));

		return entry;
	}
	#endregion

	#region 玩家选择处理
	/// <summary>
	/// 玩家选择事件处理
	/// 当玩家点击某个死亡玩家条目时触发
	/// </summary>
	/// <param name="player">被选中的死亡玩家</param>
	/// <param name="widget">对应的UI组件</param>
	private void OnPlayerSelected(DeadPlayerEntry player, DeadPlayerEntryWidget widget)
	{
		// 检查是否可以复活
		if (!player.CanResurrect)
		{
			UpdateUIStatus("Resurrect.CannotResurrect".Localize());
			return;
		}

		// 取消之前选择的玩家
		foreach (var w in _playerWidgets)
		{
			w?.SetSelected(false);
		}

		// 设置当前选中的玩家
		_selectedPlayer = player;
		// 获取复活费用
		_resurrectionCost = player.ResurrectionCost;

		// 更新选中状态
		widget?.SetSelected(true);

		// 更新UI显示
		UpdateUIStatus($"{player.PlayerName} - {_resurrectionCost} Gold");

		// 更新费用文本
		costText?.text = "Resurrect.Cost".Localize() + $": {_resurrectionCost}";

		// 检查是否有足够金币
		bool canAfford = GameRun.Money >= _resurrectionCost;
		if (resurrectButton?.button != null)
		{
			// 根据金币是否足够启用/禁用复活按钮
			resurrectButton.button.interactable = canAfford;
		}

		// 金币不足时显示提示
		if (!canAfford && statusText != null)
		{
			statusText.text += "\n" + "Resurrect.InsufficientGold".Localize();
		}
	}

	/// <summary>
	/// 更新金币显示
	/// 显示当前游戏中的金币数量
	/// </summary>
	private void UpdateGoldDisplay()
	{
		goldAmount?.text = "Resurrect.CurrentGold".Localize() + $": {GameRun.Money}";
	}
	#endregion

	#region 按钮事件处理
	/// <summary>
	/// 复活按钮点击事件
	/// 扣除金币并执行复活操作
	/// </summary>
	private void OnResurrectPlayer()
	{
		// 验证选中玩家是否有效
		if (_selectedPlayer == null || !_selectedPlayer.CanResurrect)
			return;

		// 检查金币是否足够
		if (GameRun.Money < _resurrectionCost)
		{
			UpdateUIStatus("Resurrect.InsufficientGold".Localize());
			return;
		}

		// 扣除金币
		GameRun.ConsumeMoney(_resurrectionCost);

		// 执行复活逻辑
		StartCoroutine(ExecuteResurrection());
	}

	/// <summary>
	/// 取消复活按钮点击事件
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
	/// 执行复活协程
	/// 处理复活流程，包括UI更新、实际复活和网络同步
	/// </summary>
	private IEnumerator ExecuteResurrection()
	{
		// 禁用按钮以防止重复复活
		if (resurrectButton?.button != null)
		{
			resurrectButton.button.interactable = false;
		}

		if (cancelButton?.button != null)
		{
			cancelButton.button.interactable = false;
		}

		// 更新状态文本
		UpdateUIStatus("Resurrect.Resurrecting".Localize() + ": " + _selectedPlayer.PlayerName);

		// 等待1秒
		yield return new WaitForSeconds(1f);

		// 执行实际的复活逻辑
		yield return ResurrectPlayer(_selectedPlayer);

		// 更新状态为复活完成
		UpdateUIStatus("Resurrect.Resurrected".Localize() + ": " + _selectedPlayer.PlayerName);

		// 发送网络事件
		SendResurrectionEvent(_selectedPlayer);

		// 等待复活完成时间
		yield return new WaitForSeconds(ResurrectCompleteWaitTime);

		// 隐藏面板
		Hide();
	}

	/// <summary>
	/// 复活玩家协程
	/// 执行实际的玩家复活逻辑
	/// </summary>
	/// <param name="player">要复活的死亡玩家</param>
	private IEnumerator ResurrectPlayer(DeadPlayerEntry player)
	{
		// 实际的复活逻辑
		// 这里需要根据游戏的具体实现来处理
		// 示例：恢复玩家的生命值，重新加入游戏等

		// 如果是本地玩家，直接恢复
		if (player.PlayerName == GameRun.Player.Name)
		{
			// 恢复满生命值
			GameRun.Heal(GameRun.Player.MaxHp, false, null);
			// 通过Heal方法自动恢复玩家状态为Alive
		}
		else
		{
			// 其他玩家需要通过网络同步
			// 发送复活请求到服务器或其他玩家
		}

		yield return null;
	}

	/// <summary>
	/// 发送复活事件到网络
	/// 将复活操作同步到其他玩家
	/// </summary>
	/// <param name="player">被复活的玩家</param>
	private void SendResurrectionEvent(DeadPlayerEntry player)
	{
		try
		{
			// 获取同步管理器
			ISynchronizationManager syncManager = ModService.ServiceProvider.GetService<ISynchronizationManager>();
			if (syncManager != null)
			{
				// 构建复活数据字典
				Dictionary<string, object> resurrectionData = new Dictionary<string, object>
				{
					["EventType"] = "PlayerResurrected",
					["PlayerName"] = player.PlayerName,
					["Cost"] = _resurrectionCost,
					["Timestamp"] = DateTime.Now.Ticks
				};

				// TODO: 根据实际的网络管理器实现发送逻辑
				// syncManager.SendGameEvent(resurrectionData);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError($"[ResurrectPanel] 发送复活事件失败: {ex.Message}");
		}
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
	#endregion
}
