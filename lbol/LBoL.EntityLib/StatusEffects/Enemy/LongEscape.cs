using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000B4 RID: 180
	[UsedImplicitly]
	public sealed class LongEscape : StatusEffect
	{
		// Token: 0x06000271 RID: 625 RVA: 0x00006F24 File Offset: 0x00005124
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
