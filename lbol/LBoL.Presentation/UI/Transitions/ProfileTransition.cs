using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
namespace LBoL.Presentation.UI.Transitions
{
	public class ProfileTransition : UiTransition
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
			}
			else
			{
				base.GetComponent<CanvasGroup>().DOFade(0f, this.fadeTime).From(1f, true, false)
					.OnComplete(delegate
					{
						onComplete.Invoke();
					});
			}
			this.widget0.Animate(0f, isOut);
			this.widget1.Animate(0.1f, isOut);
			this.widget2.Animate(0.2f, isOut);
		}
		public override void Kill(Transform target)
		{
			target.DOKill(false);
		}
		[SerializeField]
		private ProfileWidget widget0;
		[SerializeField]
		private ProfileWidget widget1;
		[SerializeField]
		private ProfileWidget widget2;
		[SerializeField]
		private float fadeTime;
	}
}
