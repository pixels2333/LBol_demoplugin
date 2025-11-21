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
	// Token: 0x0200007F RID: 127
	public class GameResultTransition : UiTransition
	{
		// Token: 0x17000120 RID: 288
		// (get) Token: 0x0600068A RID: 1674 RVA: 0x0001C669 File Offset: 0x0001A869
		// (set) Token: 0x0600068B RID: 1675 RVA: 0x0001C671 File Offset: 0x0001A871
		private Sequence Sequence { get; set; }

		// Token: 0x17000121 RID: 289
		// (get) Token: 0x0600068C RID: 1676 RVA: 0x0001C67A File Offset: 0x0001A87A
		// (set) Token: 0x0600068D RID: 1677 RVA: 0x0001C682 File Offset: 0x0001A882
		private float DeltaX { get; set; }

		// Token: 0x17000122 RID: 290
		// (get) Token: 0x0600068E RID: 1678 RVA: 0x0001C68B File Offset: 0x0001A88B
		// (set) Token: 0x0600068F RID: 1679 RVA: 0x0001C694 File Offset: 0x0001A894
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

		// Token: 0x06000690 RID: 1680 RVA: 0x0001C72C File Offset: 0x0001A92C
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

		// Token: 0x06000691 RID: 1681 RVA: 0x0001C9D4 File Offset: 0x0001ABD4
		public override void Kill(Transform target)
		{
			this.Sequence.Kill(false);
		}

		// Token: 0x06000692 RID: 1682 RVA: 0x0001C9E2 File Offset: 0x0001ABE2
		private static void Cleanup(Action onComplete, Transform trans, CanvasGroup canvasGroup)
		{
			onComplete.Invoke();
		}

		// Token: 0x04000413 RID: 1043
		[SerializeField]
		private float duration = 0.2f;

		// Token: 0x04000414 RID: 1044
		[SerializeField]
		protected Ease ease = Ease.Linear;

		// Token: 0x04000415 RID: 1045
		[SerializeField]
		private RectMask2D mask;

		// Token: 0x04000416 RID: 1046
		[SerializeField]
		private RectTransform scroll;

		// Token: 0x04000417 RID: 1047
		[SerializeField]
		private List<Image> waveImageList;

		// Token: 0x04000418 RID: 1048
		[SerializeField]
		private RawImage bambooImage;

		// Token: 0x0400041B RID: 1051
		private float _tweenValue;
	}
}
