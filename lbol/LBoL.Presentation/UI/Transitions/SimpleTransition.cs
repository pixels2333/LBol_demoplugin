using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace LBoL.Presentation.UI.Transitions
{
	// Token: 0x02000084 RID: 132
	public class SimpleTransition : UiTransition
	{
		// Token: 0x060006A8 RID: 1704 RVA: 0x0001D158 File Offset: 0x0001B358
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

		// Token: 0x060006A9 RID: 1705 RVA: 0x0001D3AF File Offset: 0x0001B5AF
		public override void Kill(Transform target)
		{
			target.DOComplete(false);
		}

		// Token: 0x060006AA RID: 1706 RVA: 0x0001D3B9 File Offset: 0x0001B5B9
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

		// Token: 0x04000434 RID: 1076
		[SerializeField]
		private float duration = 0.2f;

		// Token: 0x04000435 RID: 1077
		[SerializeField]
		private bool doMove;

		// Token: 0x04000436 RID: 1078
		[SerializeField]
		private Vector2 deltaPosition;

		// Token: 0x04000437 RID: 1079
		[SerializeField]
		private bool doScale;

		// Token: 0x04000438 RID: 1080
		[SerializeField]
		private Vector2 scale;

		// Token: 0x04000439 RID: 1081
		[SerializeField]
		private bool doFade;

		// Token: 0x0400043A RID: 1082
		[SerializeField]
		protected Ease ease = Ease.Linear;
	}
}
