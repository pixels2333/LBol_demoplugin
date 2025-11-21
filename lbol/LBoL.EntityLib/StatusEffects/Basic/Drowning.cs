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
	// Token: 0x020000EE RID: 238
	public sealed class Drowning : StatusEffect
	{
		// Token: 0x06000352 RID: 850 RVA: 0x00008BD5 File Offset: 0x00006DD5
		protected override string GetBaseDescription()
		{
			if (!this._isPlayer)
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}

		// Token: 0x06000353 RID: 851 RVA: 0x00008BEC File Offset: 0x00006DEC
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnEnded));
			if (unit is PlayerUnit)
			{
				this._isPlayer = true;
				base.ReactOwnerEvent<DieEventArgs>(base.Battle.EnemyDied, new EventSequencedReactor<DieEventArgs>(this.OnEnemyDied));
			}
		}

		// Token: 0x06000354 RID: 852 RVA: 0x00008C42 File Offset: 0x00006E42
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

		// Token: 0x06000355 RID: 853 RVA: 0x00008C52 File Offset: 0x00006E52
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

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x06000356 RID: 854 RVA: 0x00008C69 File Offset: 0x00006E69
		public override string UnitEffectName
		{
			get
			{
				return "DrowningLoop";
			}
		}

		// Token: 0x0400002F RID: 47
		private bool _isPlayer;
	}
}
