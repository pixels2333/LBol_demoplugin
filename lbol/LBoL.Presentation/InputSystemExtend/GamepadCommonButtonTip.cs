using System;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.InputSystemExtend
{
	public class GamepadCommonButtonTip : GamepadButtonTip
	{
		protected override void OnInputDeviceChanged(InputDeviceType inputDevice)
		{
			if (this.keyboardOnlyIcon != null)
			{
				this.keyboardOnlyIcon.gameObject.SetActive(inputDevice == InputDeviceType.MouseAndKeyboard);
			}
			base.OnInputDeviceChanged(inputDevice);
		}
		[SerializeField]
		private Image keyboardOnlyIcon;
	}
}
