using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Transitions
{
	public class MapTransition : UiTransition
	{
		private Sequence Sequence { get; set; }
		private float DeltaX { get; set; }
		private float TweenValue
		{
			get
			{
				return this._tweenValue;
			}
			set
			{
				this._tweenValue = value;
				this.scroll.anchoredPosition = new Vector2(this.DeltaX * (1f - value), 0f);
				float num = this.scroll.anchoredPosition.x - this.scroll.sizeDelta.x / 2f;
				num = Mathf.Min(3840f, num);
				num = Mathf.Max(0f, num);
				this.mask.padding = new Vector4(num, 0f, 0f, 0f);
			}
		}
		public override void Animate(Transform target, bool isOut, Action onComplete)
		{
			target.DOKill(false);
			CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
			if (canvasGroup)
			{
				canvasGroup.DOKill(false);
			}
			this.DeltaX = 3840f + this.scroll.sizeDelta.x * 2f;
			Sequence sequence = DOTween.Sequence();
			if (isOut)
			{
				sequence.Join(DOTween.To(() => this.TweenValue, delegate(float x)
				{
					this.TweenValue = x;
				}, 0f, this.duration).From(1f, true, false).SetEase(this.ease));
				if (this.bg != null)
				{
					sequence.Append(this.bg.DOFade(0f, 0.2f));
				}
			}
			else
			{
				target.DOKill(false);
				sequence.Join(DOTween.To(() => this.TweenValue, delegate(float x)
				{
					this.TweenValue = x;
				}, 1f, this.duration).From(0f, true, false).SetEase(this.ease));
				if (this.bg != null)
				{
					sequence.Join(this.bg.DOFade(1f, 0.2f));
				}
			}
			sequence.SetUpdate(true).OnComplete(delegate
			{
				MapTransition.Cleanup(onComplete, target, canvasGroup);
			}).Play<Sequence>();
			this.Sequence = sequence;
		}
		public override void Kill(Transform target)
		{
			this.Sequence.Kill(false);
		}
		private static void Cleanup(Action onComplete, Transform trans, CanvasGroup canvasGroup)
		{
			onComplete.Invoke();
		}
		[SerializeField]
		private float duration = 0.2f;
		[SerializeField]
		protected Ease ease = Ease.Linear;
		[SerializeField]
		private RectMask2D mask;
		[SerializeField]
		private RectTransform scroll;
		[SerializeField]
		private CanvasGroup bg;
		private float _tweenValue;
	}
}
