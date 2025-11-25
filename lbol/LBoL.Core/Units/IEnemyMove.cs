using System;
using System.Collections.Generic;
using LBoL.Core.Battle;
namespace LBoL.Core.Units
{
	public interface IEnemyMove
	{
		Intention Intention { get; }
		IEnumerable<BattleAction> Actions { get; }
		IEnemyMove AsHiddenIntention(bool hidden = true)
		{
			this.Intention.AsHidden(hidden);
			return this;
		}
	}
}
