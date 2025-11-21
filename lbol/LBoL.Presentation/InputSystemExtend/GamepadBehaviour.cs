using System;
using LBoL.Core;
using UnityEngine;
using UnityEngine.Events;

namespace LBoL.Presentation.InputSystemExtend
{
	// Token: 0x020000E1 RID: 225
	public class GamepadBehaviour : MonoBehaviour
	{
		// Token: 0x06000D6F RID: 3439 RVA: 0x00041291 File Offset: 0x0003F491
		protected virtual void Start()
		{
			if (this.bindingPanel && string.IsNullOrEmpty(this.parentPanelName))
			{
				this.FindBindingPanel();
			}
		}

		// Token: 0x06000D70 RID: 3440 RVA: 0x000412B0 File Offset: 0x0003F4B0
		protected virtual void OnEnable()
		{
			if (this.bindingPanel && string.IsNullOrEmpty(this.parentPanelName))
			{
				this.FindBindingPanel();
			}
			Singleton<InputDeviceManager>.Instance.OnInputDeviceChanged.AddListener(new UnityAction<InputDeviceType>(this.OnInputDeviceChanged));
			this.OnInputDeviceChanged(Singleton<InputDeviceManager>.Instance.CurrentInputDevice);
		}

		// Token: 0x06000D71 RID: 3441 RVA: 0x00041304 File Offset: 0x0003F504
		protected virtual void OnDisable()
		{
			InputDeviceManager instance = Singleton<InputDeviceManager>.Instance;
			if (instance == null)
			{
				return;
			}
			instance.OnInputDeviceChanged.RemoveListener(new UnityAction<InputDeviceType>(this.OnInputDeviceChanged));
		}

		// Token: 0x06000D72 RID: 3442 RVA: 0x00041327 File Offset: 0x0003F527
		protected virtual void OnInputDeviceChanged(InputDeviceType inputDevice)
		{
		}

		// Token: 0x06000D73 RID: 3443 RVA: 0x00041329 File Offset: 0x0003F529
		public bool IsParentPanelAvailable()
		{
			return this.parentPanelName == Singleton<GamepadNavigationManager>.Instance.GetTopPanel();
		}

		// Token: 0x06000D74 RID: 3444 RVA: 0x00041340 File Offset: 0x0003F540
		protected void FindBindingPanel()
		{
			IInteractablePanel componentInParent = base.GetComponentInParent<IInteractablePanel>(true);
			if (componentInParent != null)
			{
				this.parentPanelName = componentInParent.GetPanelName();
				return;
			}
			this.parentPanelName = "";
			this.bindingPanel = false;
		}

		// Token: 0x04000A21 RID: 2593
		[SerializeField]
		protected bool bindingPanel = true;

		// Token: 0x04000A22 RID: 2594
		protected string parentPanelName;
	}
}
