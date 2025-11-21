using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Core.Cards;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;

namespace LBoL.Presentation.UI.ExtraWidgets
{
	// Token: 0x020000C6 RID: 198
	public class CardsRow : RecyclableScrollRectWidget.RecyclableCell
	{
		// Token: 0x06000C10 RID: 3088 RVA: 0x0003E408 File Offset: 0x0003C608
		public void SetCards(int index, IEnumerable<Card> cards)
		{
			this._tempCards = cards;
			this.root.DOFade(1f, 0.2f).From(0f, true, false).SetDelay(0.05f)
				.OnPlay(delegate
				{
					this.SetCardsDelay(index);
				});
		}

		// Token: 0x06000C11 RID: 3089 RVA: 0x0003E470 File Offset: 0x0003C670
		private void SetCardsDelay(int index)
		{
			int num = 0;
			using (IEnumerator<Card> enumerator = this._tempCards.GetEnumerator())
			{
				while (enumerator.MoveNext() && num < this.cardsWidgets.Length)
				{
					CardWidget cardWidget = this.cardsWidgets[num];
					bool flag = MuseumPanel.IsCardRevealed(enumerator.Current);
					Card card = enumerator.Current;
					bool flag2 = card != null && card.Config.DebugLevel == 1 && !GameMaster.ShowAllCardsInMuseum;
					if (GameMaster.ShowAllCardsInMuseum)
					{
						flag2 = false;
					}
					cardWidget.TooltipEnabled = flag;
					cardWidget.NotReveal = !flag;
					cardWidget.NotReadyInMuseum = flag2;
					cardWidget.GetComponent<ShowingCard>().enabled = flag && !flag2;
					cardWidget.Card = enumerator.Current;
					GameObject gameObject = this.cardLockMaskList[num];
					Card card2 = enumerator.Current;
					gameObject.SetActive(MuseumPanel.IsCardLocked((card2 != null) ? card2.Id : null));
					cardWidget.name = string.Format("Card[{0},{1}] ({2})", index, num, enumerator.Current.DebugName);
					GameObject gameObject2 = cardWidget.gameObject;
					Card card3 = enumerator.Current;
					gameObject2.SetActive(!MuseumPanel.IsCardLocked((card3 != null) ? card3.Id : null));
					cardWidget.GetComponent<ShowingCard>().SetScale(0.8f, 0.95f);
					num++;
				}
				goto IL_018B;
			}
			IL_013F:
			CardWidget cardWidget2 = this.cardsWidgets[num];
			cardWidget2.Card = null;
			cardWidget2.name = string.Format("Card[{0},{1}] (null)", index, num);
			cardWidget2.gameObject.SetActive(false);
			this.cardLockMaskList[num].SetActive(false);
			num++;
			IL_018B:
			if (num >= this.cardsWidgets.Length)
			{
				return;
			}
			goto IL_013F;
		}

		// Token: 0x06000C12 RID: 3090 RVA: 0x0003E630 File Offset: 0x0003C830
		public override void actionOnGet()
		{
			base.gameObject.SetActive(true);
		}

		// Token: 0x06000C13 RID: 3091 RVA: 0x0003E63E File Offset: 0x0003C83E
		public override void actionOnRelease()
		{
			this.root.alpha = 0f;
			this.root.DOKill(false);
			base.gameObject.SetActive(false);
		}

		// Token: 0x06000C14 RID: 3092 RVA: 0x0003E669 File Offset: 0x0003C869
		public override void actionOnDestroy()
		{
			Object.Destroy(base.transform.gameObject);
		}

		// Token: 0x06000C15 RID: 3093 RVA: 0x0003E67C File Offset: 0x0003C87C
		public override void ShowWithDelay(float delay)
		{
			this.root.gameObject.SetActive(false);
			this.root.DOFade(1f, 0.2f).From(0f, true, false).SetDelay(delay)
				.OnPlay(delegate
				{
					this.root.gameObject.SetActive(true);
				});
		}

		// Token: 0x0400095B RID: 2395
		[SerializeField]
		private CardWidget[] cardsWidgets;

		// Token: 0x0400095C RID: 2396
		[SerializeField]
		private CanvasGroup root;

		// Token: 0x0400095D RID: 2397
		[SerializeField]
		private List<GameObject> cardLockMaskList;

		// Token: 0x0400095E RID: 2398
		private IEnumerable<Card> _tempCards;

		// Token: 0x0400095F RID: 2399
		private const float DelayTime = 0.05f;
	}
}
