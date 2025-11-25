using System;
using LBoL.Core;
using LBoL.Presentation.InputSystemExtend;
namespace LBoL.Presentation.UI
{
	public abstract class UiDialogBase : UiBase, IInteractablePanel
	{
		public string GetPanelName()
		{
			string name = base.gameObject.name;
			if (!name.Contains("(clone)", 5))
			{
				return name;
			}
			return name.Replace("(clone)", "", 5).Trim();
		}
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
		protected sealed override void OnTransitionOutFinished()
		{
			base.OnTransitionOutFinished();
			Singleton<GamepadNavigationManager>.Instance.RemovePanel(this);
			Singleton<GamepadNavigationManager>.Instance.SetAvailable(true);
			GamepadNavigationManager.RefreshSelection();
		}
	}
}
