using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.ConfigData;
using LBoL.Core;
using TMPro;
using UnityEngine;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000040 RID: 64
	public class BgmHint : MonoBehaviour
	{
		// Token: 0x0600041D RID: 1053 RVA: 0x0001097C File Offset: 0x0000EB7C
		private void Awake()
		{
			this.canvasGroup.alpha = 0f;
		}

		// Token: 0x0600041E RID: 1054 RVA: 0x00010990 File Offset: 0x0000EB90
		public void ShowHint(BgmConfig config)
		{
			this.DOKill(false);
			this.text.text = config.TrackName;
			this.subText.text = "TopInfo.Original".Localize(true) + config.Original;
			this.canvasGroup.alpha = 1f;
			this.actor.localPosition = new Vector3(this.distance, 0f, 0f);
			DOTween.Sequence().Append(this.actor.DOLocalMoveX(0f, 0.5f, false).SetEase(Ease.OutCubic)).AppendInterval(3f)
				.Append(this.canvasGroup.DOFade(0f, 0.5f))
				.SetLink(base.gameObject)
				.SetUpdate(true)
				.SetTarget(this);
		}

		// Token: 0x040001EF RID: 495
		[SerializeField]
		private CanvasGroup canvasGroup;

		// Token: 0x040001F0 RID: 496
		[SerializeField]
		private Transform actor;

		// Token: 0x040001F1 RID: 497
		[SerializeField]
		private TextMeshProUGUI text;

		// Token: 0x040001F2 RID: 498
		[SerializeField]
		private TextMeshProUGUI subText;

		// Token: 0x040001F3 RID: 499
		[SerializeField]
		private float distance = 900f;
	}
}
