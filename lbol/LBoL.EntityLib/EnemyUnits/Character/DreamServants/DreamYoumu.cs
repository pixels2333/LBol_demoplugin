using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Character.DreamServants
{
	[UsedImplicitly]
	public sealed class DreamYoumu : EnemyUnit
	{
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<DreamServant>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
			this.React(new ApplyStatusEffectAction<LouguanJianSe>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true));
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			yield return base.AttackMove(base.GetSpellCardName(default(int?), 0), base.Gun1, base.Damage1, 3, false, base.Gun1, false);
			yield break;
		}
	}
}
