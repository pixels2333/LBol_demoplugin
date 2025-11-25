using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class ShenmiPifeng : Exhibit
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnding));
		}
		private IEnumerable<BattleAction> OnTurnEnding(UnitEventArgs args)
		{
			int num = base.Battle.HandZone.Count * base.Value1;
			if (num > 0)
			{
				base.NotifyActivating();
				yield return new CastBlockShieldAction(base.Battle.Player, num, 0, BlockShieldType.Normal, true);
			}
			yield break;
		}
	}
}
