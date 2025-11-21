using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x02000398 RID: 920
	[UsedImplicitly]
	public sealed class LoyalMaid : Card
	{
		// Token: 0x06000D1C RID: 3356 RVA: 0x00018F77 File Offset: 0x00017177
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarting));
		}

		// Token: 0x06000D1D RID: 3357 RVA: 0x00018F9B File Offset: 0x0001719B
		private IEnumerable<BattleAction> OnPlayerTurnStarting(UnitEventArgs args)
		{
			if (base.Battle.Player.IsExtraTurn && base.Zone == CardZone.Discard)
			{
				yield return new MoveCardAction(this, CardZone.Hand);
				base.SetTurnCost(base.Mana);
			}
			yield break;
		}
	}
}
