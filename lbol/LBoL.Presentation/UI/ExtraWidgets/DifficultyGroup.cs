using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace LBoL.Presentation.UI.ExtraWidgets
{
	// Token: 0x020000C8 RID: 200
	public sealed class DifficultyGroup : MonoBehaviour
	{
		// Token: 0x06000C1A RID: 3098 RVA: 0x0003E796 File Offset: 0x0003C996
		private CanvasGroup GetCanvasGroup()
		{
			if (this._canvasGroup)
			{
				return this._canvasGroup;
			}
			this._canvasGroup = base.gameObject.GetOrAddComponent<CanvasGroup>();
			return this._canvasGroup;
		}

		// Token: 0x06000C1B RID: 3099 RVA: 0x0003E7C3 File Offset: 0x0003C9C3
		private void Awake()
		{
			SimpleTooltipSource.CreateWithGeneralKey(this.lockIcon.gameObject, "StartGame.NeedClear", null).WithPosition(TooltipDirection.Bottom, TooltipAlignment.Center);
		}

		// Token: 0x06000C1C RID: 3100 RVA: 0x0003E7E3 File Offset: 0x0003C9E3
		public void SetDifficultyActive(bool active)
		{
			base.gameObject.SetActive(active);
			base.transform.localPosition = Vector3.zero;
			this.GetCanvasGroup().alpha = 1f;
		}

		// Token: 0x06000C1D RID: 3101 RVA: 0x0003E811 File Offset: 0x0003CA11
		public void SetLocked(bool locked)
		{
			this.lockIcon.gameObject.SetActive(locked);
			this.icon.color = (locked ? DifficultyGroup.LockedColor : Color.white);
		}

		// Token: 0x06000C1E RID: 3102 RVA: 0x0003E840 File Offset: 0x0003CA40
		public void SetClearState(ClearState state)
		{
			switch (state)
			{
			case ClearState.NotCleared:
				this.clearIcon.gameObject.SetActive(false);
				this.pClearIcon.gameObject.SetActive(false);
				return;
			case ClearState.Cleared:
				this.clearIcon.gameObject.SetActive(true);
				this.pClearIcon.gameObject.SetActive(false);
				return;
			case ClearState.PerfectCleared:
				this.clearIcon.gameObject.SetActive(false);
				this.pClearIcon.gameObject.SetActive(true);
				return;
			default:
				throw new ArgumentOutOfRangeException("state", state, null);
			}
		}

		// Token: 0x06000C1F RID: 3103 RVA: 0x0003E8DC File Offset: 0x0003CADC
		public void FadeIn(float fromX, float duration)
		{
			this.DOKill(false);
			DOTween.Sequence().Join(base.transform.DOLocalMoveX(0f, duration, false).From(fromX, true, false).SetEase(Ease.OutCubic)).Join(this.GetCanvasGroup().DOFade(1f, duration).From(0f, true, false)
				.SetEase(Ease.OutCubic))
				.SetUpdate(true)
				.SetTarget(this)
				.SetLink(base.gameObject);
		}

		// Token: 0x06000C20 RID: 3104 RVA: 0x0003E960 File Offset: 0x0003CB60
		public void FadeOut(float toX, float duration)
		{
			this.DOKill(false);
			DOTween.Sequence().Join(base.transform.DOLocalMoveX(toX, duration, false).From(0f, true, false).SetEase(Ease.OutCubic)).Join(this.GetCanvasGroup().DOFade(0f, duration).From(1f, true, false)
				.SetEase(Ease.OutCubic))
				.OnComplete(delegate
				{
					base.gameObject.SetActive(false);
				})
				.SetUpdate(true)
				.SetTarget(this)
				.SetLink(base.gameObject);
		}

		// Token: 0x04000962 RID: 2402
		[SerializeField]
		private Image icon;

		// Token: 0x04000963 RID: 2403
		[SerializeField]
		private Image clearIcon;

		// Token: 0x04000964 RID: 2404
		[SerializeField]
		private Image pClearIcon;

		// Token: 0x04000965 RID: 2405
		[SerializeField]
		private Image lockIcon;

		// Token: 0x04000966 RID: 2406
		private CanvasGroup _canvasGroup;

		// Token: 0x04000967 RID: 2407
		private static readonly Color LockedColor = new Color(0.5f, 0.5f, 0.5f);
	}
}
