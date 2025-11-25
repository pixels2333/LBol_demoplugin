using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
namespace LBoL.Presentation.UI.Widgets
{
	public class CardZoneUpperCountWidget : MonoBehaviour
	{
		public void Awake()
		{
			base.gameObject.SetActive(false);
			this._isActive = false;
			this._isTransitioned = false;
		}
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
		[SerializeField]
		private Transform bg;
		[SerializeField]
		private Transform icon;
		[SerializeField]
		private Transform text;
		[SerializeField]
		private bool isDrawZone;
		private Sequence _sequence;
		private Tween _loopTween;
		private bool _isTransitioned;
		private bool _isActive;
	}
}
