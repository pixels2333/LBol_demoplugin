using System;
using System.Collections.Generic;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Enemy
{
	// Token: 0x0200036E RID: 878
	public sealed class Wushuai : Card
	{
		// Token: 0x06000C9B RID: 3227 RVA: 0x000186E7 File Offset: 0x000168E7
		public override IEnumerable<BattleAction> OnDraw()
		{
			if (!base.Battle.BattleShouldEnd)
			{
				yield return new ApplyStatusEffectAction<LockedOn>(base.Battle.Player, new int?(base.Value1), default(int?), default(int?), default(int?), 0f, false);
				yield return new ExileCardAction(this);
			}
			yield break;
		}
	}
}
