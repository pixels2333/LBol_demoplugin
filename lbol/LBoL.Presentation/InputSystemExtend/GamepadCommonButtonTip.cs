using System;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.InputSystemExtend
{
	// Token: 0x020000E8 RID: 232
	public class GamepadCommonButtonTip : GamepadButtonTip
	{
		// Token: 0x06000D94 RID: 3476 RVA: 0x000418C5 File Offset: 0x0003FAC5
		protected override void OnInputDeviceChanged(InputDeviceType inputDevice)
		{
			if (this.keyboardOnlyIcon != null)
			{
				this.keyboardOnlyIcon.gameObject.SetActive(inputDevice == InputDeviceType.MouseAndKeyboard);
			}
			base.OnInputDeviceChanged(inputDevice);
		}

		// Token: 0x04000A3D RID: 2621
		[SerializeField]
		private Image keyboardOnlyIcon;
	}
}
