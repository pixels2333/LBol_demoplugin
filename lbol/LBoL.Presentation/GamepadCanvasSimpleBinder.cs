using System;
using LBoL.Core;
using LBoL.Presentation.InputSystemExtend;
using UnityEngine;
[RequireComponent(typeof(CanvasGroup))]
public class GamepadCanvasSimpleBinder : MonoBehaviour
{
	private void Start()
	{
		this.canvasGroup = base.GetComponent<CanvasGroup>();
		IInteractablePanel component = base.GetComponent<IInteractablePanel>();
		this.panelName = ((component != null) ? component.GetPanelName() : null);
	}
	private void Update()
	{
		if (this.panelName == null || this.panelName == string.Empty)
		{
			return;
		}
		if (Singleton<InputDeviceManager>.Instance.CurrentInputDevice == InputDeviceType.Gamepad)
		{
			this.canvasGroup.interactable = Singleton<GamepadNavigationManager>.Instance.GetTopPanel() == this.panelName;
			return;
		}
		this.canvasGroup.interactable = true;
	}
	private CanvasGroup canvasGroup;
	private string panelName;
}
