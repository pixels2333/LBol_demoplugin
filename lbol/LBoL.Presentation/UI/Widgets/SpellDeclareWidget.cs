using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000072 RID: 114
	public class SpellDeclareWidget : MonoBehaviour
	{
		// Token: 0x060005E5 RID: 1509 RVA: 0x000197C0 File Offset: 0x000179C0
		public void DoAnimationIn()
		{
			this.portraitRoot.DOAnchorPos(new Vector2(1500f, -1000f), 1.5f, false).From<TweenerCore<Vector2, Vector2, VectorOptions>>().SetRelative<TweenerCore<Vector2, Vector2, VectorOptions>>()
				.SetEase(Ease.OutCubic);
			base.gameObject.GetOrAddComponent<CanvasGroup>().DOFade(0f, 0.4f).From(1f, true, false)
				.SetDelay(2f);
		}

		// Token: 0x04000398 RID: 920
		[SerializeField]
		private RectTransform portraitRoot;
	}
}
