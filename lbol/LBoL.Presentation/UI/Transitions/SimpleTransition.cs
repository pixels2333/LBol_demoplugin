using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI.Extensions;
namespace LBoL.Presentation.UI.Transitions
{
	public class SimpleTransition : UiTransition
	{
		public override void Animate(Transform target, bool isOut, Action onComplete)
		{
			target.DOKill(false);
			CanvasGroup canvasGroup = target.gameObject.GetOrAddComponent<CanvasGroup>();
			if (canvasGroup)
			{
				canvasGroup.DOKill(false);
			}
			Sequence sequence = DOTween.Sequence();
			if (isOut)
			{
				if (this.doMove)
				{
					sequence.Join(target.DOLocalMove(this.deltaPosition, this.duration, false).From(Vector3.zero, true, false).SetEase(this.ease));
				}
				if (this.doScale)
				{
					sequence.Join(target.DOScale(this.scale, this.duration).From(Vector3.one, true, false).SetEase(this.ease));
				}
				if (this.doFade && canvasGroup)
				{
					sequence.Join(canvasGroup.DOFade(0f, this.duration).From(1f, true, false).SetEase(this.ease));
				}
			}
			else
			{
				target.DOKill(false);
				if (this.doMove)
				{
					sequence.Join(target.DOLocalMove(Vector3.zero, this.duration, false).From(this.deltaPosition, true, false).SetEase(this.ease));
				}
				if (this.doScale)
				{
					sequence.Join(target.DOScale(Vector3.one, this.duration).From(this.scale, true, false).SetEase(this.ease));
				}
				if (this.doFade && canvasGroup)
				{
					sequence.Join(canvasGroup.DOFade(1f, this.duration).From(0f, true, false).SetEase(this.ease));
				}
			}
			sequence.SetUpdate(true).SetLink(target.gameObject).SetTarget(target)
				.OnComplete(delegate
				{
					SimpleTransition.Cleanup(onComplete, target, canvasGroup);
				})
				.Play<Sequence>();
		}
		public override void Kill(Transform target)
		{
			target.DOComplete(false);
		}
		private static void Cleanup(Action onComplete, Transform trans, CanvasGroup canvasGroup)
		{
			onComplete.Invoke();
			trans.localPosition = Vector3.zero;
			trans.localScale = Vector3.one;
			if (canvasGroup)
			{
				canvasGroup.alpha = 1f;
			}
		}
		[SerializeField]
		private float duration = 0.2f;
		[SerializeField]
		private bool doMove;
		[SerializeField]
		private Vector2 deltaPosition;
		[SerializeField]
		private bool doScale;
		[SerializeField]
		private Vector2 scale;
		[SerializeField]
		private bool doFade;
		[SerializeField]
		protected Ease ease = Ease.Linear;
	}
}
