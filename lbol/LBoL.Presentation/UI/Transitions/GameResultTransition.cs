using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base.Extensions;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Transitions
{
	public class GameResultTransition : UiTransition
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
				this.scroll.anchoredPosition = new Vector2(this.DeltaX * (1f - value), -110f);
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
			this.DeltaX = 3840f + this.scroll.sizeDelta.x;
			Sequence sequence = DOTween.Sequence();
			if (isOut)
			{
				sequence.Join(DOTween.To(() => this.TweenValue, delegate(float x)
				{
					this.TweenValue = x;
				}, 0f, this.duration).From(1f, true, false).SetEase(this.ease));
				sequence.Append(canvasGroup.DOFade(0f, 0.2f).From(1f, true, false));
			}
			else
			{
				target.DOKill(false);
				sequence.Join(canvasGroup.DOFade(1f, 0.4f).From(0f, true, false));
				sequence.Join(DOTween.To(() => this.TweenValue, delegate(float x)
				{
					this.TweenValue = x;
				}, 1f, this.duration).From(0f, true, false).SetEase(this.ease));
				foreach (Image image in this.waveImageList)
				{
					image.transform.DOKill(false);
					int num = this.waveImageList.IndexOf(image);
					image.transform.DOLocalMoveY(40f, 1f, false).SetRelative<TweenerCore<Vector3, Vector3, VectorOptions>>().SetLoops(-1, LoopType.Yoyo)
						.SetEase(Ease.InOutSine)
						.SetDelay((float)num * 0.33f)
						.SetLink(image.gameObject);
				}
				float lerp = 0f;
				DOTween.To(() => lerp, delegate(float v)
				{
					lerp = v;
					v.Lerp(0f, 1f);
					this.bambooImage.uvRect = new Rect(lerp, 0f, 1f, 1f);
				}, 1f, 24f).SetLoops(-1, LoopType.Restart).SetLink(base.gameObject)
					.SetEase(Ease.Linear);
			}
			sequence.SetUpdate(true).OnComplete(delegate
			{
				GameResultTransition.Cleanup(onComplete, target, canvasGroup);
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
		private List<Image> waveImageList;
		[SerializeField]
		private RawImage bambooImage;
		private float _tweenValue;
	}
}
