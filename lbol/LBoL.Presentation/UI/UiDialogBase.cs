using System;
using LBoL.Core;
using LBoL.Presentation.InputSystemExtend;

namespace LBoL.Presentation.UI
{
	// Token: 0x02000032 RID: 50
	public abstract class UiDialogBase : UiBase, IInteractablePanel
	{
		// Token: 0x0600035F RID: 863 RVA: 0x0000E8A0 File Offset: 0x0000CAA0
		public string GetPanelName()
		{
			string name = base.gameObject.name;
			if (!name.Contains("(clone)", 5))
			{
				return name;
			}
			return name.Replace("(clone)", "", 5).Trim();
		}

		// Token: 0x06000360 RID: 864 RVA: 0x0000E8DF File Offset: 0x0000CADF
		protected sealed override void OnTransitionInFinished()
		{
			base.OnTransitionInFinished();
			if (!this.enableGamepadInputInTransitionIn)
			{
				Singleton<GamepadNavigationManager>.Instance.AddPanel(this);
				Singleton<GamepadNavigationManager>.Instance.SetAvailable(true);
			}
			GamepadNavigationManager.RefreshSelection();
		}

		// Token: 0x06000361 RID: 865 RVA: 0x0000E90A File Offset: 0x0000CB0A
		protected sealed override void OnTransitionOutFinished()
		{
			base.OnTransitionOutFinished();
			Singleton<GamepadNavigationManager>.Instance.RemovePanel(this);
			Singleton<GamepadNavigationManager>.Instance.SetAvailable(true);
			GamepadNavigationManager.RefreshSelection();
		}
	}
}
