using System;
using LBoL.Core.Cards;
namespace LBoL.Core
{
	public class CardEventArgs : GameEventArgs
	{
		public Card Card { get; internal set; }
		protected override string GetBaseDebugString()
		{
			return "Card = " + GameEventArgs.DebugString(this.Card);
		}
	}
}
