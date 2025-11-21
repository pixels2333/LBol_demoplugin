using System;
using System.Collections;
using LBoL.Core;
using LBoL.Presentation.InputSystemExtend;
using UnityEngine;

namespace LBoL.Presentation.UI
{
	// Token: 0x02000031 RID: 49
	public abstract class UiDialog<TPayload> : UiDialogBase
	{
		// Token: 0x06000358 RID: 856 RVA: 0x0000E79C File Offset: 0x0000C99C
		public void Show(TPayload payload)
		{
			if (!this.enableGamepadInputInTransitionIn)
			{
				Singleton<GamepadNavigationManager>.Instance.SetAvailable(false);
			}
			else
			{
				Singleton<GamepadNavigationManager>.Instance.AddPanel(this);
			}
			UiManager.ShowDialogCheck(this);
			base.KillTransitionOut();
			this.OnShowing(payload);
			base.gameObject.SetActive(true);
			if (base.IsVisible)
			{
				this.OnTransitionInFinished();
				return;
			}
			base.IsVisible = true;
			base.DoTransitionIn();
		}

		// Token: 0x06000359 RID: 857 RVA: 0x0000E804 File Offset: 0x0000CA04
		public IEnumerator ShowAsync(TPayload payload)
		{
			this.Show(payload);
			yield return new WaitUntil(() => base.IsVisible = false);
			yield break;
		}

		// Token: 0x0600035A RID: 858 RVA: 0x0000E81A File Offset: 0x0000CA1A
		protected virtual void OnShowing(TPayload payload)
		{
		}

		// Token: 0x0600035B RID: 859 RVA: 0x0000E81C File Offset: 0x0000CA1C
		public void Hide()
		{
			this.Hide(true);
		}

		// Token: 0x0600035C RID: 860 RVA: 0x0000E828 File Offset: 0x0000CA28
		public void Hide(bool transition)
		{
			if (!this.enableGamepadInputInTransitionIn)
			{
				Singleton<GamepadNavigationManager>.Instance.SetAvailable(false);
			}
			else
			{
				Singleton<GamepadNavigationManager>.Instance.AddPanel(this);
			}
			UiManager.HideDialogCheck(this);
			base.KillTransitionIn();
			base.IsVisible = false;
			this.OnHiding();
			if (transition)
			{
				base.DoTransitionOut();
				return;
			}
			this.OnTransitionOutFinished();
		}
	}
}
