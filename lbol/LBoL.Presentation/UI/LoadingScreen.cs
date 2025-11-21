using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace LBoL.Presentation.UI
{
	// Token: 0x02000021 RID: 33
	public class LoadingScreen : MonoBehaviour
	{
		// Token: 0x06000311 RID: 785 RVA: 0x0000D570 File Offset: 0x0000B770
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

		// Token: 0x06000312 RID: 786 RVA: 0x0000D5BC File Offset: 0x0000B7BC
		public async UniTask Hide(float duration = 0.5f)
		{
			await this.root.DOFade(0f, duration).From(1f, true, false);
			this.wave.DOKill(false);
			this.root.gameObject.SetActive(false);
		}

		// Token: 0x04000142 RID: 322
		[SerializeField]
		private CanvasGroup root;

		// Token: 0x04000143 RID: 323
		[SerializeField]
		private RectTransform wave;

		// Token: 0x04000144 RID: 324
		[SerializeField]
		private Transform logoGroup;
	}
}
