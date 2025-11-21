using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Transitions
{
	// Token: 0x02000085 RID: 133
	public class StartGameTransition : UiTransition
	{
		// Token: 0x17000126 RID: 294
		// (get) Token: 0x060006AC RID: 1708 RVA: 0x0001D404 File Offset: 0x0001B604
		// (set) Token: 0x060006AD RID: 1709 RVA: 0x0001D40C File Offset: 0x0001B60C
		private Sequence Sequence { get; set; }

		// Token: 0x060006AE RID: 1710 RVA: 0x0001D418 File Offset: 0x0001B618
		public override void Animate(Transform target, bool isOut, Action onComplete)
		{
			target.DOKill(false);
			CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
			if (canvasGroup)
			{
				canvasGroup.DOKill(false);
			}
			if (!isOut)
			{
				canvasGroup.alpha = 1f;
				Sequence sequence = DOTween.Sequence();
				sequence.Join(this.blackMask.DOFade(0f, this.duration * 2f).From(1f, true, false));
				sequence.Join(this.background.DOScale(1f, this.duration * 2f).From(3f, true, false));
				sequence.Join(this.backgroundManaRotationRoot.DOScale(1f, this.duration * 2f).From(3f, true, false));
				sequence.Join(this.bottomGroupRoot.DOLocalMoveY(-1500f, this.duration, false).From<TweenerCore<Vector3, Vector3, VectorOptions>>().SetRelative<TweenerCore<Vector3, Vector3, VectorOptions>>());
				sequence.Join(this.bottomGroupRoot.GetComponent<CanvasGroup>().DOFade(1f, this.duration).From(0f, true, false));
				sequence.Join(this.nameGroupRoot.DOAnchorPos(new Vector2(1000f, 1000f), this.duration, false).From<TweenerCore<Vector2, Vector2, VectorOptions>>().SetRelative<TweenerCore<Vector2, Vector2, VectorOptions>>());
				sequence.Join(this.nameGroupRoot.GetComponent<CanvasGroup>().DOFade(1f, this.duration).From(0f, true, false));
				for (int i = 0; i < this.characters.Count; i++)
				{
					if (i == 2)
					{
						sequence.Join(this.characters[i].DOAnchorPosY(-200f, this.duration, false).From<TweenerCore<Vector2, Vector2, VectorOptions>>().SetRelative<TweenerCore<Vector2, Vector2, VectorOptions>>());
					}
					else
					{
						sequence.Join(this.characters[i].DOAnchorPosX(this._standPicX[i], this.duration, false).From<TweenerCore<Vector2, Vector2, VectorOptions>>().SetRelative<TweenerCore<Vector2, Vector2, VectorOptions>>());
					}
				}
				sequence.Join(this.confirmButton.DOAnchorPosX(500f, this.duration, false).From<TweenerCore<Vector2, Vector2, VectorOptions>>().SetRelative<TweenerCore<Vector2, Vector2, VectorOptions>>());
				sequence.Join(this.returnButton.DOAnchorPosX(-500f, this.duration, false).From<TweenerCore<Vector2, Vector2, VectorOptions>>().SetRelative<TweenerCore<Vector2, Vector2, VectorOptions>>());
				sequence.Join(this.replayOpeningButton.DOAnchorPosX(-500f, this.duration, false).From<TweenerCore<Vector2, Vector2, VectorOptions>>().SetRelative<TweenerCore<Vector2, Vector2, VectorOptions>>());
				sequence.Join(this.setupGroupRoot.DOAnchorPosY(-500f, this.duration, false).From<TweenerCore<Vector2, Vector2, VectorOptions>>().SetRelative<TweenerCore<Vector2, Vector2, VectorOptions>>()
					.SetDelay(0.4f));
				sequence.Join(this.setupGroupRoot.GetComponent<CanvasGroup>().DOFade(1f, this.duration).From(0f, true, false)
					.SetDelay(0.4f));
				sequence.SetUpdate(true).OnComplete(delegate
				{
					StartGameTransition.Cleanup(onComplete, target, canvasGroup);
				}).Play<Sequence>();
				this.Sequence = sequence;
				return;
			}
			Sequence sequence2 = DOTween.Sequence();
			sequence2.Join(canvasGroup.DOFade(0f, 0.2f).From(1f, true, false));
			sequence2.SetUpdate(true).OnComplete(delegate
			{
				StartGameTransition.Cleanup(onComplete, target, canvasGroup);
			}).Play<Sequence>();
			this.Sequence = sequence2;
		}

		// Token: 0x060006AF RID: 1711 RVA: 0x0001D7A6 File Offset: 0x0001B9A6
		public override void Kill(Transform target)
		{
			this.Sequence.Complete(false);
			this.Sequence.Kill(false);
		}

		// Token: 0x060006B0 RID: 1712 RVA: 0x0001D7C0 File Offset: 0x0001B9C0
		private static void Cleanup(Action onComplete, Transform trans, CanvasGroup canvasGroup)
		{
			onComplete.Invoke();
		}

		// Token: 0x0400043B RID: 1083
		[SerializeField]
		private float duration = 0.2f;

		// Token: 0x0400043C RID: 1084
		[SerializeField]
		protected Ease ease = Ease.Linear;

		// Token: 0x0400043D RID: 1085
		[SerializeField]
		private Image blackMask;

		// Token: 0x0400043E RID: 1086
		[SerializeField]
		private RectTransform background;

		// Token: 0x0400043F RID: 1087
		[SerializeField]
		private RectTransform backgroundManaRotationRoot;

		// Token: 0x04000440 RID: 1088
		[SerializeField]
		private RectTransform bottomGroupRoot;

		// Token: 0x04000441 RID: 1089
		[SerializeField]
		private RectTransform nameGroupRoot;

		// Token: 0x04000442 RID: 1090
		[SerializeField]
		private RectTransform setupGroupRoot;

		// Token: 0x04000443 RID: 1091
		[SerializeField]
		private List<RectTransform> characters;

		// Token: 0x04000444 RID: 1092
		[SerializeField]
		private RectTransform confirmButton;

		// Token: 0x04000445 RID: 1093
		[SerializeField]
		private RectTransform returnButton;

		// Token: 0x04000446 RID: 1094
		[SerializeField]
		private RectTransform replayOpeningButton;

		// Token: 0x04000448 RID: 1096
		private readonly float[] _standPicX = new float[] { -400f, -200f, 0f, 200f, 400f };
	}
}
