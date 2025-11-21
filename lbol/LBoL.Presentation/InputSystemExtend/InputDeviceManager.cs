using System;
using System.Collections;
using LBoL.Core;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace LBoL.Presentation.InputSystemExtend
{
	// Token: 0x020000F0 RID: 240
	public class InputDeviceManager : Singleton<InputDeviceManager>
	{
		// Token: 0x17000247 RID: 583
		// (get) Token: 0x06000DBE RID: 3518 RVA: 0x000422F4 File Offset: 0x000404F4
		// (set) Token: 0x06000DBF RID: 3519 RVA: 0x000422FC File Offset: 0x000404FC
		public static bool GamepadEnabled
		{
			get
			{
				return InputDeviceManager._gamepadEnabled;
			}
			set
			{
				if (!value)
				{
					using (ReadOnlyArray<Gamepad>.Enumerator enumerator = Gamepad.all.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Gamepad gamepad = enumerator.Current;
							InputSystem.DisableDevice(gamepad, false);
						}
						goto IL_006E;
					}
				}
				foreach (Gamepad gamepad2 in Gamepad.all)
				{
					InputSystem.EnableDevice(gamepad2);
				}
				IL_006E:
				InputDeviceManager._gamepadEnabled = value;
			}
		}

		// Token: 0x17000248 RID: 584
		// (get) Token: 0x06000DC0 RID: 3520 RVA: 0x0004239C File Offset: 0x0004059C
		// (set) Token: 0x06000DC1 RID: 3521 RVA: 0x000423A4 File Offset: 0x000405A4
		public InputDeviceType CurrentInputDevice { get; private set; }

		// Token: 0x06000DC2 RID: 3522 RVA: 0x000423AD File Offset: 0x000405AD
		private void Awake()
		{
			InputDeviceManager.GamepadEnabled = true;
		}

		// Token: 0x06000DC3 RID: 3523 RVA: 0x000423B5 File Offset: 0x000405B5
		private void OnEnable()
		{
			InputSystem.onActionChange += new Action<object, InputActionChange>(this.UpdateCurrentInputDevice);
		}

		// Token: 0x06000DC4 RID: 3524 RVA: 0x000423C8 File Offset: 0x000405C8
		private void OnDisable()
		{
			InputSystem.onActionChange -= new Action<object, InputActionChange>(this.UpdateCurrentInputDevice);
		}

		// Token: 0x06000DC5 RID: 3525 RVA: 0x000423DC File Offset: 0x000405DC
		private void UpdateCurrentInputDevice(object obj, InputActionChange change)
		{
			EventSystem.current.currentSelectedGameObject != null;
			if (change == InputActionChange.ActionPerformed)
			{
				InputAction inputAction = obj as InputAction;
				InputDeviceType deviceType = this.GetDeviceType(inputAction.activeControl.device);
				if (this.disableMouseInput && deviceType == InputDeviceType.MouseAndKeyboard)
				{
					return;
				}
				if (deviceType == this.CurrentInputDevice)
				{
					return;
				}
				Cursor.visible = deviceType == InputDeviceType.MouseAndKeyboard;
				if (deviceType == InputDeviceType.Gamepad && this.CurrentInputDevice == InputDeviceType.MouseAndKeyboard)
				{
					this.disableMouseInput = true;
					Mouse.current.WarpCursorPosition(Vector2.zero);
					base.StartCoroutine(this.EnableMouseInputCorutine());
				}
				this.CurrentInputDevice = deviceType;
				UnityEvent<InputDeviceType> onInputDeviceChanged = this.OnInputDeviceChanged;
				if (onInputDeviceChanged != null)
				{
					onInputDeviceChanged.Invoke(this.CurrentInputDevice);
				}
				if (!this._messageSent && this.CurrentInputDevice == InputDeviceType.Gamepad)
				{
					MessageDialog dialog = UiManager.GetDialog<MessageDialog>();
					if (!dialog.IsVisible)
					{
						this._messageSent = true;
						dialog.Show(new MessageContent
						{
							TextKey = "GamepadUnfinishedText",
							SubTextKey = "GamepadUnfinishedSubText",
							Buttons = DialogButtons.Confirm
						});
					}
				}
			}
		}

		// Token: 0x06000DC6 RID: 3526 RVA: 0x000424D6 File Offset: 0x000406D6
		private InputDeviceType GetDeviceType(InputDevice device)
		{
			if (device is Mouse || device is Keyboard)
			{
				return InputDeviceType.MouseAndKeyboard;
			}
			if (device is Gamepad)
			{
				return InputDeviceType.Gamepad;
			}
			return InputDeviceType.None;
		}

		// Token: 0x06000DC7 RID: 3527 RVA: 0x000424F5 File Offset: 0x000406F5
		private IEnumerator EnableMouseInputCorutine()
		{
			yield return null;
			yield return null;
			this.disableMouseInput = false;
			yield break;
		}

		// Token: 0x04000A55 RID: 2645
		private static bool _gamepadEnabled;

		// Token: 0x04000A57 RID: 2647
		public UnityEvent<InputDeviceType> OnInputDeviceChanged;

		// Token: 0x04000A58 RID: 2648
		private bool disableMouseInput;

		// Token: 0x04000A59 RID: 2649
		private bool _messageSent;
	}
}
