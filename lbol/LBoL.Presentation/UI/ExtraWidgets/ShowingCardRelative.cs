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
	[RequireComponent(typeof(CardWidget))]
	public class ShowingCardRelative : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
	{
		private RectTransform RectTransform
		{
			get
			{
				return (RectTransform)base.transform;
			}
		}
		private float NormalScale { get; set; }
		private float HoverScale { get; set; }
		private bool Hovering { get; set; }
		private void Awake()
		{
			this.SetScale(1f, 1.2f);
		}
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
		public void OnPointerEnter(PointerEventData eventData)
		{
			this.StartHover();
		}
		public void OnPointerExit(PointerEventData eventData)
		{
			this.EndHover();
		}
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
		public void OnPointerClick(PointerEventData eventData)
		{
			this.EndHover();
			this._clickEvent();
		}
		public void SetScale(float normal, float hover)
		{
			this.NormalScale = normal;
			this.HoverScale = hover;
			base.transform.localScale = new Vector3(normal, normal, 1f);
		}
		public void AddListener(UnityAction call)
		{
			this._clickEvent = call;
		}
		public void OnGamepadSelectedChanged(bool value)
		{
		}
		public void OnGamepadClickCardDetail()
		{
			this.OnPointerClick(null);
		}
		private UnityAction _clickEvent;
		public bool tiltWhenHover = true;
		private const float MaxTiltAngle = 8f;
		private const float CardRadius = 455f;
		private float _pointerX;
		private float _pointerY;
		private float _screenScale = 1f;
	}
}
