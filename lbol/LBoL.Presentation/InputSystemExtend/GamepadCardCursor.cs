using System;
using LBoL.Core;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LBoL.Presentation.InputSystemExtend
{
	// Token: 0x020000E7 RID: 231
	public class GamepadCardCursor : GamepadButtonCursor
	{
		// Token: 0x06000D8E RID: 3470 RVA: 0x00041798 File Offset: 0x0003F998
		protected override void Start()
		{
			base.Start();
			Button button;
			if (base.transform.parent.TryGetComponent<Button>(out button) && button.interactable)
			{
				button.navigation = new Navigation
				{
					mode = Navigation.Mode.None
				};
			}
		}

		// Token: 0x06000D8F RID: 3471 RVA: 0x000417E0 File Offset: 0x0003F9E0
		public override void OnSubmit(BaseEventData eventData)
		{
			base.OnSubmit(eventData);
			if (Singleton<InputDeviceManager>.Instance.CurrentInputDevice == InputDeviceType.Gamepad)
			{
				ExecuteEvents.Execute<IPointerClickHandler>(base.transform.parent.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
				ExecuteEvents.Execute<ISubmitHandler>(base.transform.parent.gameObject, eventData, ExecuteEvents.submitHandler);
			}
		}

		// Token: 0x06000D90 RID: 3472 RVA: 0x00041842 File Offset: 0x0003FA42
		public override void OnSelect(BaseEventData eventData)
		{
			base.OnSelect(eventData);
			if (Singleton<InputDeviceManager>.Instance.CurrentInputDevice == InputDeviceType.Gamepad)
			{
				ExecuteEvents.Execute<ISelectHandler>(base.transform.parent.gameObject, eventData, ExecuteEvents.selectHandler);
			}
		}

		// Token: 0x06000D91 RID: 3473 RVA: 0x00041874 File Offset: 0x0003FA74
		public override void OnDeselect(BaseEventData eventData)
		{
			base.OnDeselect(eventData);
			if (Singleton<InputDeviceManager>.Instance == null)
			{
				return;
			}
			if (Singleton<InputDeviceManager>.Instance.CurrentInputDevice == InputDeviceType.Gamepad)
			{
				ExecuteEvents.Execute<IDeselectHandler>(base.transform.parent.gameObject, eventData, ExecuteEvents.deselectHandler);
			}
		}

		// Token: 0x06000D92 RID: 3474 RVA: 0x000418B4 File Offset: 0x0003FAB4
		protected override void OnInputDeviceChanged(InputDeviceType inputDevice)
		{
			base.OnInputDeviceChanged(inputDevice);
		}
	}
}
