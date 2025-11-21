using System;
using LBoL.Presentation.UI.Panels;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000056 RID: 86
	public class FastDeckViewButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		// Token: 0x060004FA RID: 1274 RVA: 0x000153C8 File Offset: 0x000135C8
		public void OnPointerEnter(PointerEventData eventData)
		{
			FastDeckViewButton.Deck deck = this.deck;
			if (deck == FastDeckViewButton.Deck.Draw)
			{
				this.cardUi.ShowDrawFastView();
				return;
			}
			if (deck != FastDeckViewButton.Deck.Discard)
			{
				throw new ArgumentOutOfRangeException();
			}
			this.cardUi.ShowDiscardFastView();
		}

		// Token: 0x060004FB RID: 1275 RVA: 0x00015404 File Offset: 0x00013604
		public void OnPointerExit(PointerEventData eventData)
		{
			FastDeckViewButton.Deck deck = this.deck;
			if (deck == FastDeckViewButton.Deck.Draw)
			{
				this.cardUi.HideDrawFastView();
				return;
			}
			if (deck != FastDeckViewButton.Deck.Discard)
			{
				throw new ArgumentOutOfRangeException();
			}
			this.cardUi.HideDiscardFastView();
		}

		// Token: 0x040002BB RID: 699
		[SerializeField]
		private FastDeckViewButton.Deck deck;

		// Token: 0x040002BC RID: 700
		[SerializeField]
		private CardUi cardUi;

		// Token: 0x020001D2 RID: 466
		private enum Deck
		{
			// Token: 0x04000F09 RID: 3849
			Draw,
			// Token: 0x04000F0A RID: 3850
			Discard
		}
	}
}
