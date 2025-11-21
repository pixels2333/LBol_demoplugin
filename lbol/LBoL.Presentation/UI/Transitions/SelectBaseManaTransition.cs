using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Transitions
{
	// Token: 0x02000083 RID: 131
	public class SelectBaseManaTransition : UiTransition
	{
		// Token: 0x060006A5 RID: 1701 RVA: 0x0001CF20 File Offset: 0x0001B120
		public override void Animate(Transform target, bool isOut, Action onComplete)
		{
			target.DOKill(false);
			if (!isOut)
			{
				base.GetComponent<CanvasGroup>().DOFade(1f, this.fadeTime).From(0f, true, false)
					.OnComplete(delegate
					{
						onComplete.Invoke();
					});
				this.ringGroup.DOKill(false);
				this.ringGroup.DOScale(1f, this.fadeTime * 2f).From(0f, true, false).SetLink(base.gameObject);
				this.ring1.transform.DOKill(false);
				this.ring1.transform.DOLocalRotate(new Vector3(0f, 0f, 360f), 50f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear)
					.SetLink(base.gameObject);
				this.ring2.transform.DOKill(false);
				this.ring2.transform.DOLocalRotate(new Vector3(0f, 0f, 360f), 40f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear)
					.SetLink(base.gameObject);
				this.ring3.transform.DOKill(false);
				this.ring3.transform.DOLocalRotate(new Vector3(0f, 0f, -360f), 50f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear)
					.SetLink(base.gameObject);
				this.ring4.transform.DOKill(false);
				this.ring4.transform.DOLocalRotate(new Vector3(0f, 0f, -360f), 40f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear)
					.SetLink(base.gameObject);
				return;
			}
			base.GetComponent<CanvasGroup>().DOFade(0f, this.fadeTime).From(1f, true, false)
				.OnComplete(delegate
				{
					onComplete.Invoke();
				});
		}

		// Token: 0x060006A6 RID: 1702 RVA: 0x0001D146 File Offset: 0x0001B346
		public override void Kill(Transform target)
		{
			target.DOKill(false);
		}

		// Token: 0x0400042D RID: 1069
		[SerializeField]
		private Image ring1;

		// Token: 0x0400042E RID: 1070
		[SerializeField]
		private Image ring2;

		// Token: 0x0400042F RID: 1071
		[SerializeField]
		private Image ring3;

		// Token: 0x04000430 RID: 1072
		[SerializeField]
		private Image ring4;

		// Token: 0x04000431 RID: 1073
		[SerializeField]
		private ParticleSystem particle;

		// Token: 0x04000432 RID: 1074
		[SerializeField]
		private Transform ringGroup;

		// Token: 0x04000433 RID: 1075
		[SerializeField]
		private float fadeTime;
	}
}
