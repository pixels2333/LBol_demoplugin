using System;
using System.Collections.Generic;
using LBoL.Core.Cards;
namespace LBoL.Core
{
	public class FollowAttackEventArgs : GameEventArgs
	{
		public UnitSelector SourceSelector { get; internal set; }
		public int Count { get; set; }
		public bool RandomFiller { get; set; }
		protected override string GetBaseDebugString()
		{
			return "FollowAttack: " + this.Count.ToString() + " -> " + GameEventArgs.DebugString(this.SourceSelector);
		}
		public readonly List<Card> Cards = new List<Card>();
	}
}
