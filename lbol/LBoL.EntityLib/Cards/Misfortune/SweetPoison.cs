using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Others;

namespace LBoL.EntityLib.Cards.Misfortune
{
	// Token: 0x02000350 RID: 848
	[UsedImplicitly]
	public sealed class SweetPoison : Card
	{
		// Token: 0x06000C51 RID: 3153 RVA: 0x00018177 File Offset: 0x00016377
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
		}

		// Token: 0x06000C52 RID: 3154 RVA: 0x0001819B File Offset: 0x0001639B
		private IEnumerable<BattleAction> OnPlayerTurnEnding(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Zone == CardZone.Hand)
			{
				base.NotifyActivating();
				yield return base.DebuffAction<Poison>(base.Battle.Player, base.Value1, 0, 0, 0, true, 0.2f);
			}
			yield break;
		}
	}
}
