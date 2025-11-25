using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
namespace LBoL.Presentation.UI.Transitions
{
	public class BossExhibitTransition : UiTransition
	{
		public void Awake()
		{
			base.GetComponentInChildren<UILineConnector>().enabled = true;
		}
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
		public override void Kill(Transform target)
		{
			target.DOKill(false);
		}
		[Header("组件引用")]
		[SerializeField]
		private Transform ring;
		[SerializeField]
		private Transform circle;
		[SerializeField]
		private Transform balance;
		[SerializeField]
		private Transform trayLeft;
		[SerializeField]
		private Transform trayRight;
		[SerializeField]
		private Transform lightImage;
		[SerializeField]
		private Transform lightImage2;
		[SerializeField]
		private Transform rainbow;
		[SerializeField]
		private UILineRenderer line;
	}
}
