using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000071 RID: 113
	public class SideTipWidget : MonoBehaviour
	{
		// Token: 0x060005E1 RID: 1505 RVA: 0x00019708 File Offset: 0x00017908
		private void Awake()
		{
			this._text = base.GetComponent<TextMeshProUGUI>();
		}

		// Token: 0x060005E2 RID: 1506 RVA: 0x00019718 File Offset: 0x00017918
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

		// Token: 0x060005E3 RID: 1507 RVA: 0x000197A4 File Offset: 0x000179A4
		private void OnDisable()
		{
			Sequence sequence = this._sequence;
			if (sequence == null)
			{
				return;
			}
			sequence.Kill(false);
		}

		// Token: 0x04000391 RID: 913
		private TextMeshProUGUI _text;

		// Token: 0x04000392 RID: 914
		private Sequence _sequence;

		// Token: 0x04000393 RID: 915
		private const float MaxFade = 0.8f;

		// Token: 0x04000394 RID: 916
		private const float MinFade = 0.1f;

		// Token: 0x04000395 RID: 917
		private const float MaxPauseTime = 1f;

		// Token: 0x04000396 RID: 918
		private const float FadeToMinTime = 1f;

		// Token: 0x04000397 RID: 919
		private const float FadeToMaxTime = 1f;
	}
}
