using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Enemy
{
	// Token: 0x0200035B RID: 859
	public abstract class AyaNewsSp : Card
	{
		// Token: 0x06000C64 RID: 3172 RVA: 0x00018294 File Offset: 0x00016494
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<UnitEventArgs>(battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
		}

		// Token: 0x06000C65 RID: 3173 RVA: 0x000182B3 File Offset: 0x000164B3
		private IEnumerable<BattleAction> OnPlayerTurnEnding(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Zone == CardZone.Hand)
			{
				base.NotifyActivating();
				yield return base.DebuffAction<SpiritNegative>(base.Battle.Player, base.Value1, 0, 0, 0, true, 0.2f);
			}
			yield break;
		}
	}
}
