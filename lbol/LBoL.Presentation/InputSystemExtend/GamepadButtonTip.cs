using System;
using LBoL.Core;
using UnityEngine;
using UnityEngine.Events;
namespace LBoL.Presentation.InputSystemExtend
{
	public class GamepadButtonTip : GamepadBehaviour
	{
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
		protected override void OnInputDeviceChanged(InputDeviceType inputDevice)
		{
			this.UpdateTip();
		}
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
		[SerializeField]
		private GameObject gamepadTip;
		[Tooltip("如果要使提示仅在被摇杆光标选中时显示,请关联该引用,否则置空即可")]
		[SerializeField]
		private GamepadButtonCursor bindingCursor;
	}
}
