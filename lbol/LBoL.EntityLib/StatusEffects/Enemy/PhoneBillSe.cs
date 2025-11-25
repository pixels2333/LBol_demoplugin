using System;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Enemy
{
	public sealed class PhoneBillSe : StatusEffect
	{
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
