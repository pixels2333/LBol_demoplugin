using System;
using LBoL.Core;
using LBoL.Presentation.InputSystemExtend;
using UnityEngine;

// Token: 0x02000005 RID: 5
[RequireComponent(typeof(CanvasGroup))]
public class GamepadCanvasSimpleBinder : MonoBehaviour
{
	// Token: 0x06000015 RID: 21 RVA: 0x00002476 File Offset: 0x00000676
	private void Start()
	{
		this.canvasGroup = base.GetComponent<CanvasGroup>();
		IInteractablePanel component = base.GetComponent<IInteractablePanel>();
		this.panelName = ((component != null) ? component.GetPanelName() : null);
	}

	// Token: 0x06000016 RID: 22 RVA: 0x0000249C File Offset: 0x0000069C
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

	// Token: 0x04000009 RID: 9
	private CanvasGroup canvasGroup;

	// Token: 0x0400000A RID: 10
	private string panelName;
}
