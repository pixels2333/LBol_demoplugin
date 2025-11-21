using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001A1 RID: 417
	[UsedImplicitly]
	public sealed class Tnt : Exhibit
	{
		// Token: 0x060005F6 RID: 1526 RVA: 0x0000E004 File Offset: 0x0000C204
		protected override void OnEnterBattle()
		{
			base.Counter = base.Value1;
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, delegate(UnitEventArgs _)
			{
				if (base.Counter == 0)
				{
					base.Blackout = true;
				}
			});
		}

		// Token: 0x060005F7 RID: 1527 RVA: 0x0000E061 File Offset: 0x0000C261
		private IEnumerable<BattleAction> OnPlayerTurnStarted(GameEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Counter > 0)
			{
				int num = base.Counter - 1;
				base.Counter = num;
				if (base.Counter == 1)
				{
					base.Active = true;
				}
				if (base.Counter == 0)
				{
					base.NotifyActivating();
					yield return new DamageAction(base.Owner, base.Battle.EnemyGroup.Alives, DamageInfo.Reaction((float)base.Value2, false), "ExhTNT", GunType.Single);
					base.Active = false;
				}
			}
			yield break;
		}

		// Token: 0x060005F8 RID: 1528 RVA: 0x0000E071 File Offset: 0x0000C271
		protected override void OnLeaveBattle()
		{
			base.Active = false;
			base.Blackout = false;
		}
	}
}
