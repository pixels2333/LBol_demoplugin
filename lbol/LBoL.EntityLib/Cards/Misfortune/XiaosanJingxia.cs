using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Misfortune
{
	// Token: 0x02000351 RID: 849
	[UsedImplicitly]
	public sealed class XiaosanJingxia : Card
	{
		// Token: 0x06000C54 RID: 3156 RVA: 0x000181B3 File Offset: 0x000163B3
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
		}

		// Token: 0x06000C55 RID: 3157 RVA: 0x000181D7 File Offset: 0x000163D7
		private IEnumerable<BattleAction> OnPlayerTurnEnding(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Zone == CardZone.Hand)
			{
				base.NotifyActivating();
				yield return base.DebuffAction<Weak>(base.Battle.Player, 0, base.Value1, 0, 0, false, 0.2f);
			}
			yield break;
		}
	}
}
