using System;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.ExtraWidgets
{
	// Token: 0x020000CE RID: 206
	public class PuzzleToggleWidget : CommonToggleWidget
	{
		// Token: 0x17000200 RID: 512
		// (get) Token: 0x06000C86 RID: 3206 RVA: 0x0003F495 File Offset: 0x0003D695
		public Toggle Toggle
		{
			get
			{
				return this.toggle;
			}
		}

		// Token: 0x17000201 RID: 513
		// (get) Token: 0x06000C87 RID: 3207 RVA: 0x0003F49D File Offset: 0x0003D69D
		// (set) Token: 0x06000C88 RID: 3208 RVA: 0x0003F4A8 File Offset: 0x0003D6A8
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

		// Token: 0x06000C89 RID: 3209 RVA: 0x0003F549 File Offset: 0x0003D749
		public void SetEnd()
		{
			this.lineImage.gameObject.SetActive(false);
		}

		// Token: 0x06000C8A RID: 3210 RVA: 0x0003F55C File Offset: 0x0003D75C
		public void SetPuzzle(PuzzleFlag flag)
		{
			this._puzzleFlag = flag;
			PuzzleFlagDisplayWord displayWord = PuzzleFlags.GetDisplayWord(flag);
			this._tooltip = SimpleTooltipSource.CreateDirect(base.gameObject, displayWord.Name, displayWord.Description);
		}

		// Token: 0x0400099B RID: 2459
		[SerializeField]
		private Image lineImage;

		// Token: 0x0400099C RID: 2460
		private SimpleTooltipSource _tooltip;

		// Token: 0x0400099D RID: 2461
		private PuzzleFlag _puzzleFlag;

		// Token: 0x0400099E RID: 2462
		private bool _isLock;
	}
}
