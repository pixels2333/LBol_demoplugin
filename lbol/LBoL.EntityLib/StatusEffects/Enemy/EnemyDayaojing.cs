using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x0200009B RID: 155
	public sealed class EnemyDayaojing : StatusEffect
	{
		// Token: 0x0600022D RID: 557 RVA: 0x000067E2 File Offset: 0x000049E2
		protected override void OnAdded(Unit unit)
		{
			if (base.Owner.IsInTurn)
			{
				this._skipFirstTurn = true;
			}
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnEnded));
		}

		// Token: 0x0600022E RID: 558 RVA: 0x00006815 File Offset: 0x00004A15
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

		// Token: 0x04000016 RID: 22
		private bool _skipFirstTurn;
	}
}
