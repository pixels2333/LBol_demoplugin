using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	// Token: 0x02000044 RID: 68
	[UsedImplicitly]
	public sealed class MeilingWaterSe : StatusEffect
	{
		// Token: 0x060000D3 RID: 211 RVA: 0x00003831 File Offset: 0x00001A31
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x060000D4 RID: 212 RVA: 0x00003855 File Offset: 0x00001A55
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			yield return new DamageAction(base.Owner, base.Owner, DamageInfo.HpLose((float)base.Level, false), "Instant", GunType.Single);
			yield break;
		}
	}
}
