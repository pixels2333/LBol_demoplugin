using System;
using System.Collections;
using LBoL.Core;
using LBoL.Presentation.InputSystemExtend;
using UnityEngine;
namespace LBoL.Presentation.UI
{
	public abstract class UiDialog<TPayload> : UiDialogBase
	{
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
		public IEnumerator ShowAsync(TPayload payload)
		{
			this.Show(payload);
			yield return new WaitUntil(() => base.IsVisible = false);
			yield break;
		}
		protected virtual void OnShowing(TPayload payload)
		{
		}
		public void Hide()
		{
			this.Hide(true);
		}
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
