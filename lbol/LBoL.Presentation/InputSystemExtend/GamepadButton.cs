using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace LBoL.Presentation.InputSystemExtend
{
	public class GamepadButton : GamepadBehaviour
	{
		public GamepadButtonKey Button
		{
			get
			{
				return this.button;
			}
		}
		public UnityEvent OnClick
		{
			get
			{
				return this.onClick;
			}
		}
		protected override void OnEnable()
		{
			base.OnEnable();
			if (InputSystem.actions == null)
			{
				return;
			}
			if (this.button == GamepadButtonKey.None)
			{
				Debug.LogWarning("invalid gamepad key in: " + base.gameObject.name);
				return;
			}
			if (this.button == GamepadButtonKey.A || this.button == GamepadButtonKey.B)
			{
				Debug.LogWarning("Can not bind GamepadA and GamepadB in GamepadButton at: " + base.gameObject.name);
				return;
			}
			this.curAction = InputSystem.actions.FindAction(GamepadButton.GetButtonActionName(this.button), false);
			if (this.curAction == null)
			{
				Debug.LogWarning("no action found for button : " + GamepadButton.GetButtonActionName(this.button));
			}
			this.curAction.performed += new Action<InputAction.CallbackContext>(this.OnActionPerformed);
		}
		protected override void OnDisable()
		{
			base.OnDisable();
			if (this.curAction == null)
			{
				return;
			}
			this.curAction.performed -= new Action<InputAction.CallbackContext>(this.OnActionPerformed);
			this.curAction = null;
		}
		private void OnActionPerformed(InputAction.CallbackContext obj)
		{
			if (this.bindingPanel && !base.IsParentPanelAvailable())
			{
				return;
			}
			if (this.bindingCursor != null && !this.bindingCursor.IsSelected)
			{
				return;
			}
			if (this.bindButton != null && this.bindButton.interactable)
			{
				CanvasGroup componentInParent = this.bindButton.GetComponentInParent<CanvasGroup>();
				if (componentInParent != null && !componentInParent.interactable)
				{
					return;
				}
				this.bindButton.onClick.Invoke();
			}
			UnityEvent unityEvent = this.onClick;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
		}
		public static string GetButtonActionName(GamepadButtonKey buttonType)
		{
			return "Gamepad" + buttonType.ToString();
		}
		[SerializeField]
		private GamepadButtonCursor bindingCursor;
		[SerializeField]
		private Button bindButton;
		[Tooltip("不允许直接绑定A键与B键回调，它们是通用的确认键与返回键")]
		[SerializeField]
		private GamepadButtonKey button;
		[SerializeField]
		private UnityEvent onClick;
		private InputAction curAction;
	}
}
