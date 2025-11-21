using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000165 RID: 357
	[UsedImplicitly]
	public sealed class FeixiangJian : Exhibit
	{
		// Token: 0x060004EB RID: 1259 RVA: 0x0000C7DB File Offset: 0x0000A9DB
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x060004EC RID: 1260 RVA: 0x0000C7FF File Offset: 0x0000A9FF
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			base.NotifyActivating();
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new DamageAction(base.Owner, base.Battle.EnemyGroup.Alives, DamageInfo.Reaction((float)base.Value1, false), "ExhFeixiang", GunType.Single);
			yield break;
		}
	}
}
