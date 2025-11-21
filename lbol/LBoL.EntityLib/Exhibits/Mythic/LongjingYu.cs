using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Mythic
{
	// Token: 0x02000151 RID: 337
	[UsedImplicitly]
	public sealed class LongjingYu : MythicExhibit
	{
		// Token: 0x06000497 RID: 1175 RVA: 0x0000BED4 File Offset: 0x0000A0D4
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}

		// Token: 0x06000498 RID: 1176 RVA: 0x0000BEF3 File Offset: 0x0000A0F3
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			base.NotifyActivating();
			yield return new GainManaAction(base.Mana);
			yield break;
		}
	}
}
