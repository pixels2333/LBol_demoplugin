using System;
using LBoL.Core;
using LBoL.Core.Cards;
using TMPro;
using UnityEngine;
namespace LBoL.Presentation.UI.Widgets
{
	public class CardInFastView : MonoBehaviour
	{
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
		[SerializeField]
		private CardWidget cardWidget;
		[SerializeField]
		private TextMeshProUGUI cardCount;
	}
}
