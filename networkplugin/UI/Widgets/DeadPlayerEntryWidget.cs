using LBoL.Presentation.UI.Widgets;
using NetworkPlugin.UI.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Widgets;

/// <summary>
/// 死亡玩家列表条目 Widget
/// 基于游戏 UI 模式（类似 StartStatusWidget）
/// 显示单个死亡玩家的信息
/// </summary>
public class DeadPlayerEntryWidget : CommonButtonWidget
{
	public DeadPlayerEntry PlayerEntry { get; private set; }
	public bool IsSelected { get; private set; }

	[SerializeField]
	private TextMeshProUGUI playerName;

	[SerializeField]
	private TextMeshProUGUI playerInfo;

	[SerializeField]
	private Image selectedIndicator;

	[SerializeField]
	private Color selectedColor = new Color(0.4f, 0.6f, 0.8f, 0.9f);

	[SerializeField]
	private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

	private Image _bgImage;

	private void Start()
	{
		if (_bgImage == null)
		{
			_bgImage = GetComponent<Image>();
		}
	}

	public void SetPlayer(DeadPlayerEntry player)
	{
		PlayerEntry = player;

		// 设置玩家名字
		if (playerName != null)
		{
			playerName.text = player.PlayerName;
		}

		// 设置玩家信息（等级、复活成本、死因）
		if (playerInfo != null)
		{
			playerInfo.text = $"Lv.{player.Level} | {player.ResurrectionCost}G | {player.DeadCause}";
		}

		// 设置文字颜色和按钮状态
		bool canResurrect = player.CanResurrect;
		Color textColor = canResurrect ? Color.white : Color.gray;

		if (playerName != null)
			playerName.color = textColor;

		if (playerInfo != null)
			playerInfo.color = textColor;

		// 设置按钮可交互性
		if (button != null)
		{
			button.interactable = canResurrect;
		}

		SetSelected(false);
	}

	public void SetSelected(bool selected)
	{
		IsSelected = selected;

		if (_bgImage == null)
		{
			_bgImage = GetComponent<Image>();
		}

		if (_bgImage != null)
		{
			_bgImage.color = selected ? selectedColor : normalColor;
		}

		if (selectedIndicator != null)
		{
			selectedIndicator.gameObject.SetActive(selected);
		}
	}
}
