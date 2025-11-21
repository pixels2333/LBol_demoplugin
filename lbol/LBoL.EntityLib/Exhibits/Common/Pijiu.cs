using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000184 RID: 388
	[UsedImplicitly]
	public sealed class Pijiu : Exhibit
	{
		// Token: 0x06000573 RID: 1395 RVA: 0x0000D4F8 File Offset: 0x0000B6F8
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x06000574 RID: 1396 RVA: 0x0000D51C File Offset: 0x0000B71C
		private IEnumerable<BattleAction> OnPlayerTurnStarted(GameEventArgs args)
		{
			if (base.Battle.Player.TurnCounter == 1)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<TempFirepower>(base.Owner, new int?(base.Value1), default(int?), default(int?), default(int?), 0f, true);
				yield return new AddCardsToHandAction(Library.CreateCards<Xiaozhuo>(base.Value2, false), AddCardsType.Normal);
				base.Blackout = true;
			}
			yield break;
		}

		// Token: 0x06000575 RID: 1397 RVA: 0x0000D52C File Offset: 0x0000B72C
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
