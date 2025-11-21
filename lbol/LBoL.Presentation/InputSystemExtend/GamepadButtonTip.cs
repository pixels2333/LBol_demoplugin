using System;
using LBoL.Core;
using UnityEngine;
using UnityEngine.Events;

namespace LBoL.Presentation.InputSystemExtend
{
	// Token: 0x020000E6 RID: 230
	public class GamepadButtonTip : GamepadBehaviour
	{
		// Token: 0x06000D88 RID: 3464 RVA: 0x00041634 File Offset: 0x0003F834
		protected override void OnEnable()
		{
			base.OnEnable();
			if (this.bindingCursor != null)
			{
				this.bindingCursor.OnSelectChanged.AddListener(new UnityAction<bool>(this.OnBindingCursorSelectChanged));
			}
			if (this.bindingPanel)
			{
				Singleton<GamepadNavigationManager>.Instance.OnPanelUpdate.AddListener(new UnityAction(this.UpdateTip));
			}
		}

		// Token: 0x06000D89 RID: 3465 RVA: 0x00041694 File Offset: 0x0003F894
		protected override void OnDisable()
		{
			base.OnDisable();
			if (this.bindingCursor != null)
			{
				this.bindingCursor.OnSelectChanged.RemoveListener(new UnityAction<bool>(this.OnBindingCursorSelectChanged));
			}
			if (this.bindingPanel)
			{
				GamepadNavigationManager instance = Singleton<GamepadNavigationManager>.Instance;
				if (instance == null)
				{
					return;
				}
				instance.OnPanelUpdate.RemoveListener(new UnityAction(this.UpdateTip));
			}
		}

		// Token: 0x06000D8A RID: 3466 RVA: 0x000416F9 File Offset: 0x0003F8F9
		protected override void OnInputDeviceChanged(InputDeviceType inputDevice)
		{
			this.UpdateTip();
		}

		// Token: 0x06000D8B RID: 3467 RVA: 0x00041701 File Offset: 0x0003F901
		private void OnBindingCursorSelectChanged(bool value)
		{
			if (value)
			{
				this.UpdateTip();
				return;
			}
			if (this.gamepadTip != null)
			{
				this.gamepadTip.SetActive(false);
			}
		}

		// Token: 0x06000D8C RID: 3468 RVA: 0x00041728 File Offset: 0x0003F928
		private void UpdateTip()
		{
			if (this.gamepadTip == null)
			{
				return;
			}
			if (!base.IsParentPanelAvailable())
			{
				this.gamepadTip.SetActive(false);
				return;
			}
			bool flag = Singleton<InputDeviceManager>.Instance.CurrentInputDevice == InputDeviceType.Gamepad;
			if (this.bindingCursor != null)
			{
				flag = this.bindingCursor.IsSelected && flag;
			}
			this.gamepadTip.SetActive(flag);
		}

		// Token: 0x04000A3B RID: 2619
		[SerializeField]
		private GameObject gamepadTip;

		// Token: 0x04000A3C RID: 2620
		[Tooltip("如果要使提示仅在被摇杆光标选中时显示,请关联该引用,否则置空即可")]
		[SerializeField]
		private GamepadButtonCursor bindingCursor;
	}
}
