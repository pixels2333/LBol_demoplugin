using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class SwitchWidget : CommonButtonWidget
	{
		public bool IsOn
		{
			get
			{
				return this._isOn;
			}
			set
			{
				this._isOn = value;
				this.ValueChangedAnim(false);
				this.onToggleChanged.Invoke(this.IsOn);
			}
		}
		public bool IsLocked { get; set; }
		public void SetValueWithoutNotifier(bool value, bool instant = true)
		{
			this._isOn = value;
			this.ValueChangedAnim(instant);
		}
		public override void OnPointerEnter(PointerEventData eventData)
		{
			if (base.gameObject.activeInHierarchy && !this.IsLocked)
			{
				AudioManager.Button(5);
			}
		}
		public override void OnPointerClick(PointerEventData eventData)
		{
			base.OnPointerClick(eventData);
			if (this.IsLocked)
			{
				return;
			}
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				this.IsOn = !this.IsOn;
				AudioManager.Button(this._isOn ? 3 : 4);
			}
		}
		private void Awake()
		{
			this._sizeX = base.GetComponent<RectTransform>().sizeDelta.x;
			this._distance = Mathf.Max(this._sizeX / 4f, this._distance);
		}
		private static Color ActiveColor
		{
			get
			{
				return Color.white;
			}
		}
		private static Color InactiveColor
		{
			get
			{
				return new Color(0.2f, 0.2f, 0.2f);
			}
		}
		private void ValueChangedAnim(bool instant = false)
		{
			this.DOKill(false);
			if (this._sizeX == 0f)
			{
				this._sizeX = base.GetComponent<RectTransform>().sizeDelta.x;
				this._distance = Mathf.Max(this._sizeX / 4f, this._distance);
			}
			if (instant)
			{
				this.textOn.alpha = (float)(this._isOn ? 1 : 0);
				this.textOff.alpha = (float)(this._isOn ? 0 : 1);
				this.textOn.rectTransform.localPosition = Vector3.zero;
				this.textOff.rectTransform.localPosition = Vector3.zero;
				this.mask2D.padding = (this._isOn ? new Vector4(0f, 0f, 0f, 0f) : new Vector4(0f, 0f, this._sizeX, 0f));
				return;
			}
			if (this._isOn)
			{
				DOTween.To(() => this.textOn.alpha, new DOSetter<float>(this.SetTween), 1f, this.animTime).SetUpdate(true).SetTarget(this);
				return;
			}
			DOTween.To(() => this.textOn.alpha, new DOSetter<float>(this.SetTween), 0f, this.animTime).SetUpdate(true).SetTarget(this);
		}
		private void SetTween(float x)
		{
			this.textOn.alpha = x;
			this.textOff.alpha = 1f - x;
			this.textOn.rectTransform.localPosition = new Vector3((1f - x) * this._distance, 0f, 0f);
			this.textOff.rectTransform.localPosition = new Vector3(-x * this._distance, 0f, 0f);
			this.mask2D.padding = new Vector4(0f, 0f, this._sizeX * (1f - x), 0f);
		}
		public void AddListener(UnityAction<bool> action)
		{
			this.onToggleChanged.AddListener(action);
		}
		[SerializeField]
		private TextMeshProUGUI textOn;
		[SerializeField]
		private TextMeshProUGUI textOff;
		[SerializeField]
		private RectMask2D mask2D;
		[SerializeField]
		private float animTime = 0.2f;
		[SerializeField]
		private UnityEvent<bool> onToggleChanged;
		private bool _isOn;
		private float _distance = 100f;
		private const float FillRatio = 0.2f;
		private float _sizeX;
	}
}
