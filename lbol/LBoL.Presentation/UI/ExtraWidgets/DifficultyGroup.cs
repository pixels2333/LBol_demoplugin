using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
namespace LBoL.Presentation.UI.ExtraWidgets
{
	public sealed class DifficultyGroup : MonoBehaviour
	{
		private CanvasGroup GetCanvasGroup()
		{
			if (this._canvasGroup)
			{
				return this._canvasGroup;
			}
			this._canvasGroup = base.gameObject.GetOrAddComponent<CanvasGroup>();
			return this._canvasGroup;
		}
		private void Awake()
		{
			SimpleTooltipSource.CreateWithGeneralKey(this.lockIcon.gameObject, "StartGame.NeedClear", null).WithPosition(TooltipDirection.Bottom, TooltipAlignment.Center);
		}
		public void SetDifficultyActive(bool active)
		{
			base.gameObject.SetActive(active);
			base.transform.localPosition = Vector3.zero;
			this.GetCanvasGroup().alpha = 1f;
		}
		public void SetLocked(bool locked)
		{
			this.lockIcon.gameObject.SetActive(locked);
			this.icon.color = (locked ? DifficultyGroup.LockedColor : Color.white);
		}
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
		public void FadeIn(float fromX, float duration)
		{
			this.DOKill(false);
			DOTween.Sequence().Join(base.transform.DOLocalMoveX(0f, duration, false).From(fromX, true, false).SetEase(Ease.OutCubic)).Join(this.GetCanvasGroup().DOFade(1f, duration).From(0f, true, false)
				.SetEase(Ease.OutCubic))
				.SetUpdate(true)
				.SetTarget(this)
				.SetLink(base.gameObject);
		}
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
		[SerializeField]
		private Image icon;
		[SerializeField]
		private Image clearIcon;
		[SerializeField]
		private Image pClearIcon;
		[SerializeField]
		private Image lockIcon;
		private CanvasGroup _canvasGroup;
		private static readonly Color LockedColor = new Color(0.5f, 0.5f, 0.5f);
	}
}
