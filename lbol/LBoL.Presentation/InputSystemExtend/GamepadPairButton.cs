using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
namespace LBoL.Presentation.InputSystemExtend
{
	public class GamepadPairButton : GamepadBehaviour
	{
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
		private void OnLeftActionStarted(InputAction.CallbackContext obj)
		{
			this.leftActionCoroutine = base.StartCoroutine(this.HoldLeftActionCoroutine(obj));
		}
		private void OnLeftActionCanceled(InputAction.CallbackContext obj)
		{
			if (this.leftActionCoroutine == null)
			{
				return;
			}
			base.StopCoroutine(this.leftActionCoroutine);
			this.leftActionCoroutine = null;
		}
		private IEnumerator HoldLeftActionCoroutine(InputAction.CallbackContext obj)
		{
			while (base.gameObject.activeInHierarchy)
			{
				this.OnLeftActionPerformed(obj);
				yield return new WaitForSeconds(this.holdInterval);
			}
			yield break;
		}
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
		private void OnRightActionStarted(InputAction.CallbackContext obj)
		{
			this.rightActionCoroutine = base.StartCoroutine(this.HoldRightActionCoroutine(obj));
		}
		private void OnRightActionCanceled(InputAction.CallbackContext obj)
		{
			if (this.rightActionCoroutine == null)
			{
				return;
			}
			base.StopCoroutine(this.rightActionCoroutine);
			this.rightActionCoroutine = null;
		}
		private IEnumerator HoldRightActionCoroutine(InputAction.CallbackContext obj)
		{
			while (base.gameObject.activeInHierarchy)
			{
				this.OnRightActionPerformed(obj);
				yield return new WaitForSeconds(this.holdInterval);
			}
			yield break;
		}
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
		[SerializeField]
		private GamepadButtonCursor bindingCursor;
		[SerializeField]
		private GamepadButtonKey buttonLeft;
		[SerializeField]
		private GamepadButtonKey buttonRight;
		[SerializeField]
		private float valueStep = 1f;
		[SerializeField]
		private GamepadButtonPressType pressType;
		[SerializeField]
		private float holdInterval = 0.1f;
		[SerializeField]
		private UnityEvent<float> onValueChanged;
		private InputAction curActionLeft;
		private InputAction curActionRight;
		private Coroutine leftActionCoroutine;
		private Coroutine rightActionCoroutine;
	}
}
