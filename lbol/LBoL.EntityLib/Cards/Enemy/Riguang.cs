using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.Exhibits.Shining;

namespace LBoL.EntityLib.Cards.Enemy
{
	// Token: 0x0200036C RID: 876
	public sealed class Riguang : Card
	{
		// Token: 0x06000C96 RID: 3222 RVA: 0x000185F8 File Offset: 0x000167F8
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
		}

		// Token: 0x06000C97 RID: 3223 RVA: 0x0001861C File Offset: 0x0001681C
		private IEnumerable<BattleAction> OnPlayerTurnEnding(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Zone == CardZone.Hand)
			{
				if (base.Battle.Player.HasExhibit<KoishiB>())
				{
					base.NotifyActivating();
					base.Battle.Player.GetExhibit<KoishiB>().NotifyActivating();
				}
				else
				{
					base.NotifyActivating();
					yield return base.DamageSelfAction(base.Value1, "ESunnyCard");
				}
			}
			yield break;
		}
	}
}
