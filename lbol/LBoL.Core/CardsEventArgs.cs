using System;
using System.Linq;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;
namespace LBoL.Core
{
	public class CardsEventArgs : GameEventArgs
	{
		public Card[] Cards { get; set; }
		protected override string GetBaseDebugString()
		{
			return "Cards = [" + ", ".Join(Enumerable.Select<Card, string>(this.Cards, new Func<Card, string>(GameEventArgs.DebugString))) + "]";
		}
	}
}
