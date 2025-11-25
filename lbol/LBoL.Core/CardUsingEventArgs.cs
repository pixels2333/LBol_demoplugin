using System;
using LBoL.Base;
using LBoL.Core.Cards;
namespace LBoL.Core
{
	public class CardUsingEventArgs : GameEventArgs
	{
		public Card Card { get; internal set; }
		public UnitSelector Selector { get; internal set; }
		public ManaGroup ConsumingMana { get; internal set; }
		public bool PlayTwice { get; set; }
		public bool Kicker { get; set; }
		public CardUsingEventArgs Clone()
		{
			return new CardUsingEventArgs
			{
				Card = this.Card,
				Selector = this.Selector,
				ConsumingMana = this.ConsumingMana,
				PlayTwice = this.PlayTwice,
				Kicker = this.Kicker
			};
		}
		protected override string GetBaseDebugString()
		{
			return GameEventArgs.DebugString(this.Card) + " -> {" + GameEventArgs.DebugString(this.Selector) + "}";
		}
	}
}
