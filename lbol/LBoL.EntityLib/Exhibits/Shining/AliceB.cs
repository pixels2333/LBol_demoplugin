using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.EntityLib.Dolls;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x02000120 RID: 288
	[UsedImplicitly]
	public sealed class AliceB : ShiningExhibit
	{
		// Token: 0x060003F7 RID: 1015 RVA: 0x0000AEF0 File Offset: 0x000090F0
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}

		// Token: 0x060003F8 RID: 1016 RVA: 0x0000AF0F File Offset: 0x0000910F
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			if (base.Owner.DollSlotCount == 0)
			{
				yield return new AddDollSlotAction(1);
			}
			yield return new AddDollAction(Library.CreateDoll<Penglai>());
			yield break;
		}
	}
}
