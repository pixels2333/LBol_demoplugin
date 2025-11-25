using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
namespace LBoL.Presentation.UI.Transitions
{
	public class MusicRoomTransition : UiTransition
	{
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
		public override void Kill(Transform target)
		{
			target.DOKill(false);
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
		private RectTransform leftPanel;
		[SerializeField]
		private RectTransform rightPanel;
		[SerializeField]
		private RectTransform bottomPanel;
	}
}
