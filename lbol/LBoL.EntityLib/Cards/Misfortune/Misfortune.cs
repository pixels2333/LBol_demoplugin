using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Misfortune
{
	// Token: 0x0200034C RID: 844
	[UsedImplicitly]
	public sealed class Misfortune : Card
	{
		// Token: 0x06000C47 RID: 3143 RVA: 0x000180BB File Offset: 0x000162BB
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnding));
		}

		// Token: 0x06000C48 RID: 3144 RVA: 0x000180DF File Offset: 0x000162DF
		private IEnumerable<BattleAction> OnTurnEnding(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			int num = base.GameRun.Money / 100 * base.Value1;
			if (base.Zone == CardZone.Hand && num > 0)
			{
				base.NotifyActivating();
				yield return base.LoseLifeAction(num);
			}
			yield break;
		}
	}
}
