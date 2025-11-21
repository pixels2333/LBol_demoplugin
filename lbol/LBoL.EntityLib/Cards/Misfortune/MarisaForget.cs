using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Misfortune
{
	// Token: 0x0200034B RID: 843
	[UsedImplicitly]
	public sealed class MarisaForget : Card
	{
		// Token: 0x06000C44 RID: 3140 RVA: 0x0001807F File Offset: 0x0001627F
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
		}

		// Token: 0x06000C45 RID: 3141 RVA: 0x000180A3 File Offset: 0x000162A3
		private IEnumerable<BattleAction> OnPlayerTurnEnding(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Zone == CardZone.Hand)
			{
				base.NotifyActivating();
				yield return base.DebuffAction<Fragil>(base.Battle.Player, 0, base.Value1, 0, 0, false, 0.2f);
			}
			yield break;
		}
	}
}
