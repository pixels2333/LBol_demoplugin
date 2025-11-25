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
	public class InputDeviceManager : Singleton<InputDeviceManager>
	{
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
		public InputDeviceType CurrentInputDevice { get; private set; }
		private void Awake()
		{
			InputDeviceManager.GamepadEnabled = true;
		}
		private void OnEnable()
		{
			InputSystem.onActionChange += new Action<object, InputActionChange>(this.UpdateCurrentInputDevice);
		}
		private void OnDisable()
		{
			InputSystem.onActionChange -= new Action<object, InputActionChange>(this.UpdateCurrentInputDevice);
		}
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
		private IEnumerator EnableMouseInputCorutine()
		{
			yield return null;
			yield return null;
			this.disableMouseInput = false;
			yield break;
		}
		private static bool _gamepadEnabled;
		public UnityEvent<InputDeviceType> OnInputDeviceChanged;
		private bool disableMouseInput;
		private bool _messageSent;
	}
}
