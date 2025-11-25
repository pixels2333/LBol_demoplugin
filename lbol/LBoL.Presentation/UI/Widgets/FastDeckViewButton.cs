using System;
using LBoL.Presentation.UI.Panels;
using UnityEngine;
using UnityEngine.EventSystems;
namespace LBoL.Presentation.UI.Widgets
{
	public class FastDeckViewButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
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
		[SerializeField]
		private FastDeckViewButton.Deck deck;
		[SerializeField]
		private CardUi cardUi;
		private enum Deck
		{
			Draw,
			Discard
		}
	}
}
