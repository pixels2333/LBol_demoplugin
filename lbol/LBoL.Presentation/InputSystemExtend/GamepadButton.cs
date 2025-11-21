using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LBoL.Presentation.InputSystemExtend
{
	// Token: 0x020000E3 RID: 227
	public class GamepadButton : GamepadBehaviour
	{
		// Token: 0x17000242 RID: 578
		// (get) Token: 0x06000D76 RID: 3446 RVA: 0x00041386 File Offset: 0x0003F586
		public GamepadButtonKey Button
		{
			get
			{
				return this.button;
			}
		}

		// Token: 0x17000243 RID: 579
		// (get) Token: 0x06000D77 RID: 3447 RVA: 0x0004138E File Offset: 0x0003F58E
		public UnityEvent OnClick
		{
			get
			{
				return this.onClick;
			}
		}

		// Token: 0x06000D78 RID: 3448 RVA: 0x00041398 File Offset: 0x0003F598
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

		// Token: 0x06000D79 RID: 3449 RVA: 0x0004145E File Offset: 0x0003F65E
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

		// Token: 0x06000D7A RID: 3450 RVA: 0x00041490 File Offset: 0x0003F690
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

		// Token: 0x06000D7B RID: 3451 RVA: 0x00041523 File Offset: 0x0003F723
		public static string GetButtonActionName(GamepadButtonKey buttonType)
		{
			return "Gamepad" + buttonType.ToString();
		}

		// Token: 0x04000A26 RID: 2598
		[SerializeField]
		private GamepadButtonCursor bindingCursor;

		// Token: 0x04000A27 RID: 2599
		[SerializeField]
		private Button bindButton;

		// Token: 0x04000A28 RID: 2600
		[Tooltip("不允许直接绑定A键与B键回调，它们是通用的确认键与返回键")]
		[SerializeField]
		private GamepadButtonKey button;

		// Token: 0x04000A29 RID: 2601
		[SerializeField]
		private UnityEvent onClick;

		// Token: 0x04000A2A RID: 2602
		private InputAction curAction;
	}
}
