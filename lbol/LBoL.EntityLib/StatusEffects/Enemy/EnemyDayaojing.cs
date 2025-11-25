using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Enemy
{
	public sealed class EnemyDayaojing : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			if (base.Owner.IsInTurn)
			{
				this._skipFirstTurn = true;
			}
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnEnded));
		}
		private IEnumerable<BattleAction> OnOwnerTurnEnded(UnitEventArgs args)
		{
			if (this._skipFirstTurn)
			{
				this._skipFirstTurn = false;
				yield break;
			}
			Unit owner = base.Owner;
			if (owner != null && owner.IsAlive)
			{
				base.NotifyActivating();
				yield return PerformAction.Sfx("FairySupport", 0f);
				yield return PerformAction.Effect(base.Owner, "DaiyoFairy", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				yield return new HealAction(base.Owner, base.Owner, base.Level, HealType.Normal, 0.2f);
			}
			yield break;
		}
		private bool _skipFirstTurn;
	}
}
