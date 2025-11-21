using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.Black
{
	// Token: 0x02000062 RID: 98
	public sealed class NextTurnLoseGame : StatusEffect
	{
		// Token: 0x06000158 RID: 344 RVA: 0x00004A4F File Offset: 0x00002C4F
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarting));
			base.Highlight = true;
		}

		// Token: 0x06000159 RID: 345 RVA: 0x00004A7A File Offset: 0x00002C7A
		private IEnumerable<BattleAction> OnPlayerTurnStarting(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			yield return new DamageAction(base.Owner, base.Owner, DamageInfo.Reaction(9999f, false), "Instant", GunType.Single);
			Unit owner = base.Owner;
			if (owner != null && !owner.IsDead)
			{
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}
	}
}
