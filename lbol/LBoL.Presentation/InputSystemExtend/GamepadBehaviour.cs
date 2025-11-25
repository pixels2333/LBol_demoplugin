using System;
using LBoL.Core;
using UnityEngine;
using UnityEngine.Events;
namespace LBoL.Presentation.InputSystemExtend
{
	public class GamepadBehaviour : MonoBehaviour
	{
		protected virtual void Start()
		{
			if (this.bindingPanel && string.IsNullOrEmpty(this.parentPanelName))
			{
				this.FindBindingPanel();
			}
		}
		protected virtual void OnEnable()
		{
			if (this.bindingPanel && string.IsNullOrEmpty(this.parentPanelName))
			{
				this.FindBindingPanel();
			}
			Singleton<InputDeviceManager>.Instance.OnInputDeviceChanged.AddListener(new UnityAction<InputDeviceType>(this.OnInputDeviceChanged));
			this.OnInputDeviceChanged(Singleton<InputDeviceManager>.Instance.CurrentInputDevice);
		}
		protected virtual void OnDisable()
		{
			InputDeviceManager instance = Singleton<InputDeviceManager>.Instance;
			if (instance == null)
			{
				return;
			}
			instance.OnInputDeviceChanged.RemoveListener(new UnityAction<InputDeviceType>(this.OnInputDeviceChanged));
		}
		protected virtual void OnInputDeviceChanged(InputDeviceType inputDevice)
		{
		}
		public bool IsParentPanelAvailable()
		{
			return this.parentPanelName == Singleton<GamepadNavigationManager>.Instance.GetTopPanel();
		}
		protected void FindBindingPanel()
		{
			IInteractablePanel componentInParent = base.GetComponentInParent<IInteractablePanel>(true);
			if (componentInParent != null)
			{
				this.parentPanelName = componentInParent.GetPanelName();
				return;
			}
			this.parentPanelName = "";
			this.bindingPanel = false;
		}
		[SerializeField]
		protected bool bindingPanel = true;
		protected string parentPanelName;
	}
}
