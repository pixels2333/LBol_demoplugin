using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.Exhibits.Shining;
namespace LBoL.EntityLib.Cards.Enemy
{
	public sealed class Riguang : Card
	{
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
		}
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
