using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.EntityLib.Dolls;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x02000121 RID: 289
	[UsedImplicitly]
	public sealed class AliceU : ShiningExhibit
	{
		// Token: 0x060003FA RID: 1018 RVA: 0x0000AF27 File Offset: 0x00009127
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}

		// Token: 0x060003FB RID: 1019 RVA: 0x0000AF46 File Offset: 0x00009146
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			if (base.Owner.DollSlotCount == 0)
			{
				yield return new AddDollSlotAction(1);
			}
			yield return new AddDollAction(Library.CreateDoll<Shanghai>());
			yield break;
		}
	}
}
