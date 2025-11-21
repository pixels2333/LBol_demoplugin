using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace LBoL.Presentation.UI.Transitions
{
	// Token: 0x0200007E RID: 126
	public class BossExhibitTransition : UiTransition
	{
		// Token: 0x06000686 RID: 1670 RVA: 0x0001C0A4 File Offset: 0x0001A2A4
		public void Awake()
		{
			base.GetComponentInChildren<UILineConnector>().enabled = true;
		}

		// Token: 0x06000687 RID: 1671 RVA: 0x0001C0B4 File Offset: 0x0001A2B4
		public override void Animate(Transform target, bool isOut, Action onComplete)
		{
			target.DOKill(false);
			GameObject gameObject = target.gameObject;
			CanvasGroup component = target.GetComponent<CanvasGroup>();
			if (component)
			{
				component.DOKill(false);
			}
			if (isOut)
			{
				component.DOFade(0f, 0.4f).From(1f, true, false).OnComplete(delegate
				{
					onComplete.Invoke();
				});
				return;
			}
			component.DOFade(1f, 0.3f).From(0f, true, false).SetUpdate(true)
				.SetLink(gameObject);
			this.ring.GetComponent<Image>().DOFade(1f, 1.6f).From(0.2f, true, false)
				.SetUpdate(true)
				.SetLink(gameObject);
			this.ring.DOScale(1f, 1f).From(3f, true, false).SetUpdate(true)
				.SetLink(gameObject);
			this.ring.DOLocalRotate(new Vector3(0f, 0f, -90f), 6f, RotateMode.FastBeyond360).From(Vector3.zero, true, false).SetLoops(-1, LoopType.Incremental)
				.SetEase(Ease.Linear)
				.SetUpdate(true)
				.SetLink(gameObject);
			this.circle.DOLocalRotate(Vector3.zero, 1.2f, RotateMode.FastBeyond360).From(new Vector3(0f, 0f, 210f), true, false).SetUpdate(true)
				.SetLink(gameObject);
			this.circle.DOScaleX(1f, 1f).From(0.3f, true, false).SetUpdate(true)
				.SetLink(gameObject);
			this.balance.DOLocalMoveY(0f, 1.2f, false).From(300f, true, false).SetEase(Ease.OutSine)
				.SetUpdate(true)
				.SetLink(gameObject);
			this.balance.GetComponent<Image>().DOFade(1f, 1.4f).From(0.2f, true, false)
				.SetUpdate(true)
				.SetLink(gameObject);
			this.trayLeft.DOLocalMoveY(-250f, 1.2f, false).From(-700f, true, false).SetEase(Ease.OutSine)
				.SetUpdate(true)
				.SetLink(gameObject);
			this.trayRight.DOLocalMoveY(-250f, 1.2f, false).From(-700f, true, false).SetEase(Ease.OutSine)
				.SetUpdate(true)
				.SetLink(gameObject);
			this.trayLeft.GetComponent<Image>().DOFade(1f, 1.4f).From(0.2f, true, false)
				.SetUpdate(true)
				.SetLink(gameObject);
			this.trayRight.GetComponent<Image>().DOFade(1f, 1.4f).From(0.2f, true, false)
				.SetUpdate(true)
				.SetLink(gameObject);
			this.lightImage.DOLocalRotate(new Vector3(0f, 0f, 90f), 0.8f, RotateMode.FastBeyond360).From(Vector3.zero, true, false).SetEase(Ease.Linear)
				.SetUpdate(true)
				.SetLink(gameObject);
			this.lightImage.GetComponent<Image>().DOFade(0f, 0.3f).From(1f, true, false)
				.SetDelay(0.5f)
				.SetUpdate(true)
				.SetLink(gameObject);
			this.lightImage.GetComponent<Image>().DOFade(0.4f, 0.4f).SetDelay(0.8f)
				.SetUpdate(true)
				.SetLink(gameObject);
			this.lightImage2.GetComponent<Image>().DOFade(0f, 0f).SetUpdate(true)
				.SetLink(gameObject);
			this.lightImage2.GetComponent<Image>().DOFade(0.4f, 0.4f).SetDelay(0.8f)
				.SetUpdate(true)
				.SetLink(gameObject);
			this.lightImage.DOScale(2f, 0.6f).From(6f, true, false).SetUpdate(true)
				.SetLink(gameObject);
			this.lightImage.DOLocalRotate(new Vector3(0f, 0f, 90f), 4f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear)
				.SetDelay(0.8f)
				.SetUpdate(true)
				.SetLink(gameObject);
			this.lightImage2.DOLocalRotate(new Vector3(0f, 0f, -90f), 4f, RotateMode.FastBeyond360).From(Vector3.zero, true, false).SetLoops(-1, LoopType.Incremental)
				.SetEase(Ease.Linear)
				.SetDelay(0.8f)
				.SetUpdate(true)
				.SetLink(gameObject);
			this.rainbow.DOScale(1f, 1.2f).From(0.6f, true, false).SetUpdate(true)
				.SetLink(gameObject);
			this.rainbow.GetComponent<Image>().DOFade(0f, 1f).From(1f, true, false)
				.SetDelay(0.5f)
				.SetUpdate(true)
				.SetLink(gameObject);
			this.line.DOFade(1f, 0.6f).From(0f, true, false).SetDelay(0.5f)
				.SetUpdate(true)
				.SetLink(gameObject);
			this.line.enabled = true;
			DOTween.Sequence().AppendInterval(1f).AppendCallback(delegate
			{
				onComplete.Invoke();
			})
				.SetTarget(target)
				.SetUpdate(true)
				.SetLink(gameObject);
		}

		// Token: 0x06000688 RID: 1672 RVA: 0x0001C657 File Offset: 0x0001A857
		public override void Kill(Transform target)
		{
			target.DOKill(false);
		}

		// Token: 0x0400040A RID: 1034
		[Header("组件引用")]
		[SerializeField]
		private Transform ring;

		// Token: 0x0400040B RID: 1035
		[SerializeField]
		private Transform circle;

		// Token: 0x0400040C RID: 1036
		[SerializeField]
		private Transform balance;

		// Token: 0x0400040D RID: 1037
		[SerializeField]
		private Transform trayLeft;

		// Token: 0x0400040E RID: 1038
		[SerializeField]
		private Transform trayRight;

		// Token: 0x0400040F RID: 1039
		[SerializeField]
		private Transform lightImage;

		// Token: 0x04000410 RID: 1040
		[SerializeField]
		private Transform lightImage2;

		// Token: 0x04000411 RID: 1041
		[SerializeField]
		private Transform rainbow;

		// Token: 0x04000412 RID: 1042
		[SerializeField]
		private UILineRenderer line;
	}
}
