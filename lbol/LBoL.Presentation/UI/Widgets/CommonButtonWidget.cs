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
	// Token: 0x0200004C RID: 76
	public class CommonButtonWidget : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
	{
		// Token: 0x170000C9 RID: 201
		// (get) Token: 0x06000494 RID: 1172 RVA: 0x00012ECD File Offset: 0x000110CD
		private bool PlaySound
		{
			get
			{
				return this.forcePlaySound || (this.button && this.button.interactable);
			}
		}

		// Token: 0x06000495 RID: 1173 RVA: 0x00012EF3 File Offset: 0x000110F3
		private void OnEnable()
		{
			Singleton<InputDeviceManager>.Instance.OnInputDeviceChanged.AddListener(new UnityAction<InputDeviceType>(this.OnInputDeviceChanged));
		}

		// Token: 0x06000496 RID: 1174 RVA: 0x00012F11 File Offset: 0x00011111
		private void OnDisable()
		{
			InputDeviceManager instance = Singleton<InputDeviceManager>.Instance;
			if (instance == null)
			{
				return;
			}
			instance.OnInputDeviceChanged.RemoveListener(new UnityAction<InputDeviceType>(this.OnInputDeviceChanged));
		}

		// Token: 0x06000497 RID: 1175 RVA: 0x00012F34 File Offset: 0x00011134
		public virtual void OnInputDeviceChanged(InputDeviceType inputDevice)
		{
			this.OnPointerExit(null);
		}

		// Token: 0x06000498 RID: 1176 RVA: 0x00012F40 File Offset: 0x00011140
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

		// Token: 0x06000499 RID: 1177 RVA: 0x00012FEC File Offset: 0x000111EC
		public virtual void OnPointerExit(PointerEventData eventData)
		{
			if (this.hoverScale > 1f)
			{
				this.root.DOKill(false);
				this.root.DOScale(1f, 0.1f).SetAutoKill<TweenerCore<Vector3, Vector3, VectorOptions>>().SetUpdate(true);
			}
		}

		// Token: 0x0600049A RID: 1178 RVA: 0x0001302C File Offset: 0x0001122C
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

		// Token: 0x0600049B RID: 1179 RVA: 0x000130AA File Offset: 0x000112AA
		public virtual void OnSelect(BaseEventData eventData)
		{
			if (Singleton<InputDeviceManager>.Instance.CurrentInputDevice == InputDeviceType.Gamepad)
			{
				this.OnPointerEnter(null);
			}
		}

		// Token: 0x0600049C RID: 1180 RVA: 0x000130C0 File Offset: 0x000112C0
		public virtual void OnDeselect(BaseEventData eventData)
		{
			if (Singleton<InputDeviceManager>.Instance.CurrentInputDevice == InputDeviceType.Gamepad)
			{
				this.OnPointerExit(null);
			}
		}

		// Token: 0x0600049D RID: 1181 RVA: 0x000130D8 File Offset: 0x000112D8
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

		// Token: 0x04000267 RID: 615
		[SerializeField]
		private bool forcePlaySound;

		// Token: 0x04000268 RID: 616
		[SerializeField]
		protected CommonButtonWidget.ButtonBehavior buttonBehavior;

		// Token: 0x04000269 RID: 617
		[SerializeField]
		protected CommonButtonWidget.ButtonWeight buttonWeight;

		// Token: 0x0400026A RID: 618
		[SerializeField]
		public Button button;

		// Token: 0x0400026B RID: 619
		[SerializeField]
		[Range(1f, 1.2f)]
		protected float hoverScale = 1f;

		// Token: 0x0400026C RID: 620
		[SerializeField]
		protected Transform root;

		// Token: 0x0400026D RID: 621
		private bool _setButtonOnce;

		// Token: 0x020001D0 RID: 464
		protected enum ButtonBehavior
		{
			// Token: 0x04000F02 RID: 3842
			Open,
			// Token: 0x04000F03 RID: 3843
			Close
		}

		// Token: 0x020001D1 RID: 465
		protected enum ButtonWeight
		{
			// Token: 0x04000F05 RID: 3845
			Normal,
			// Token: 0x04000F06 RID: 3846
			Light,
			// Token: 0x04000F07 RID: 3847
			NoSound
		}
	}
}
