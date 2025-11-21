using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001AF RID: 431
	[UsedImplicitly]
	public sealed class YueYuyi : Exhibit
	{
		// Token: 0x0600062F RID: 1583 RVA: 0x0000E5D0 File Offset: 0x0000C7D0
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, delegate(UnitEventArgs _)
			{
				base.Blackout = true;
			});
		}

		// Token: 0x06000630 RID: 1584 RVA: 0x0000E621 File Offset: 0x0000C821
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}

		// Token: 0x06000631 RID: 1585 RVA: 0x0000E62A File Offset: 0x0000C82A
		private IEnumerable<BattleAction> OnPlayerTurnStarted(GameEventArgs args)
		{
			if (base.Battle.Player.TurnCounter == 1)
			{
				base.NotifyActivating();
				Unit player = base.Battle.Player;
				int? num = new int?(base.Value1);
				yield return new ApplyStatusEffectAction<Invincible>(player, default(int?), num, default(int?), default(int?), 0f, true);
			}
			yield break;
		}
	}
}
