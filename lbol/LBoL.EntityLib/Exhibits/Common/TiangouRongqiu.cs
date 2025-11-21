using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200019C RID: 412
	[UsedImplicitly]
	public sealed class TiangouRongqiu : Exhibit
	{
		// Token: 0x060005DD RID: 1501 RVA: 0x0000DE09 File Offset: 0x0000C009
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}

		// Token: 0x060005DE RID: 1502 RVA: 0x0000DE28 File Offset: 0x0000C028
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			base.NotifyActivating();
			yield return new ApplyStatusEffectAction<Spirit>(base.Owner, new int?(base.Value1), default(int?), default(int?), default(int?), 0f, true);
			base.Blackout = true;
			yield break;
		}

		// Token: 0x060005DF RID: 1503 RVA: 0x0000DE38 File Offset: 0x0000C038
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
