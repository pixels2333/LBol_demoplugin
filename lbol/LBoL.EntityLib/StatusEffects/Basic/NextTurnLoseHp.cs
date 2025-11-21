using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Basic
{
	// Token: 0x020000F4 RID: 244
	[UsedImplicitly]
	public sealed class NextTurnLoseHp : StatusEffect
	{
		// Token: 0x06000369 RID: 873 RVA: 0x00008E36 File Offset: 0x00007036
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x0600036A RID: 874 RVA: 0x00008E5A File Offset: 0x0000705A
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			yield return new DamageAction(base.Battle.Player, base.Battle.Player, DamageInfo.HpLose((float)base.Level, false), "Instant", GunType.Single);
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}
	}
}
