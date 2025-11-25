using System;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.ExtraWidgets
{
	public class PuzzleToggleWidget : CommonToggleWidget
	{
		public Toggle Toggle
		{
			get
			{
				return this.toggle;
			}
		}
		public bool IsLock
		{
			get
			{
				return this._isLock;
			}
			set
			{
				this._isLock = value;
				this.toggle.interactable = !value;
				base.SetLock(!value);
				if (value)
				{
					PuzzleConfig puzzleConfig = PuzzleConfig.FromId(this._puzzleFlag.ToString());
					string text = string.Format("StartGame.LockPuzzle".Localize(true), puzzleConfig.UnlockLevel.ToString());
					this._tooltip.SetDirect("StartGame.Puzzle".Localize(true), text);
					return;
				}
				PuzzleFlagDisplayWord displayWord = PuzzleFlags.GetDisplayWord(this._puzzleFlag);
				this._tooltip.SetDirect(displayWord.Name, displayWord.Description);
			}
		}
		public void SetEnd()
		{
			this.lineImage.gameObject.SetActive(false);
		}
		public void SetPuzzle(PuzzleFlag flag)
		{
			this._puzzleFlag = flag;
			PuzzleFlagDisplayWord displayWord = PuzzleFlags.GetDisplayWord(flag);
			this._tooltip = SimpleTooltipSource.CreateDirect(base.gameObject, displayWord.Name, displayWord.Description);
		}
		[SerializeField]
		private Image lineImage;
		private SimpleTooltipSource _tooltip;
		private PuzzleFlag _puzzleFlag;
		private bool _isLock;
	}
}
