using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Presentation.InputSystemExtend;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace LBoL.Presentation.UI.ExtraWidgets
{
	// Token: 0x020000D3 RID: 211
	[RequireComponent(typeof(CardWidget))]
	[DisallowMultipleComponent]
	public sealed class ShowingCard : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
	{
		// Token: 0x17000213 RID: 531
		// (get) Token: 0x06000CCA RID: 3274 RVA: 0x0003FF12 File Offset: 0x0003E112
		private RectTransform RectTransform
		{
			get
			{
				return (RectTransform)base.transform;
			}
		}

		// Token: 0x17000214 RID: 532
		// (get) Token: 0x06000CCB RID: 3275 RVA: 0x0003FF1F File Offset: 0x0003E11F
		// (set) Token: 0x06000CCC RID: 3276 RVA: 0x0003FF27 File Offset: 0x0003E127
		private float NormalScale { get; set; }

		// Token: 0x17000215 RID: 533
		// (get) Token: 0x06000CCD RID: 3277 RVA: 0x0003FF30 File Offset: 0x0003E130
		// (set) Token: 0x06000CCE RID: 3278 RVA: 0x0003FF38 File Offset: 0x0003E138
		private float HoverScale { get; set; }

		// Token: 0x17000216 RID: 534
		// (get) Token: 0x06000CCF RID: 3279 RVA: 0x0003FF41 File Offset: 0x0003E141
		// (set) Token: 0x06000CD0 RID: 3280 RVA: 0x0003FF49 File Offset: 0x0003E149
		private CardWidget CardWidget { get; set; }

		// Token: 0x17000217 RID: 535
		// (get) Token: 0x06000CD1 RID: 3281 RVA: 0x0003FF52 File Offset: 0x0003E152
		// (set) Token: 0x06000CD2 RID: 3282 RVA: 0x0003FF5A File Offset: 0x0003E15A
		private bool Hovering { get; set; }

		// Token: 0x06000CD3 RID: 3283 RVA: 0x0003FF63 File Offset: 0x0003E163
		private void Awake()
		{
			this.CardWidget = base.GetComponent<CardWidget>();
		}

		// Token: 0x06000CD4 RID: 3284 RVA: 0x0003FF74 File Offset: 0x0003E174
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
				if (vector2.magnitude > 600f)
				{
					this.EndHover();
					return;
				}
				Quaternion quaternion = Quaternion.AngleAxis(Mathf.Clamp01(vector2.magnitude / (455f * this.HoverScale)) * 5f, new Vector3(vector2.y, -vector2.x));
				this.RectTransform.localRotation = quaternion * Quaternion.identity;
			}
		}

		// Token: 0x06000CD5 RID: 3285 RVA: 0x000400C1 File Offset: 0x0003E2C1
		public void OnPointerEnter(PointerEventData eventData)
		{
			this.StartHover();
		}

		// Token: 0x06000CD6 RID: 3286 RVA: 0x000400C9 File Offset: 0x0003E2C9
		public void OnPointerExit(PointerEventData eventData)
		{
			this.EndHover();
		}

		// Token: 0x06000CD7 RID: 3287 RVA: 0x000400D4 File Offset: 0x0003E2D4
		private void StartHover()
		{
			this.Hovering = true;
			UiManager.HoveringRightClickInteractionElements = true;
			if (Math.Abs(this.NormalScale - this.HoverScale) > 0.01f)
			{
				base.transform.DOScale(this.HoverScale, 0.12f).SetUpdate(true).SetEase(Ease.OutCubic);
			}
			this.CardWidget.ShowTooltip();
			AudioManager.Card(0);
		}

		// Token: 0x06000CD8 RID: 3288 RVA: 0x0004013C File Offset: 0x0003E33C
		private void EndHover()
		{
			this.Hovering = false;
			UiManager.HoveringRightClickInteractionElements = false;
			if (Math.Abs(this.NormalScale - this.HoverScale) > 0.01f)
			{
				base.transform.DOScale(this.NormalScale, 0.12f).SetUpdate(true).SetEase(Ease.OutCubic);
			}
			this.CardWidget.HideTooltip();
			if (this.tiltWhenHover)
			{
				this.RectTransform.DOLocalRotateQuaternion(Quaternion.identity, 0.12f).SetUpdate(true).SetEase(Ease.OutCubic);
			}
		}

		// Token: 0x06000CD9 RID: 3289 RVA: 0x000401CC File Offset: 0x0003E3CC
		public void OnPointerClick(PointerEventData eventData)
		{
			if (!this.canDetail)
			{
				return;
			}
			string topPanel = Singleton<GamepadNavigationManager>.Instance.GetTopPanel();
			if (eventData.button == PointerEventData.InputButton.Right || this.leftToDetail)
			{
				AudioManager.Card(3);
				GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
				UiManager.GetPanel<CardDetailPanel>().Show(new CardDetailPayload(base.GetComponent<RectTransform>(), this.CardWidget.Card, false));
				GamepadNavigationManager.SetOverrideOrigin(currentSelectedGameObject, topPanel);
			}
		}

		// Token: 0x06000CDA RID: 3290 RVA: 0x00040235 File Offset: 0x0003E435
		public void SetScale(float normal)
		{
			this.SetScale(normal, normal * 1.15f);
		}

		// Token: 0x06000CDB RID: 3291 RVA: 0x00040245 File Offset: 0x0003E445
		public void SetScale(float normal, float hover)
		{
			this.NormalScale = normal;
			this.HoverScale = hover;
			base.transform.localScale = new Vector3(normal, normal, 1f);
			this.tiltWhenHover = hover > normal * 1.1f;
		}

		// Token: 0x06000CDC RID: 3292 RVA: 0x0004027C File Offset: 0x0003E47C
		public void OnGamepadSelectedChanged(bool value)
		{
			if (!this.CardWidget)
			{
				return;
			}
			if (value && base.enabled && base.gameObject.activeSelf)
			{
				this.CardWidget.ShowTooltip();
				return;
			}
			this.CardWidget.HideTooltip();
		}

		// Token: 0x06000CDD RID: 3293 RVA: 0x000402BC File Offset: 0x0003E4BC
		public void OnGamepadClickCardDetail()
		{
			this.OnPointerClick(new PointerEventData(EventSystem.current)
			{
				button = PointerEventData.InputButton.Right
			});
		}

		// Token: 0x040009C6 RID: 2502
		public bool leftToDetail;

		// Token: 0x040009C7 RID: 2503
		public bool canDetail = true;

		// Token: 0x040009C9 RID: 2505
		public bool tiltWhenHover = true;

		// Token: 0x040009CB RID: 2507
		private const float MaxTiltAngle = 5f;

		// Token: 0x040009CC RID: 2508
		private const float CardRadius = 455f;

		// Token: 0x040009CD RID: 2509
		private const float LeaveRadius = 600f;

		// Token: 0x040009CE RID: 2510
		private float _pointerX;

		// Token: 0x040009CF RID: 2511
		private float _pointerY;

		// Token: 0x040009D0 RID: 2512
		private float _screenScale = 1f;
	}
}
