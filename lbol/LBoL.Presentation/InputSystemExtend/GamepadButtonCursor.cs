using System;
using LBoL.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace LBoL.Presentation.InputSystemExtend
{
	[RequireComponent(typeof(Selectable))]
	public class GamepadButtonCursor : GamepadBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler, ISubmitHandler
	{
		public bool IsSelected { get; private set; }
		public UnityEvent<bool> OnSelectChanged
		{
			get
			{
				return this.onSelectChanged;
			}
		}
		public UnityEvent OnClick
		{
			get
			{
				return this.onClick;
			}
		}
		protected void Awake()
		{
			this.OnDeselect(null);
		}
		protected override void OnDisable()
		{
			base.OnDisable();
			this.OnDeselect(null);
		}
		public virtual void OnDeselect(BaseEventData eventData)
		{
			this.IsSelected = false;
			if (this.cursor != null)
			{
				this.cursor.SetActive(false);
			}
			UnityEvent<bool> unityEvent = this.onSelectChanged;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke(false);
		}
		public virtual void OnSelect(BaseEventData eventData)
		{
			if (Singleton<InputDeviceManager>.Instance.CurrentInputDevice == InputDeviceType.Gamepad)
			{
				this.IsSelected = true;
				if (this.cursor != null)
				{
					this.cursor.SetActive(true);
				}
				UnityEvent<bool> unityEvent = this.onSelectChanged;
				if (unityEvent == null)
				{
					return;
				}
				unityEvent.Invoke(true);
			}
		}
		public virtual void OnSubmit(BaseEventData eventData)
		{
			if (Singleton<InputDeviceManager>.Instance.CurrentInputDevice == InputDeviceType.Gamepad)
			{
				UnityEvent unityEvent = this.onClick;
				if (unityEvent == null)
				{
					return;
				}
				unityEvent.Invoke();
			}
		}
		protected override void OnInputDeviceChanged(InputDeviceType inputDevice)
		{
			if (inputDevice != InputDeviceType.Gamepad)
			{
				this.OnDeselect(null);
			}
		}
		[SerializeField]
		private GameObject cursor;
		[SerializeField]
		private UnityEvent<bool> onSelectChanged;
		[SerializeField]
		protected UnityEvent onClick;
	}
}
