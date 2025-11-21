using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200018F RID: 399
	[UsedImplicitly]
	public sealed class ShiyanQicai : Exhibit
	{
		// Token: 0x060005A1 RID: 1441 RVA: 0x0000D926 File Offset: 0x0000BB26
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.Reshuffled, new EventSequencedReactor<GameEventArgs>(this.OnReshuffled));
		}

		// Token: 0x060005A2 RID: 1442 RVA: 0x0000D945 File Offset: 0x0000BB45
		private IEnumerable<BattleAction> OnReshuffled(GameEventArgs args)
		{
			base.NotifyActivating();
			yield return new GainManaAction(base.Mana);
			yield break;
		}
	}
}
