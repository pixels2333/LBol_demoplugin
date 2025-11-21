using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000162 RID: 354
	[UsedImplicitly]
	public sealed class DiyuChepiao : Exhibit
	{
		// Token: 0x060004E2 RID: 1250 RVA: 0x0000C701 File Offset: 0x0000A901
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<DamageEventArgs>(base.Battle.Player.DamageReceived, new EventSequencedReactor<DamageEventArgs>(this.OnPlayerDamageReceived));
		}

		// Token: 0x060004E3 RID: 1251 RVA: 0x0000C725 File Offset: 0x0000A925
		private IEnumerable<BattleAction> OnPlayerDamageReceived(DamageEventArgs args)
		{
			if (args.DamageInfo.Damage > 0f)
			{
				base.NotifyActivating();
				if (base.Value1 > 1)
				{
					yield return new DrawManyCardAction(base.Value1);
				}
				else
				{
					yield return new DrawCardAction();
				}
			}
			yield break;
		}
	}
}
