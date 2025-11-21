using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x02000137 RID: 311
	[UsedImplicitly]
	public sealed class QinaKapian : ShiningExhibit
	{
		// Token: 0x06000444 RID: 1092 RVA: 0x0000B6C9 File Offset: 0x000098C9
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UsUsingEventArgs>(base.Battle.UsUsed, new EventSequencedReactor<UsUsingEventArgs>(this.OnUsUsed));
		}

		// Token: 0x06000445 RID: 1093 RVA: 0x0000B6E8 File Offset: 0x000098E8
		private IEnumerable<BattleAction> OnUsUsed(UsUsingEventArgs args)
		{
			base.NotifyActivating();
			yield return new DamageAction(base.Owner, base.Owner, DamageInfo.HpLose((float)base.Value1, false), "Instant", GunType.Single);
			yield return new DrawManyCardAction(base.Value2);
			yield break;
		}
	}
}
