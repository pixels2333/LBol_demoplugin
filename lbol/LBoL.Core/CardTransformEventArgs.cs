using System;
using LBoL.Core.Cards;
namespace LBoL.Core
{
	public class CardTransformEventArgs : GameEventArgs
	{
		public Card SourceCard { get; internal set; }
		public Card DestinationCard { get; internal set; }
		protected override string GetBaseDebugString()
		{
			return "CardTransform: " + GameEventArgs.DebugString(this.SourceCard) + " -> " + GameEventArgs.DebugString(this.DestinationCard);
		}
	}
}
