using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base.Extensions;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace LBoL.Presentation.UI.ExtraWidgets
{
	// Token: 0x020000D4 RID: 212
	[RequireComponent(typeof(CardWidget))]
	public class ShowingCardRelative : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
	{
		// Token: 0x17000218 RID: 536
		// (get) Token: 0x06000CDF RID: 3295 RVA: 0x00040303 File Offset: 0x0003E503
		private RectTransform RectTransform
		{
			get
			{
				return (RectTransform)base.transform;
			}
		}

		// Token: 0x17000219 RID: 537
		// (get) Token: 0x06000CE0 RID: 3296 RVA: 0x00040310 File Offset: 0x0003E510
		// (set) Token: 0x06000CE1 RID: 3297 RVA: 0x00040318 File Offset: 0x0003E518
		private float NormalScale { get; set; }

		// Token: 0x1700021A RID: 538
		// (get) Token: 0x06000CE2 RID: 3298 RVA: 0x00040321 File Offset: 0x0003E521
		// (set) Token: 0x06000CE3 RID: 3299 RVA: 0x00040329 File Offset: 0x0003E529
		private float HoverScale { get; set; }

		// Token: 0x1700021B RID: 539
		// (get) Token: 0x06000CE4 RID: 3300 RVA: 0x00040332 File Offset: 0x0003E532
		// (set) Token: 0x06000CE5 RID: 3301 RVA: 0x0004033A File Offset: 0x0003E53A
		private bool Hovering { get; set; }

		// Token: 0x06000CE6 RID: 3302 RVA: 0x00040343 File Offset: 0x0003E543
		private void Awake()
		{
			this.SetScale(1f, 1.2f);
		}

		// Token: 0x06000CE7 RID: 3303 RVA: 0x00040358 File Offset: 0x0003E558
		private void Update()
		{
			if (base.gameObject.activeSelf && this.tiltWhenHover && this.Hovering)
			{
				if (Mouse.current == null)
				{
					return;
				}
				float num;
				float num2;
				Mouse.current.position.ReadValue().Deconstruct(out num, out num2);
				float num3 = num;
				float num4 = num2;
				int width = Screen.width;
				if (Math.Abs(width - 3840) > 1)
				{
					this._screenScale = 3840f / (float)width;
				}
				else
				{
					this._screenScale = 1f;
				}
				this._pointerX = num3 * this._screenScale;
				this._pointerY = num4 * this._screenScale;
				Vector3 vector = CameraController.UiCamera.WorldToScreenPoint(this.RectTransform.position) * this._screenScale;
				Vector2 vector2 = new Vector2(this._pointerX - vector.x, this._pointerY - vector.y);
				Quaternion quaternion = Quaternion.AngleAxis(Mathf.Clamp01(vector2.magnitude / (455f * this.HoverScale)) * 8f, new Vector3(vector2.y, -vector2.x));
				this.RectTransform.localRotation = quaternion * Quaternion.identity;
			}
		}

		// Token: 0x06000CE8 RID: 3304 RVA: 0x00040490 File Offset: 0x0003E690
		public void OnPointerEnter(PointerEventData eventData)
		{
			this.StartHover();
		}

		// Token: 0x06000CE9 RID: 3305 RVA: 0x00040498 File Offset: 0x0003E698
		public void OnPointerExit(PointerEventData eventData)
		{
			this.EndHover();
		}

		// Token: 0x06000CEA RID: 3306 RVA: 0x000404A0 File Offset: 0x0003E6A0
		private void StartHover()
		{
			this.Hovering = true;
			UiManager.HoveringRightClickInteractionElements = true;
			if (Math.Abs(this.NormalScale - this.HoverScale) > 0.01f)
			{
				base.transform.DOScale(this.HoverScale, 0.12f).SetUpdate(true).SetEase(Ease.OutCubic);
			}
			AudioManager.Card(0);
		}

		// Token: 0x06000CEB RID: 3307 RVA: 0x00040500 File Offset: 0x0003E700
		private void EndHover()
		{
			this.Hovering = false;
			UiManager.HoveringRightClickInteractionElements = false;
			if (Math.Abs(this.NormalScale - this.HoverScale) > 0.01f)
			{
				base.transform.DOScale(this.NormalScale, 0.12f).SetUpdate(true).SetEase(Ease.OutCubic);
			}
			if (this.tiltWhenHover)
			{
				this.RectTransform.DOLocalRotateQuaternion(Quaternion.identity, 0.12f).SetUpdate(true).SetEase(Ease.OutCubic);
			}
		}

		// Token: 0x06000CEC RID: 3308 RVA: 0x00040582 File Offset: 0x0003E782
		public void OnPointerClick(PointerEventData eventData)
		{
			this.EndHover();
			this._clickEvent();
		}

		// Token: 0x06000CED RID: 3309 RVA: 0x00040595 File Offset: 0x0003E795
		public void SetScale(float normal, float hover)
		{
			this.NormalScale = normal;
			this.HoverScale = hover;
			base.transform.localScale = new Vector3(normal, normal, 1f);
		}

		// Token: 0x06000CEE RID: 3310 RVA: 0x000405BC File Offset: 0x0003E7BC
		public void AddListener(UnityAction call)
		{
			this._clickEvent = call;
		}

		// Token: 0x06000CEF RID: 3311 RVA: 0x000405C5 File Offset: 0x0003E7C5
		public void OnGamepadSelectedChanged(bool value)
		{
		}

		// Token: 0x06000CF0 RID: 3312 RVA: 0x000405C7 File Offset: 0x0003E7C7
		public void OnGamepadClickCardDetail()
		{
			this.OnPointerClick(null);
		}

		// Token: 0x040009D3 RID: 2515
		private UnityAction _clickEvent;

		// Token: 0x040009D4 RID: 2516
		public bool tiltWhenHover = true;

		// Token: 0x040009D6 RID: 2518
		private const float MaxTiltAngle = 8f;

		// Token: 0x040009D7 RID: 2519
		private const float CardRadius = 455f;

		// Token: 0x040009D8 RID: 2520
		private float _pointerX;

		// Token: 0x040009D9 RID: 2521
		private float _pointerY;

		// Token: 0x040009DA RID: 2522
		private float _screenScale = 1f;
	}
}
