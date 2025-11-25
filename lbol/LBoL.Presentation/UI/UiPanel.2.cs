using System;
using LBoL.Core;
using LBoL.Presentation.InputSystemExtend;
using UnityEngine;
namespace LBoL.Presentation.UI
{
	public abstract class UiPanel<TPayload> : UiPanelBase
	{
		public void Show(TPayload payload)
		{
			if (base.IsVisible)
			{
				Debug.LogError("Cannot reshow " + base.GetType().Name);
				return;
			}
			if (!this.enableGamepadInputInTransitionIn)
			{
				Singleton<GamepadNavigationManager>.Instance.SetAvailable(false);
			}
			else
			{
				Singleton<GamepadNavigationManager>.Instance.AddPanel(this);
			}
			base.KillTransitionOut();
			base.gameObject.SetActive(true);
			this.OnShowing(payload);
			base.IsVisible = true;
			base.DoTransitionIn();
		}
		protected virtual void OnShowing(TPayload payload)
		{
		}
	}
}
