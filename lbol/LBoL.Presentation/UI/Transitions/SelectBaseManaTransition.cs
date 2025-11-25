using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Transitions
{
	public class SelectBaseManaTransition : UiTransition
	{
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
		public override void Kill(Transform target)
		{
			target.DOKill(false);
		}
		[SerializeField]
		private Image ring1;
		[SerializeField]
		private Image ring2;
		[SerializeField]
		private Image ring3;
		[SerializeField]
		private Image ring4;
		[SerializeField]
		private ParticleSystem particle;
		[SerializeField]
		private Transform ringGroup;
		[SerializeField]
		private float fadeTime;
	}
}
