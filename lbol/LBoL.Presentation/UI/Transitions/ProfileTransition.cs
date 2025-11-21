using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;

namespace LBoL.Presentation.UI.Transitions
{
	// Token: 0x02000082 RID: 130
	public class ProfileTransition : UiTransition
	{
		// Token: 0x060006A2 RID: 1698 RVA: 0x0001CE4C File Offset: 0x0001B04C
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

		// Token: 0x060006A3 RID: 1699 RVA: 0x0001CF0E File Offset: 0x0001B10E
		public override void Kill(Transform target)
		{
			target.DOKill(false);
		}

		// Token: 0x04000429 RID: 1065
		[SerializeField]
		private ProfileWidget widget0;

		// Token: 0x0400042A RID: 1066
		[SerializeField]
		private ProfileWidget widget1;

		// Token: 0x0400042B RID: 1067
		[SerializeField]
		private ProfileWidget widget2;

		// Token: 0x0400042C RID: 1068
		[SerializeField]
		private float fadeTime;
	}
}
