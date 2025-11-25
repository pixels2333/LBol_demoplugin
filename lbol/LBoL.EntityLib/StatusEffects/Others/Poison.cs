using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Others
{
	public sealed class Poison : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			if (unit is EnemyUnit)
			{
				base.ReactOwnerEvent<GameEventArgs>(base.Battle.AllEnemyTurnStarted, new EventSequencedReactor<GameEventArgs>(this.OnAllEnemyTurnStarted));
				base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnEnemyTurnStarted));
				return;
			}
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}
		private IEnumerable<BattleAction> OnAllEnemyTurnStarted(GameEventArgs args)
		{
			return this.TakeEffect();
		}
		private IEnumerable<BattleAction> OnEnemyTurnStarted(UnitEventArgs args)
		{
			Unit owner = base.Owner;
			if (owner == null || !owner.IsExtraTurn)
			{
				yield break;
			}
			foreach (BattleAction battleAction in this.TakeEffect())
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield break;
			yield break;
		}
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			return this.TakeEffect();
		}
		public IEnumerable<BattleAction> TakeEffect()
		{
			if (base.Owner == null || base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			yield return DamageAction.LoseLife(base.Owner, base.Level, "Poison");
			int num = base.Level - 1;
			base.Level = num;
			if (base.Level == 0)
			{
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}
	}
}
