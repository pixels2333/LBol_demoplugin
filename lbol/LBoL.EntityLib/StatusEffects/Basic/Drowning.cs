using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Normal;
namespace LBoL.EntityLib.StatusEffects.Basic
{
	public sealed class Drowning : StatusEffect
	{
		protected override string GetBaseDescription()
		{
			if (!this._isPlayer)
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnEnded));
			if (unit is PlayerUnit)
			{
				this._isPlayer = true;
				base.ReactOwnerEvent<DieEventArgs>(base.Battle.EnemyDied, new EventSequencedReactor<DieEventArgs>(this.OnEnemyDied));
			}
		}
		private IEnumerable<BattleAction> OnOwnerTurnEnded(UnitEventArgs args)
		{
			if (base.Owner == null || base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			yield return DamageAction.Reaction(base.Owner, base.Level, (base.Level >= 15) ? "溺水BuffB" : "溺水BuffA");
			yield break;
		}
		private IEnumerable<BattleAction> OnEnemyDied(DieEventArgs args)
		{
			BattleController battle = base.Battle;
			if (battle != null && battle.BattleShouldEnd)
			{
				yield break;
			}
			if (args.Unit is WaterGirl)
			{
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}
		public override string UnitEffectName
		{
			get
			{
				return "DrowningLoop";
			}
		}
		private bool _isPlayer;
	}
}
