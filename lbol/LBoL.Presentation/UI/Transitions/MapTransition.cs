using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Transitions
{
	// Token: 0x02000080 RID: 128
	public class MapTransition : UiTransition
	{
		// Token: 0x17000123 RID: 291
		// (get) Token: 0x06000694 RID: 1684 RVA: 0x0001CA04 File Offset: 0x0001AC04
		// (set) Token: 0x06000695 RID: 1685 RVA: 0x0001CA0C File Offset: 0x0001AC0C
		private Sequence Sequence { get; set; }

		// Token: 0x17000124 RID: 292
		// (get) Token: 0x06000696 RID: 1686 RVA: 0x0001CA15 File Offset: 0x0001AC15
		// (set) Token: 0x06000697 RID: 1687 RVA: 0x0001CA1D File Offset: 0x0001AC1D
		private float DeltaX { get; set; }

		// Token: 0x17000125 RID: 293
		// (get) Token: 0x06000698 RID: 1688 RVA: 0x0001CA26 File Offset: 0x0001AC26
		// (set) Token: 0x06000699 RID: 1689 RVA: 0x0001CA30 File Offset: 0x0001AC30
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

		// Token: 0x0600069A RID: 1690 RVA: 0x0001CAC8 File Offset: 0x0001ACC8
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

		// Token: 0x0600069B RID: 1691 RVA: 0x0001CC6A File Offset: 0x0001AE6A
		public override void Kill(Transform target)
		{
			this.Sequence.Kill(false);
		}

		// Token: 0x0600069C RID: 1692 RVA: 0x0001CC78 File Offset: 0x0001AE78
		private static void Cleanup(Action onComplete, Transform trans, CanvasGroup canvasGroup)
		{
			onComplete.Invoke();
		}

		// Token: 0x0400041C RID: 1052
		[SerializeField]
		private float duration = 0.2f;

		// Token: 0x0400041D RID: 1053
		[SerializeField]
		protected Ease ease = Ease.Linear;

		// Token: 0x0400041E RID: 1054
		[SerializeField]
		private RectMask2D mask;

		// Token: 0x0400041F RID: 1055
		[SerializeField]
		private RectTransform scroll;

		// Token: 0x04000420 RID: 1056
		[SerializeField]
		private CanvasGroup bg;

		// Token: 0x04000423 RID: 1059
		private float _tweenValue;
	}
}
