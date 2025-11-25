using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Units;
namespace LBoL.EntityLib.EnemyUnits.Normal
{
	[UsedImplicitly]
	public sealed class Limao : EnemyUnit
	{
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 2, false, "Instant", false);
			yield break;
		}
	}
}
