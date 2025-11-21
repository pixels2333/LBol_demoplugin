using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace LBoL.Presentation.UI.Transitions
{
	// Token: 0x02000081 RID: 129
	public class MusicRoomTransition : UiTransition
	{
		// Token: 0x0600069E RID: 1694 RVA: 0x0001CC9C File Offset: 0x0001AE9C
		public override void Animate(Transform target, bool isOut, Action onComplete)
		{
			target.DOKill(false);
			CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
			if (canvasGroup)
			{
				canvasGroup.DOKill(false);
			}
			if (isOut)
			{
				canvasGroup.DOFade(0f, this.duration).From(1f, true, false).SetEase(this.ease)
					.SetUpdate(true)
					.SetTarget(target)
					.OnComplete(delegate
					{
						MusicRoomTransition.Cleanup(onComplete, target, canvasGroup);
					});
				return;
			}
			DOTween.Sequence().Join(canvasGroup.DOFade(1f, this.duration).From(0f, true, false).SetEase(this.ease)).Join(this.leftPanel.DOAnchorPosX(0f, this.duration, false).From(new Vector2(-2000f, 0f), true, false).SetEase(this.ease))
				.Join(this.rightPanel.DOAnchorPosX(-1000f, this.duration, false).From(new Vector2(1000f, 0f), true, false).SetEase(this.ease))
				.SetUpdate(true)
				.SetTarget(target)
				.OnComplete(delegate
				{
					MusicRoomTransition.Cleanup(onComplete, target, canvasGroup);
				});
		}

		// Token: 0x0600069F RID: 1695 RVA: 0x0001CE1D File Offset: 0x0001B01D
		public override void Kill(Transform target)
		{
			target.DOKill(false);
		}

		// Token: 0x060006A0 RID: 1696 RVA: 0x0001CE27 File Offset: 0x0001B027
		private static void Cleanup(Action onComplete, Transform trans, CanvasGroup canvasGroup)
		{
			onComplete.Invoke();
		}

		// Token: 0x04000424 RID: 1060
		[SerializeField]
		private float duration = 0.2f;

		// Token: 0x04000425 RID: 1061
		[SerializeField]
		protected Ease ease = Ease.Linear;

		// Token: 0x04000426 RID: 1062
		[SerializeField]
		private RectTransform leftPanel;

		// Token: 0x04000427 RID: 1063
		[SerializeField]
		private RectTransform rightPanel;

		// Token: 0x04000428 RID: 1064
		[SerializeField]
		private RectTransform bottomPanel;
	}
}
