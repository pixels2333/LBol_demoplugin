using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
namespace LBoL.EntityLib.Exhibits.Mythic
{
	[UsedImplicitly]
	public sealed class YanZianbei : MythicExhibit
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted));
		}
		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs arg)
		{
			base.NotifyActivating();
			yield return new HealAction(base.Owner, base.Owner, base.Value1, HealType.Normal, 0.2f);
			yield break;
		}
	}
}
