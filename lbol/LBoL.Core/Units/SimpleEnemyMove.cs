using System;
using System.Collections.Generic;
using LBoL.Core.Battle;
namespace LBoL.Core.Units
{
	public class SimpleEnemyMove : IEnemyMove
	{
		public SimpleEnemyMove(Intention intention, IEnumerable<BattleAction> actions)
		{
			this.Intention = intention;
			this.Actions = actions;
		}
		public SimpleEnemyMove(Intention intention, BattleAction action)
		{
			IEnumerable<BattleAction> enumerable = new BattleAction[] { action };
			this.Intention = intention;
			this.Actions = enumerable;
		}
		public SimpleEnemyMove(Intention intention)
		{
			this.Intention = intention;
		}
		public Intention Intention { get; }
		public IEnumerable<BattleAction> Actions { get; }
	}
}
