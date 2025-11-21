using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000061 RID: 97
	public class MainMenuButtonWidget : CommonButtonWidget
	{
		// Token: 0x06000549 RID: 1353 RVA: 0x00016DB7 File Offset: 0x00014FB7
		private void OnDisable()
		{
			if (this._highlight)
			{
				this.FadeOut(0f);
				this._highlight = false;
			}
		}

		// Token: 0x0600054A RID: 1354 RVA: 0x00016DD4 File Offset: 0x00014FD4
		private void FadeIn(float duration)
		{
			this._fadeTween.Kill(false);
			this._fadeTween = DOTween.Sequence().Join(this.cg.DOFade(1f, duration)).Join(this.ps.transform.DOLocalMoveX(this._localX, duration, false).From(this._localX - 100f, true, false))
				.SetLink(base.gameObject)
				.SetUpdate(true);
		}

		// Token: 0x0600054B RID: 1355 RVA: 0x00016E50 File Offset: 0x00015050
		private void FadeOut(float duration)
		{
			this._fadeTween.Kill(false);
			this._fadeTween = DOTween.Sequence().Join(this.cg.DOFade(0f, duration)).Join(this.ps.transform.DOLocalMoveX(this._localX - 100f, duration, false).SetEase(Ease.OutCubic))
				.SetLink(base.gameObject)
				.SetUpdate(true);
		}

		// Token: 0x0600054C RID: 1356 RVA: 0x00016EC5 File Offset: 0x000150C5
		public override void OnPointerEnter(PointerEventData eventData)
		{
			base.OnPointerEnter(eventData);
			if (this._highlight)
			{
				return;
			}
			this.FadeIn(0.3f);
			this._highlight = true;
		}

		// Token: 0x0600054D RID: 1357 RVA: 0x00016EE9 File Offset: 0x000150E9
		public override void OnPointerExit(PointerEventData eventData)
		{
			if (!this._highlight)
			{
				return;
			}
			this.FadeOut(0.6f);
			this._highlight = false;
		}

		// Token: 0x0600054E RID: 1358 RVA: 0x00016F08 File Offset: 0x00015108
		public override void OnPointerClick(PointerEventData eventData)
		{
			base.OnPointerClick(eventData);
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				this._highlight = false;
				Sequence sequence = DOTween.Sequence();
				sequence.Insert(0f, this.ps.transform.DOScaleX(1.4f, this.time / 2f).From(1f, true, false));
				sequence.Insert(0f, this.ps.transform.DOScaleY(0.6f, this.time / 2f).From(1f, true, false));
				sequence.Insert(this.time / 2f, this.ps.transform.DOScaleX(0f, this.time).From(0.8f, true, false));
				sequence.Insert(this.time / 2f, this.ps.transform.DOScaleY(1.5f, this.time).From(1.2f, true, false));
				sequence.SetLink(base.gameObject);
				sequence.onComplete = (TweenCallback)Delegate.Combine(sequence.onComplete, delegate
				{
					this.ps.transform.localScale = Vector3.one;
					this.FadeOut(0.6f);
					this._highlight = false;
				});
			}
		}

		// Token: 0x04000311 RID: 785
		[SerializeField]
		private GameObject ps;

		// Token: 0x04000312 RID: 786
		[SerializeField]
		private CanvasGroup cg;

		// Token: 0x04000313 RID: 787
		[SerializeField]
		private Image image;

		// Token: 0x04000314 RID: 788
		[SerializeField]
		private float time;

		// Token: 0x04000315 RID: 789
		private bool _highlight;

		// Token: 0x04000316 RID: 790
		private Tween _fadeTween;

		// Token: 0x04000317 RID: 791
		private float _localX;
	}
}
