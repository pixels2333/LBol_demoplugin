using System;
using LBoL.Core;
using LBoL.Core.Cards;
using TMPro;
using UnityEngine;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000042 RID: 66
	public class CardInFastView : MonoBehaviour
	{
		// Token: 0x06000428 RID: 1064 RVA: 0x00010B3C File Offset: 0x0000ED3C
		public void SetCard(Card card, int count, bool isOthers)
		{
			this.cardWidget.Card = card;
			if (isOthers)
			{
				this.cardCount.text = "Game.OtherCards".Localize(true) + string.Format("x{0}", count);
				return;
			}
			this.cardCount.text = ((count > 1) ? string.Format("x{0}", count) : null);
		}

		// Token: 0x040001F7 RID: 503
		[SerializeField]
		private CardWidget cardWidget;

		// Token: 0x040001F8 RID: 504
		[SerializeField]
		private TextMeshProUGUI cardCount;
	}
}
