using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Misfortune
{
	// Token: 0x02000347 RID: 839
	[UsedImplicitly]
	public sealed class BuyPeace : Card
	{
		// Token: 0x06000C33 RID: 3123 RVA: 0x00017ECA File Offset: 0x000160CA
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
		}

		// Token: 0x06000C34 RID: 3124 RVA: 0x00017EEE File Offset: 0x000160EE
		private IEnumerable<BattleAction> OnPlayerTurnEnding(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Zone == CardZone.Hand && base.Battle.GameRun.Money > 0)
			{
				base.NotifyActivating();
				yield return new LoseMoneyAction(base.Value1);
				yield return base.HealAction(base.Value2);
			}
			yield break;
		}
	}
}
