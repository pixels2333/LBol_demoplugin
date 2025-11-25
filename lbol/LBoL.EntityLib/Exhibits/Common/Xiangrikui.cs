using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class Xiangrikui : Exhibit
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			base.Counter = (base.Counter + 1) % base.Value1;
			if (base.Counter >= base.Value2 && base.Counter <= base.Value3)
			{
				base.NotifyActivating();
				yield return new GainManaAction(base.Mana);
			}
			int num = base.Counter + 1;
			base.Active = num >= base.Value2 && num <= base.Value3;
			yield break;
		}
		protected override void OnLeaveBattle()
		{
			base.Active = false;
		}
	}
}
