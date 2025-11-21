using System;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000BC RID: 188
	public sealed class PhoneBillSe : StatusEffect
	{
		// Token: 0x06000294 RID: 660 RVA: 0x000072E0 File Offset: 0x000054E0
		protected override void OnAdded(Unit unit)
		{
			EnemyUnit enemyUnit = unit as EnemyUnit;
			if (enemyUnit != null)
			{
				base.HandleOwnerEvent<DieEventArgs>(enemyUnit.EnemyPointGenerating, delegate(DieEventArgs args)
				{
					args.Money += base.Count;
				});
			}
		}
	}
}
