using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
namespace LBoL.Presentation.UI
{
	public class LoadingScreen : MonoBehaviour
	{
		public async UniTask Show(float duration = 0.5f)
		{
			this.root.gameObject.SetActive(true);
			if (duration >= 0.5f)
			{
				this.logoGroup.gameObject.SetActive(true);
				this.wave.gameObject.SetActive(true);
				this.wave.DOAnchorPos(new Vector2(-3840f, 0f), 24f, false).From(Vector2.zero, true, false).SetLoops(-1, LoopType.Restart)
					.SetEase(Ease.Linear);
			}
			else
			{
				this.logoGroup.gameObject.SetActive(false);
				this.wave.gameObject.SetActive(false);
			}
			await this.root.DOFade(1f, duration).From(0f, true, false);
		}
		public async UniTask Hide(float duration = 0.5f)
		{
			await this.root.DOFade(0f, duration).From(1f, true, false);
			this.wave.DOKill(false);
			this.root.gameObject.SetActive(false);
		}
		[SerializeField]
		private CanvasGroup root;
		[SerializeField]
		private RectTransform wave;
		[SerializeField]
		private Transform logoGroup;
	}
}
