using System;
using LBoL.Core;
using LBoL.Presentation.InputSystemExtend;
using UnityEngine;

namespace LBoL.Presentation.UI
{
	// Token: 0x02000035 RID: 53
	public abstract class UiPanel : UiPanelBase
	{
		// Token: 0x060003AB RID: 939 RVA: 0x0000F944 File Offset: 0x0000DB44
		public void Show()
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
			this.OnShowing();
			base.IsVisible = true;
			base.DoTransitionIn();
		}

		// Token: 0x060003AC RID: 940 RVA: 0x0000F9B9 File Offset: 0x0000DBB9
		protected virtual void OnShowing()
		{
		}
	}
}
