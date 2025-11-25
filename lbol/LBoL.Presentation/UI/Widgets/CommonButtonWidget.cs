using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Core;
using LBoL.Presentation.InputSystemExtend;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class CommonButtonWidget : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
	{
		private bool PlaySound
		{
			get
			{
				return this.forcePlaySound || (this.button && this.button.interactable);
			}
		}
		private void OnEnable()
		{
			Singleton<InputDeviceManager>.Instance.OnInputDeviceChanged.AddListener(new UnityAction<InputDeviceType>(this.OnInputDeviceChanged));
		}
		private void OnDisable()
		{
			InputDeviceManager instance = Singleton<InputDeviceManager>.Instance;
			if (instance == null)
			{
				return;
			}
			instance.OnInputDeviceChanged.RemoveListener(new UnityAction<InputDeviceType>(this.OnInputDeviceChanged));
		}
		public virtual void OnInputDeviceChanged(InputDeviceType inputDevice)
		{
			this.OnPointerExit(null);
		}
		public virtual void OnPointerEnter(PointerEventData eventData)
		{
			if (base.gameObject.activeInHierarchy)
			{
				if (!this._setButtonOnce)
				{
					this._setButtonOnce = true;
					if (!this.button)
					{
						this.button = base.GetComponent<Button>();
					}
				}
				if (this.PlaySound)
				{
					CommonButtonWidget.ButtonWeight buttonWeight = this.buttonWeight;
					if (buttonWeight != CommonButtonWidget.ButtonWeight.Normal)
					{
						if (buttonWeight != CommonButtonWidget.ButtonWeight.Light)
						{
							throw new ArgumentOutOfRangeException();
						}
						AudioManager.Button(5);
					}
					else
					{
						AudioManager.Button(2);
					}
				}
			}
			if (this.hoverScale > 1f)
			{
				this.root.DOKill(false);
				this.root.DOScale(this.hoverScale, 0.1f).SetAutoKill<TweenerCore<Vector3, Vector3, VectorOptions>>().SetUpdate(true);
			}
		}
		public virtual void OnPointerExit(PointerEventData eventData)
		{
			if (this.hoverScale > 1f)
			{
				this.root.DOKill(false);
				this.root.DOScale(1f, 0.1f).SetAutoKill<TweenerCore<Vector3, Vector3, VectorOptions>>().SetUpdate(true);
			}
		}
		public virtual void OnPointerClick(PointerEventData eventData)
		{
			if (this.PlaySound)
			{
				switch (this.buttonWeight)
				{
				case CommonButtonWidget.ButtonWeight.Normal:
				{
					CommonButtonWidget.ButtonBehavior buttonBehavior = this.buttonBehavior;
					if (buttonBehavior == CommonButtonWidget.ButtonBehavior.Open)
					{
						AudioManager.Button(0);
						return;
					}
					if (buttonBehavior != CommonButtonWidget.ButtonBehavior.Close)
					{
						throw new ArgumentOutOfRangeException();
					}
					AudioManager.Button(1);
					return;
				}
				case CommonButtonWidget.ButtonWeight.Light:
				{
					CommonButtonWidget.ButtonBehavior buttonBehavior = this.buttonBehavior;
					if (buttonBehavior == CommonButtonWidget.ButtonBehavior.Open)
					{
						AudioManager.Button(3);
						return;
					}
					if (buttonBehavior != CommonButtonWidget.ButtonBehavior.Close)
					{
						throw new ArgumentOutOfRangeException();
					}
					AudioManager.Button(4);
					return;
				}
				case CommonButtonWidget.ButtonWeight.NoSound:
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}
		public virtual void OnSelect(BaseEventData eventData)
		{
			if (Singleton<InputDeviceManager>.Instance.CurrentInputDevice == InputDeviceType.Gamepad)
			{
				this.OnPointerEnter(null);
			}
		}
		public virtual void OnDeselect(BaseEventData eventData)
		{
			if (Singleton<InputDeviceManager>.Instance.CurrentInputDevice == InputDeviceType.Gamepad)
			{
				this.OnPointerExit(null);
			}
		}
		public virtual void OnSubmit(BaseEventData eventData)
		{
			if (Singleton<InputDeviceManager>.Instance.CurrentInputDevice == InputDeviceType.Gamepad)
			{
				this.OnPointerClick(new PointerEventData(EventSystem.current)
				{
					button = PointerEventData.InputButton.Left
				});
			}
		}
		[SerializeField]
		private bool forcePlaySound;
		[SerializeField]
		protected CommonButtonWidget.ButtonBehavior buttonBehavior;
		[SerializeField]
		protected CommonButtonWidget.ButtonWeight buttonWeight;
		[SerializeField]
		public Button button;
		[SerializeField]
		[Range(1f, 1.2f)]
		protected float hoverScale = 1f;
		[SerializeField]
		protected Transform root;
		private bool _setButtonOnce;
		protected enum ButtonBehavior
		{
			Open,
			Close
		}
		protected enum ButtonWeight
		{
			Normal,
			Light,
			NoSound
		}
	}
}
