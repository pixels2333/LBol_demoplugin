using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.EntityLib.EnemyUnits.Normal
{
	// Token: 0x020001DD RID: 477
	[UsedImplicitly]
	public sealed class Limao : EnemyUnit
	{
		// Token: 0x06000771 RID: 1905 RVA: 0x00010BEB File Offset: 0x0000EDEB
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 2, false, "Instant", false);
			yield break;
		}
	}
}
