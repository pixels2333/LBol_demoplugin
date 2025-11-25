using System;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public class EndShootAction : SimpleAction
	{
		public EndShootAction(Unit source)
		{
			this.SourceUnit = source;
		}
		public Unit SourceUnit { get; }
	}
}
