using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000C6 RID: 198
	[UsedImplicitly]
	public sealed class ShenlingGold : StatusEffect
	{
		// Token: 0x060002AF RID: 687 RVA: 0x000075D8 File Offset: 0x000057D8
		protected override void OnAdded(Unit unit)
		{
			EnemyUnit enemyUnit = unit as EnemyUnit;
			if (enemyUnit != null)
			{
				base.HandleOwnerEvent<DieEventArgs>(enemyUnit.EnemyPointGenerating, delegate(DieEventArgs args)
				{
					args.Money += base.Level;
				});
			}
		}
	}
}
