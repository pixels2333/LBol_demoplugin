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
	public class CardsRow : RecyclableScrollRectWidget.RecyclableCell
	{
		public void SetCards(int index, IEnumerable<Card> cards)
		{
			this._tempCards = cards;
			this.root.DOFade(1f, 0.2f).From(0f, true, false).SetDelay(0.05f)
				.OnPlay(delegate
				{
					this.SetCardsDelay(index);
				});
		}
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
		public override void actionOnGet()
		{
			base.gameObject.SetActive(true);
		}
		public override void actionOnRelease()
		{
			this.root.alpha = 0f;
			this.root.DOKill(false);
			base.gameObject.SetActive(false);
		}
		public override void actionOnDestroy()
		{
			Object.Destroy(base.transform.gameObject);
		}
		public override void ShowWithDelay(float delay)
		{
			this.root.gameObject.SetActive(false);
			this.root.DOFade(1f, 0.2f).From(0f, true, false).SetDelay(delay)
				.OnPlay(delegate
				{
					this.root.gameObject.SetActive(true);
				});
		}
		[SerializeField]
		private CardWidget[] cardsWidgets;
		[SerializeField]
		private CanvasGroup root;
		[SerializeField]
		private List<GameObject> cardLockMaskList;
		private IEnumerable<Card> _tempCards;
		private const float DelayTime = 0.05f;
	}
}
