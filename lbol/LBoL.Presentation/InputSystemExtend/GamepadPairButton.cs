using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace LBoL.Presentation.InputSystemExtend
{
	// Token: 0x020000EC RID: 236
	public class GamepadPairButton : GamepadBehaviour
	{
		// Token: 0x06000DA9 RID: 3497 RVA: 0x00041C4C File Offset: 0x0003FE4C
		protected override void OnEnable()
		{
			base.OnEnable();
			if (InputSystem.actions == null)
			{
				return;
			}
			this.curActionLeft = InputSystem.actions.FindAction(GamepadButton.GetButtonActionName(this.buttonLeft), false);
			this.curActionRight = InputSystem.actions.FindAction(GamepadButton.GetButtonActionName(this.buttonRight), false);
			if (this.pressType == GamepadButtonPressType.Press)
			{
				this.curActionLeft.performed += new Action<InputAction.CallbackContext>(this.OnLeftActionPerformed);
				this.curActionRight.performed += new Action<InputAction.CallbackContext>(this.OnRightActionPerformed);
				return;
			}
			if (this.pressType == GamepadButtonPressType.Hold)
			{
				this.curActionLeft.started += new Action<InputAction.CallbackContext>(this.OnLeftActionStarted);
				this.curActionLeft.canceled += new Action<InputAction.CallbackContext>(this.OnLeftActionCanceled);
				this.curActionRight.started += new Action<InputAction.CallbackContext>(this.OnRightActionStarted);
				this.curActionRight.canceled += new Action<InputAction.CallbackContext>(this.OnRightActionCanceled);
			}
		}

		// Token: 0x06000DAA RID: 3498 RVA: 0x00041D44 File Offset: 0x0003FF44
		protected override void OnDisable()
		{
			base.OnDisable();
			if (this.leftActionCoroutine != null)
			{
				base.StopCoroutine(this.leftActionCoroutine);
				this.leftActionCoroutine = null;
			}
			if (this.rightActionCoroutine != null)
			{
				base.StopCoroutine(this.rightActionCoroutine);
				this.rightActionCoroutine = null;
			}
			this.curActionLeft.started -= new Action<InputAction.CallbackContext>(this.OnLeftActionStarted);
			this.curActionLeft.canceled -= new Action<InputAction.CallbackContext>(this.OnLeftActionCanceled);
			this.curActionLeft.performed -= new Action<InputAction.CallbackContext>(this.OnLeftActionPerformed);
			this.curActionLeft = null;
			this.curActionRight.performed -= new Action<InputAction.CallbackContext>(this.OnRightActionPerformed);
			this.curActionRight.started -= new Action<InputAction.CallbackContext>(this.OnRightActionStarted);
			this.curActionRight.canceled -= new Action<InputAction.CallbackContext>(this.OnRightActionCanceled);
			this.curActionRight = null;
		}

		// Token: 0x06000DAB RID: 3499 RVA: 0x00041E25 File Offset: 0x00040025
		private void OnLeftActionStarted(InputAction.CallbackContext obj)
		{
			this.leftActionCoroutine = base.StartCoroutine(this.HoldLeftActionCoroutine(obj));
		}

		// Token: 0x06000DAC RID: 3500 RVA: 0x00041E3A File Offset: 0x0004003A
		private void OnLeftActionCanceled(InputAction.CallbackContext obj)
		{
			if (this.leftActionCoroutine == null)
			{
				return;
			}
			base.StopCoroutine(this.leftActionCoroutine);
			this.leftActionCoroutine = null;
		}

		// Token: 0x06000DAD RID: 3501 RVA: 0x00041E58 File Offset: 0x00040058
		private IEnumerator HoldLeftActionCoroutine(InputAction.CallbackContext obj)
		{
			while (base.gameObject.activeInHierarchy)
			{
				this.OnLeftActionPerformed(obj);
				yield return new WaitForSeconds(this.holdInterval);
			}
			yield break;
		}

		// Token: 0x06000DAE RID: 3502 RVA: 0x00041E70 File Offset: 0x00040070
		private void OnLeftActionPerformed(InputAction.CallbackContext obj)
		{
			if (this.bindingPanel && !base.IsParentPanelAvailable())
			{
				return;
			}
			if (this.bindingCursor != null && !this.bindingCursor.IsSelected)
			{
				return;
			}
			this.onValueChanged.Invoke(-this.valueStep);
		}

		// Token: 0x06000DAF RID: 3503 RVA: 0x00041EBC File Offset: 0x000400BC
		private void OnRightActionStarted(InputAction.CallbackContext obj)
		{
			this.rightActionCoroutine = base.StartCoroutine(this.HoldRightActionCoroutine(obj));
		}

		// Token: 0x06000DB0 RID: 3504 RVA: 0x00041ED1 File Offset: 0x000400D1
		private void OnRightActionCanceled(InputAction.CallbackContext obj)
		{
			if (this.rightActionCoroutine == null)
			{
				return;
			}
			base.StopCoroutine(this.rightActionCoroutine);
			this.rightActionCoroutine = null;
		}

		// Token: 0x06000DB1 RID: 3505 RVA: 0x00041EEF File Offset: 0x000400EF
		private IEnumerator HoldRightActionCoroutine(InputAction.CallbackContext obj)
		{
			while (base.gameObject.activeInHierarchy)
			{
				this.OnRightActionPerformed(obj);
				yield return new WaitForSeconds(this.holdInterval);
			}
			yield break;
		}

		// Token: 0x06000DB2 RID: 3506 RVA: 0x00041F05 File Offset: 0x00040105
		private void OnRightActionPerformed(InputAction.CallbackContext obj)
		{
			if (this.bindingPanel && !base.IsParentPanelAvailable())
			{
				return;
			}
			if (this.bindingCursor != null && !this.bindingCursor.IsSelected)
			{
				return;
			}
			this.onValueChanged.Invoke(this.valueStep);
		}

		// Token: 0x04000A45 RID: 2629
		[SerializeField]
		private GamepadButtonCursor bindingCursor;

		// Token: 0x04000A46 RID: 2630
		[SerializeField]
		private GamepadButtonKey buttonLeft;

		// Token: 0x04000A47 RID: 2631
		[SerializeField]
		private GamepadButtonKey buttonRight;

		// Token: 0x04000A48 RID: 2632
		[SerializeField]
		private float valueStep = 1f;

		// Token: 0x04000A49 RID: 2633
		[SerializeField]
		private GamepadButtonPressType pressType;

		// Token: 0x04000A4A RID: 2634
		[SerializeField]
		private float holdInterval = 0.1f;

		// Token: 0x04000A4B RID: 2635
		[SerializeField]
		private UnityEvent<float> onValueChanged;

		// Token: 0x04000A4C RID: 2636
		private InputAction curActionLeft;

		// Token: 0x04000A4D RID: 2637
		private InputAction curActionRight;

		// Token: 0x04000A4E RID: 2638
		private Coroutine leftActionCoroutine;

		// Token: 0x04000A4F RID: 2639
		private Coroutine rightActionCoroutine;
	}
}
