using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Presentation.UI.Panels;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class MinimizedButtonWidget : CommonButtonWidget
	{
		public bool IsMinimized { get; private set; }
		public override void OnPointerClick(PointerEventData eventData)
		{
			base.OnPointerClick(eventData);
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				this.SwitchMinimized();
			}
		}
		public void SwitchMinimized()
		{
			if (this.IsMinimized)
			{
				this.minimizedRoot.alpha = 1f;
				this.minimizedRoot.blocksRaycasts = true;
				this.tipText.SetActive(false);
				this.icon.sprite = this.onSprite;
				UiManager.GetPanel<PlayBoard>().IsTempLockedFromMinimize = false;
				UiManager.GetPanel<VnPanel>().IsTempLocked = false;
				UiManager.GetPanel<ShopPanel>().LockedByInteractionMinimized = false;
				this.backIcon.DOFade(0f, 1f).From(1f, true, false).SetLoops(-1)
					.SetTarget(this.icon)
					.SetUpdate(true)
					.SetLink(base.gameObject);
				this.backIcon.transform.DOScale(1.8f, 1f).From(1f, true, false).SetLoops(-1)
					.SetTarget(this.icon)
					.SetUpdate(true)
					.SetLink(base.gameObject);
				this.backIcon.gameObject.SetActive(true);
			}
			else
			{
				this.minimizedRoot.alpha = 0f;
				this.minimizedRoot.blocksRaycasts = false;
				this.tipText.SetActive(true);
				this.icon.sprite = this.offSprite;
				UiManager.GetPanel<PlayBoard>().IsTempLockedFromMinimize = true;
				UiManager.GetPanel<VnPanel>().IsTempLocked = true;
				UiManager.GetPanel<ShopPanel>().LockedByInteractionMinimized = true;
				this.icon.DOKill(false);
				this.backIcon.gameObject.SetActive(false);
			}
			Cursor.visible = true;
			this.IsMinimized = !this.IsMinimized;
		}
		public void Init()
		{
			this.icon.DOKill(false);
			this.IsMinimized = false;
			this.minimizedRoot.alpha = 1f;
			this.minimizedRoot.blocksRaycasts = true;
			this.tipText.SetActive(false);
			this.icon.sprite = this.onSprite;
			UiManager.GetPanel<PlayBoard>().IsTempLockedFromMinimize = false;
			UiManager.GetPanel<VnPanel>().IsTempLocked = false;
			UiManager.GetPanel<ShopPanel>().LockedByInteractionMinimized = false;
			this.backIcon.DOFade(0f, 1f).From(1f, true, false).SetLoops(-1)
				.SetTarget(this.icon)
				.SetUpdate(true)
				.SetLink(base.gameObject);
			this.backIcon.transform.DOScale(1.8f, 1f).From(1f, true, false).SetLoops(-1)
				.SetTarget(this.icon)
				.SetUpdate(true)
				.SetLink(base.gameObject);
			this.backIcon.gameObject.SetActive(true);
		}
		[SerializeField]
		private Image icon;
		[SerializeField]
		private Image backIcon;
		[SerializeField]
		private GameObject tipText;
		[SerializeField]
		private CanvasGroup minimizedRoot;
		[SerializeField]
		private Sprite onSprite;
		[SerializeField]
		private Sprite offSprite;
	}
}
