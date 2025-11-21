using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000064 RID: 100
	public class MapPageChangeWidget : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		// Token: 0x0600057E RID: 1406 RVA: 0x00017BC3 File Offset: 0x00015DC3
		private void Awake()
		{
			this.signal.transform.localScale = Vector3.zero;
		}

		// Token: 0x0600057F RID: 1407 RVA: 0x00017BDC File Offset: 0x00015DDC
		public void Animate(bool active)
		{
			this.OnPointerExit(null);
			if (active && !this.isActive)
			{
				this.signal.transform.DOScale(1f, 0.2f).From(0f, true, false);
			}
			if (!active && this.isActive)
			{
				this.signal.transform.DOScale(0f, 0.2f).From(1f, true, false);
			}
			this.isActive = active;
		}

		// Token: 0x06000580 RID: 1408 RVA: 0x00017C5C File Offset: 0x00015E5C
		public void OnPointerEnter(PointerEventData eventData)
		{
			if (this.isActive)
			{
				return;
			}
			this.isEnter = true;
			this.signal.transform.DOScale(0.6f, 0.1f).From(0f, true, false);
			this.signal.DOFade(0.5f, 0f);
			this.border.transform.DOScale(1.2f, 0.1f).From(1f, true, false);
		}

		// Token: 0x06000581 RID: 1409 RVA: 0x00017CE0 File Offset: 0x00015EE0
		public void OnPointerExit(PointerEventData eventData)
		{
			if (!this.isEnter)
			{
				return;
			}
			if (this.isActive)
			{
				return;
			}
			this.isEnter = false;
			this.signal.transform.DOScale(0f, 0.1f).From(0.6f, true, false);
			this.signal.DOFade(1f, 0f);
			this.border.transform.DOScale(1f, 0.1f).From(1.2f, true, false);
		}

		// Token: 0x0400033C RID: 828
		[SerializeField]
		private Image signal;

		// Token: 0x0400033D RID: 829
		[SerializeField]
		private Image border;

		// Token: 0x0400033E RID: 830
		[SerializeField]
		public Button button;

		// Token: 0x0400033F RID: 831
		[SerializeField]
		private bool isActive;

		// Token: 0x04000340 RID: 832
		[SerializeField]
		private bool isEnter;
	}
}
