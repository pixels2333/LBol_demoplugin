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
	public class BgmHint : MonoBehaviour
	{
		private void Awake()
		{
			this.canvasGroup.alpha = 0f;
		}
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
		[SerializeField]
		private CanvasGroup canvasGroup;
		[SerializeField]
		private Transform actor;
		[SerializeField]
		private TextMeshProUGUI text;
		[SerializeField]
		private TextMeshProUGUI subText;
		[SerializeField]
		private float distance = 900f;
	}
}
