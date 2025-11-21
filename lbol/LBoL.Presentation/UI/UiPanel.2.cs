using System;
using LBoL.Core;
using LBoL.Presentation.InputSystemExtend;
using UnityEngine;

namespace LBoL.Presentation.UI
{
	// Token: 0x02000037 RID: 55
	public abstract class UiPanel<TPayload> : UiPanelBase
	{
		// Token: 0x060003C3 RID: 963 RVA: 0x0000FBE8 File Offset: 0x0000DDE8
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

		// Token: 0x060003C4 RID: 964 RVA: 0x0000FC5E File Offset: 0x0000DE5E
		protected virtual void OnShowing(TPayload payload)
		{
		}
	}
}
