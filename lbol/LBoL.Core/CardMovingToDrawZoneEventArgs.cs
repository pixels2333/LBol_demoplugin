using System;
using LBoL.Core.Cards;
namespace LBoL.Core
{
	public class CardMovingToDrawZoneEventArgs : GameEventArgs
	{
		public Card Card { get; internal set; }
		public CardZone SourceZone { get; internal set; }
		public DrawZoneTarget DrawZoneTarget { get; internal set; }
		protected override string GetBaseDebugString()
		{
			return string.Format("Card = {0}, {1} -> {2}", GameEventArgs.DebugString(this.Card), this.SourceZone, this.DrawZoneTarget);
		}
	}
}
