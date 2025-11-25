using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
namespace LBoL.Presentation.UI.Widgets
{
	public class SideTipWidget : MonoBehaviour
	{
		private void Awake()
		{
			this._text = base.GetComponent<TextMeshProUGUI>();
		}
		private void OnEnable()
		{
			if (this._text)
			{
				this._text.alpha = 0.8f;
				this._sequence = DOTween.Sequence().AppendInterval(1f).Append(this._text.DOFade(0.1f, 1f).SetEase(Ease.OutSine))
					.Append(this._text.DOFade(0.8f, 1f).SetEase(Ease.InSine))
					.SetUpdate(true)
					.SetLoops(-1, LoopType.Restart);
			}
		}
		private void OnDisable()
		{
			Sequence sequence = this._sequence;
			if (sequence == null)
			{
				return;
			}
			sequence.Kill(false);
		}
		private TextMeshProUGUI _text;
		private Sequence _sequence;
		private const float MaxFade = 0.8f;
		private const float MinFade = 0.1f;
		private const float MaxPauseTime = 1f;
		private const float FadeToMinTime = 1f;
		private const float FadeToMaxTime = 1f;
	}
}
