using System;
using System.Linq;
using LBoL.Core.Cards;
namespace LBoL.Core
{
	public class CardsAddingToDrawZoneEventArgs : GameEventArgs
	{
		public Card[] Cards { get; internal set; }
		public DrawZoneTarget DrawZoneTarget { get; internal set; }
		protected override string GetBaseDebugString()
		{
			return string.Format("Cards = [{0}], -> {1}", string.Join(", ", Enumerable.Select<Card, string>(this.Cards, new Func<Card, string>(GameEventArgs.DebugString))), this.DrawZoneTarget);
		}
	}
}
