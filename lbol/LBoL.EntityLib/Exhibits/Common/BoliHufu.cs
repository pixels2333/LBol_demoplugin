using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.EntityLib.StatusEffects.Basic;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000159 RID: 345
	[UsedImplicitly]
	public sealed class BoliHufu : Exhibit
	{
		// Token: 0x060004BB RID: 1211 RVA: 0x0000C368 File Offset: 0x0000A568
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, delegate(UnitEventArgs _)
			{
				base.Blackout = true;
			});
		}

		// Token: 0x060004BC RID: 1212 RVA: 0x0000C3B4 File Offset: 0x0000A5B4
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			base.NotifyActivating();
			yield return new ApplyStatusEffectAction<Amulet>(base.Owner, new int?(base.Value1), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x060004BD RID: 1213 RVA: 0x0000C3C4 File Offset: 0x0000A5C4
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
