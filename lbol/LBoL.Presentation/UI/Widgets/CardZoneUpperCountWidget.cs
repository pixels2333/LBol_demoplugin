using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000047 RID: 71
	public class CardZoneUpperCountWidget : MonoBehaviour
	{
		// Token: 0x0600047A RID: 1146 RVA: 0x00012645 File Offset: 0x00010845
		public void Awake()
		{
			base.gameObject.SetActive(false);
			this._isActive = false;
			this._isTransitioned = false;
		}

		// Token: 0x0600047B RID: 1147 RVA: 0x00012664 File Offset: 0x00010864
		public void Active(bool active)
		{
			if (active)
			{
				if (!this._isTransitioned)
				{
					this._isTransitioned = true;
					base.gameObject.SetActive(true);
					this._isActive = true;
					this.Animate(false);
					return;
				}
			}
			else if (this._isActive)
			{
				this.Animate(true);
				this._sequence.OnComplete(delegate
				{
					base.gameObject.SetActive(false);
					this._isActive = false;
				});
			}
		}

		// Token: 0x0600047C RID: 1148 RVA: 0x000126C8 File Offset: 0x000108C8
		private void Animate(bool isOut)
		{
			this._sequence.Kill(false);
			this._loopTween.Kill(false);
			this._sequence = DOTween.Sequence();
			if (isOut)
			{
				this._sequence.Join(this.bg.DOScale(Vector3.zero, 0.2f).From(Vector3.one, true, false).SetEase(Ease.OutBack));
				this._sequence.Join(this.text.DOScale(Vector3.zero, 0.2f).From(Vector3.one, true, false).SetEase(Ease.OutBack));
				this._sequence.Join(this.icon.DOScale(Vector3.zero, 0.2f).From(Vector3.one, true, false).SetEase(Ease.OutBack));
				this._isTransitioned = false;
			}
			else
			{
				this._sequence.Join(this.bg.DOScale(Vector3.one, 0.2f).From(Vector3.zero, true, false).SetEase(Ease.OutBack));
				this._sequence.Join(this.text.DOScale(Vector3.one, 0.2f).From(Vector3.zero, true, false).SetEase(Ease.OutBack));
				this._sequence.Join(this.icon.DOScale(Vector3.one, 0.2f).From(Vector3.zero, true, false).SetEase(Ease.OutBack));
				this.icon.localScale = Vector3.one;
				this.icon.localRotation = Quaternion.Euler(0f, 0f, 0f);
				if (this.isDrawZone)
				{
					this._loopTween = this.icon.DOScale(new Vector3(-1f, 1f, 1f), 0.9f).From(Vector3.one, true, false).SetLoops(-1, LoopType.Yoyo);
				}
				else
				{
					this._loopTween = this.icon.DORotate(new Vector3(0f, 0f, 360f), 1.6f, RotateMode.FastBeyond360).From(new Vector3(0f, 0f, 0f), true, false).SetEase(Ease.Linear)
						.SetLoops(-1, LoopType.Restart);
				}
			}
			this._sequence.SetUpdate(true).SetLink(base.transform.gameObject).SetTarget(base.transform);
			this._loopTween.SetUpdate(true).SetLink(base.transform.gameObject).SetTarget(base.transform);
		}

		// Token: 0x04000249 RID: 585
		[SerializeField]
		private Transform bg;

		// Token: 0x0400024A RID: 586
		[SerializeField]
		private Transform icon;

		// Token: 0x0400024B RID: 587
		[SerializeField]
		private Transform text;

		// Token: 0x0400024C RID: 588
		[SerializeField]
		private bool isDrawZone;

		// Token: 0x0400024D RID: 589
		private Sequence _sequence;

		// Token: 0x0400024E RID: 590
		private Tween _loopTween;

		// Token: 0x0400024F RID: 591
		private bool _isTransitioned;

		// Token: 0x04000250 RID: 592
		private bool _isActive;
	}
}
