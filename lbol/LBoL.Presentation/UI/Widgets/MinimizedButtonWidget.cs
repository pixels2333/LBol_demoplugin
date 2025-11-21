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
	// Token: 0x02000065 RID: 101
	public class MinimizedButtonWidget : CommonButtonWidget
	{
		// Token: 0x170000F2 RID: 242
		// (get) Token: 0x06000583 RID: 1411 RVA: 0x00017D72 File Offset: 0x00015F72
		// (set) Token: 0x06000584 RID: 1412 RVA: 0x00017D7A File Offset: 0x00015F7A
		public bool IsMinimized { get; private set; }

		// Token: 0x06000585 RID: 1413 RVA: 0x00017D83 File Offset: 0x00015F83
		public override void OnPointerClick(PointerEventData eventData)
		{
			base.OnPointerClick(eventData);
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				this.SwitchMinimized();
			}
		}

		// Token: 0x06000586 RID: 1414 RVA: 0x00017D9C File Offset: 0x00015F9C
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

		// Token: 0x06000587 RID: 1415 RVA: 0x00017F3C File Offset: 0x0001613C
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

		// Token: 0x04000341 RID: 833
		[SerializeField]
		private Image icon;

		// Token: 0x04000342 RID: 834
		[SerializeField]
		private Image backIcon;

		// Token: 0x04000343 RID: 835
		[SerializeField]
		private GameObject tipText;

		// Token: 0x04000344 RID: 836
		[SerializeField]
		private CanvasGroup minimizedRoot;

		// Token: 0x04000345 RID: 837
		[SerializeField]
		private Sprite onSprite;

		// Token: 0x04000346 RID: 838
		[SerializeField]
		private Sprite offSprite;
	}
}
