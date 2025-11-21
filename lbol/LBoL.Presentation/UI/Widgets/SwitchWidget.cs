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
	// Token: 0x02000076 RID: 118
	public class SwitchWidget : CommonButtonWidget
	{
		// Token: 0x1700010B RID: 267
		// (get) Token: 0x06000609 RID: 1545 RVA: 0x0001A181 File Offset: 0x00018381
		// (set) Token: 0x0600060A RID: 1546 RVA: 0x0001A189 File Offset: 0x00018389
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

		// Token: 0x1700010C RID: 268
		// (get) Token: 0x0600060B RID: 1547 RVA: 0x0001A1AA File Offset: 0x000183AA
		// (set) Token: 0x0600060C RID: 1548 RVA: 0x0001A1B2 File Offset: 0x000183B2
		public bool IsLocked { get; set; }

		// Token: 0x0600060D RID: 1549 RVA: 0x0001A1BB File Offset: 0x000183BB
		public void SetValueWithoutNotifier(bool value, bool instant = true)
		{
			this._isOn = value;
			this.ValueChangedAnim(instant);
		}

		// Token: 0x0600060E RID: 1550 RVA: 0x0001A1CB File Offset: 0x000183CB
		public override void OnPointerEnter(PointerEventData eventData)
		{
			if (base.gameObject.activeInHierarchy && !this.IsLocked)
			{
				AudioManager.Button(5);
			}
		}

		// Token: 0x0600060F RID: 1551 RVA: 0x0001A1E8 File Offset: 0x000183E8
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

		// Token: 0x06000610 RID: 1552 RVA: 0x0001A222 File Offset: 0x00018422
		private void Awake()
		{
			this._sizeX = base.GetComponent<RectTransform>().sizeDelta.x;
			this._distance = Mathf.Max(this._sizeX / 4f, this._distance);
		}

		// Token: 0x1700010D RID: 269
		// (get) Token: 0x06000611 RID: 1553 RVA: 0x0001A257 File Offset: 0x00018457
		private static Color ActiveColor
		{
			get
			{
				return Color.white;
			}
		}

		// Token: 0x1700010E RID: 270
		// (get) Token: 0x06000612 RID: 1554 RVA: 0x0001A25E File Offset: 0x0001845E
		private static Color InactiveColor
		{
			get
			{
				return new Color(0.2f, 0.2f, 0.2f);
			}
		}

		// Token: 0x06000613 RID: 1555 RVA: 0x0001A274 File Offset: 0x00018474
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

		// Token: 0x06000614 RID: 1556 RVA: 0x0001A3E8 File Offset: 0x000185E8
		private void SetTween(float x)
		{
			this.textOn.alpha = x;
			this.textOff.alpha = 1f - x;
			this.textOn.rectTransform.localPosition = new Vector3((1f - x) * this._distance, 0f, 0f);
			this.textOff.rectTransform.localPosition = new Vector3(-x * this._distance, 0f, 0f);
			this.mask2D.padding = new Vector4(0f, 0f, this._sizeX * (1f - x), 0f);
		}

		// Token: 0x06000615 RID: 1557 RVA: 0x0001A495 File Offset: 0x00018695
		public void AddListener(UnityAction<bool> action)
		{
			this.onToggleChanged.AddListener(action);
		}

		// Token: 0x040003B6 RID: 950
		[SerializeField]
		private TextMeshProUGUI textOn;

		// Token: 0x040003B7 RID: 951
		[SerializeField]
		private TextMeshProUGUI textOff;

		// Token: 0x040003B8 RID: 952
		[SerializeField]
		private RectMask2D mask2D;

		// Token: 0x040003B9 RID: 953
		[SerializeField]
		private float animTime = 0.2f;

		// Token: 0x040003BA RID: 954
		[SerializeField]
		private UnityEvent<bool> onToggleChanged;

		// Token: 0x040003BB RID: 955
		private bool _isOn;

		// Token: 0x040003BD RID: 957
		private float _distance = 100f;

		// Token: 0x040003BE RID: 958
		private const float FillRatio = 0.2f;

		// Token: 0x040003BF RID: 959
		private float _sizeX;
	}
}
