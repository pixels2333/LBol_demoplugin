using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Transitions
{
	public class StartGameTransition : UiTransition
	{
		private Sequence Sequence { get; set; }
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
		public override void Kill(Transform target)
		{
			this.Sequence.Complete(false);
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
		private Image blackMask;
		[SerializeField]
		private RectTransform background;
		[SerializeField]
		private RectTransform backgroundManaRotationRoot;
		[SerializeField]
		private RectTransform bottomGroupRoot;
		[SerializeField]
		private RectTransform nameGroupRoot;
		[SerializeField]
		private RectTransform setupGroupRoot;
		[SerializeField]
		private List<RectTransform> characters;
		[SerializeField]
		private RectTransform confirmButton;
		[SerializeField]
		private RectTransform returnButton;
		[SerializeField]
		private RectTransform replayOpeningButton;
		private readonly float[] _standPicX = new float[] { -400f, -200f, 0f, 200f, 400f };
	}
}
