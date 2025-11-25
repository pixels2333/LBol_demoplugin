using System;
using LBoL.Core;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace LBoL.Presentation.InputSystemExtend
{
	public class GamepadCardCursor : GamepadButtonCursor
	{
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
		public override void OnSubmit(BaseEventData eventData)
		{
			base.OnSubmit(eventData);
			if (Singleton<InputDeviceManager>.Instance.CurrentInputDevice == InputDeviceType.Gamepad)
			{
				ExecuteEvents.Execute<IPointerClickHandler>(base.transform.parent.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
				ExecuteEvents.Execute<ISubmitHandler>(base.transform.parent.gameObject, eventData, ExecuteEvents.submitHandler);
			}
		}
		public override void OnSelect(BaseEventData eventData)
		{
			base.OnSelect(eventData);
			if (Singleton<InputDeviceManager>.Instance.CurrentInputDevice == InputDeviceType.Gamepad)
			{
				ExecuteEvents.Execute<ISelectHandler>(base.transform.parent.gameObject, eventData, ExecuteEvents.selectHandler);
			}
		}
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
		protected override void OnInputDeviceChanged(InputDeviceType inputDevice)
		{
			base.OnInputDeviceChanged(inputDevice);
		}
	}
}
