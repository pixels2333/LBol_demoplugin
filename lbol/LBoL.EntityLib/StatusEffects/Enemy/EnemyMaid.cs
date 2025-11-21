using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000A0 RID: 160
	public sealed class EnemyMaid : StatusEffect
	{
		// Token: 0x0600023B RID: 571 RVA: 0x000069B7 File Offset: 0x00004BB7
		protected override void OnAdded(Unit unit)
		{
			if (base.Owner.IsInTurn)
			{
				this._skipFirstTurn = true;
			}
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnEnded));
		}

		// Token: 0x0600023C RID: 572 RVA: 0x000069EA File Offset: 0x00004BEA
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
				yield return PerformAction.Effect(base.Owner, "MaidFairy", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				yield return new DamageAction(base.Owner, base.Battle.Player, DamageInfo.Reaction((float)base.Level, false), "女仆妖精0NoAni", GunType.Single);
			}
			yield break;
		}

		// Token: 0x04000017 RID: 23
		private bool _skipFirstTurn;
	}
}
