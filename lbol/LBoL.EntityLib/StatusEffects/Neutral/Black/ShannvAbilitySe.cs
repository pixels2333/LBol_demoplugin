using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Others;
namespace LBoL.EntityLib.StatusEffects.Neutral.Black
{
	public sealed class ShannvAbilitySe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
		}
		private IEnumerable<BattleAction> OnPlayerTurnEnding(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			foreach (EnemyUnit enemyUnit in base.Battle.EnemyGroup.Alives)
			{
				if (enemyUnit.IsAlive)
				{
					yield return new ApplyStatusEffectAction<Poison>(enemyUnit, new int?(base.Level), default(int?), default(int?), default(int?), 0.2f, true);
				}
			}
			IEnumerator<EnemyUnit> enumerator = null;
			yield break;
			yield break;
		}
	}
}
