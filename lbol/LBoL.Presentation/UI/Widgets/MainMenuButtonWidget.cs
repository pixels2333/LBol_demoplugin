using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class MainMenuButtonWidget : CommonButtonWidget
	{
		private void OnDisable()
		{
			if (this._highlight)
			{
				this.FadeOut(0f);
				this._highlight = false;
			}
		}
		private void FadeIn(float duration)
		{
			this._fadeTween.Kill(false);
			this._fadeTween = DOTween.Sequence().Join(this.cg.DOFade(1f, duration)).Join(this.ps.transform.DOLocalMoveX(this._localX, duration, false).From(this._localX - 100f, true, false))
				.SetLink(base.gameObject)
				.SetUpdate(true);
		}
		private void FadeOut(float duration)
		{
			this._fadeTween.Kill(false);
			this._fadeTween = DOTween.Sequence().Join(this.cg.DOFade(0f, duration)).Join(this.ps.transform.DOLocalMoveX(this._localX - 100f, duration, false).SetEase(Ease.OutCubic))
				.SetLink(base.gameObject)
				.SetUpdate(true);
		}
		public override void OnPointerEnter(PointerEventData eventData)
		{
			base.OnPointerEnter(eventData);
			if (this._highlight)
			{
				return;
			}
			this.FadeIn(0.3f);
			this._highlight = true;
		}
		public override void OnPointerExit(PointerEventData eventData)
		{
			if (!this._highlight)
			{
				return;
			}
			this.FadeOut(0.6f);
			this._highlight = false;
		}
		public override void OnPointerClick(PointerEventData eventData)
		{
			base.OnPointerClick(eventData);
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				this._highlight = false;
				Sequence sequence = DOTween.Sequence();
				sequence.Insert(0f, this.ps.transform.DOScaleX(1.4f, this.time / 2f).From(1f, true, false));
				sequence.Insert(0f, this.ps.transform.DOScaleY(0.6f, this.time / 2f).From(1f, true, false));
				sequence.Insert(this.time / 2f, this.ps.transform.DOScaleX(0f, this.time).From(0.8f, true, false));
				sequence.Insert(this.time / 2f, this.ps.transform.DOScaleY(1.5f, this.time).From(1.2f, true, false));
				sequence.SetLink(base.gameObject);
				sequence.onComplete = (TweenCallback)Delegate.Combine(sequence.onComplete, delegate
				{
					this.ps.transform.localScale = Vector3.one;
					this.FadeOut(0.6f);
					this._highlight = false;
				});
			}
		}
		[SerializeField]
		private GameObject ps;
		[SerializeField]
		private CanvasGroup cg;
		[SerializeField]
		private Image image;
		[SerializeField]
		private float time;
		private bool _highlight;
		private Tween _fadeTween;
		private float _localX;
	}
}
