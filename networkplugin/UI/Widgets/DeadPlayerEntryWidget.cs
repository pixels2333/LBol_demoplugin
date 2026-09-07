using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation.UI.Panels;
using NetworkPlugin.UI.Models;
using System.Reflection;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Widgets;

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
	private RecordRow _recordRow;
	private static readonly FieldInfo RecordRowGameResultTextField = typeof(RecordRow).GetField("gameResultText", BindingFlags.Instance | BindingFlags.NonPublic);
	private static readonly FieldInfo RecordRowDifficultyTextField = typeof(RecordRow).GetField("difficultyText", BindingFlags.Instance | BindingFlags.NonPublic);
	private static readonly FieldInfo RecordRowSelectedIndicatorField = typeof(RecordRow).GetField("selectedIndicator", BindingFlags.Instance | BindingFlags.NonPublic);

	private void Start()
	{
		_recordRow ??= GetComponent<RecordRow>();
		EnsureRuntimeBindings();

		if (_bgImage == null)
		{
			_bgImage = GetComponent<Image>();
		}
	}

	public void SetPlayer(DeadPlayerEntry player)
	{
		PlayerEntry = player;
		EnsureRuntimeBindings();
		string displayName = string.IsNullOrWhiteSpace(player?.PlayerName) ? player?.PlayerId : player.PlayerName;

		if (playerName != null) playerName.text = displayName;

		string statusText = string.IsNullOrWhiteSpace(player.StatusText)
			? player.DeadCause
			: player.StatusText;

		int actionValue = player.ActionValue > 0 ? player.ActionValue : player.ResurrectionCost;
		string actionText = actionValue > 0 ? $"治疗+{actionValue}" : "不可治疗";
		string infoLine = $"HP {player.CurrentHp}/{player.MaxHp} | {actionText} | {statusText}";
		if (playerInfo != null) playerInfo.text = infoLine;

		bool canResurrect = player.CanResurrect;
		Color textColor = canResurrect ? Color.white : Color.gray;

		if (playerName != null) playerName.color = textColor;

		if (playerInfo != null) playerInfo.color = textColor;

		if (playerName == null || playerInfo == null)
		{
			Plugin.Logger?.LogDebug($"[DeadPlayerEntryWidget] 文本绑定缺失: playerNameNull={playerName == null}, playerInfoNull={playerInfo == null}, playerId={player?.PlayerId}");
		}

		TryApplyRecordRowTextFallback(displayName, infoLine, textColor);

		if (button != null)
		{
			button.interactable = canResurrect;
		}
		else
		{
			_recordRow ??= GetComponent<RecordRow>();
			if (_recordRow != null)
			{
				_recordRow.enabled = canResurrect;
			}
		}

		SetSelected(false);
	}

	private void EnsureRuntimeBindings()
	{
		_recordRow ??= GetComponent<RecordRow>();

		TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);

		if (playerName == null)
		{
			playerName = texts.FirstOrDefault(t => t != null && t.name.IndexOf("PlayerName", System.StringComparison.OrdinalIgnoreCase) >= 0)
				?? texts.FirstOrDefault(t => t != null && t.name.IndexOf("gameResult", System.StringComparison.OrdinalIgnoreCase) >= 0)
				?? RecordRowGameResultTextField?.GetValue(_recordRow) as TextMeshProUGUI
				?? texts.FirstOrDefault();
		}

		if (playerInfo == null)
		{
			playerInfo = texts.FirstOrDefault(t => t != null && t.name.IndexOf("PlayerInfo", System.StringComparison.OrdinalIgnoreCase) >= 0)
				?? texts.FirstOrDefault(t => t != null && t.name.IndexOf("difficulty", System.StringComparison.OrdinalIgnoreCase) >= 0)
				?? RecordRowDifficultyTextField?.GetValue(_recordRow) as TextMeshProUGUI
				?? texts.FirstOrDefault(t => t != null && !ReferenceEquals(t, playerName));
		}

		if (selectedIndicator == null)
		{
			selectedIndicator = GetComponentsInChildren<Image>(true)
				.FirstOrDefault(i => i != null && i.name.IndexOf("Selected", System.StringComparison.OrdinalIgnoreCase) >= 0);

			if (selectedIndicator == null)
			{
				GameObject selectedGo = RecordRowSelectedIndicatorField?.GetValue(_recordRow) as GameObject;
				selectedIndicator = selectedGo?.GetComponent<Image>();
			}
		}

		if (playerName != null)
		{
			Color c = playerName.color;
			c.a = 1f;
			playerName.color = c;
		}

		if (playerInfo != null)
		{
			Color c = playerInfo.color;
			c.a = 1f;
			playerInfo.color = c;
		}
	}

	private void TryApplyRecordRowTextFallback(string displayName, string infoLine, Color textColor)
	{
		_recordRow ??= GetComponent<RecordRow>();
		if (_recordRow == null)
		{
			return;
		}

		TextMeshProUGUI rowName = playerName ?? RecordRowGameResultTextField?.GetValue(_recordRow) as TextMeshProUGUI;
		if (rowName != null)
		{
			rowName.text = displayName;
			rowName.color = textColor;
		}

		TextMeshProUGUI rowInfo = playerInfo ?? RecordRowDifficultyTextField?.GetValue(_recordRow) as TextMeshProUGUI;
		if (rowInfo != null)
		{
			rowInfo.text = infoLine;
			rowInfo.color = textColor;
		}
	}

	public void SetSelected(bool selected)
	{
		IsSelected = selected;

		_recordRow ??= GetComponent<RecordRow>();
		if (_recordRow != null)
		{
			_recordRow.SetSelected(selected, false);
		}

		if (_bgImage == null)
		{
			_bgImage = GetComponent<Image>();
		}

		if (_bgImage != null) _bgImage.color = selected ? selectedColor : normalColor;

		selectedIndicator?.gameObject.SetActive(selected);
	}
}
