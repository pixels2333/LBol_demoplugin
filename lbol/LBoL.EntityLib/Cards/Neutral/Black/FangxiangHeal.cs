using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Black
{
	// Token: 0x0200032F RID: 815
	[UsedImplicitly]
	public sealed class FangxiangHeal : Card
	{
		// Token: 0x06000BEF RID: 3055 RVA: 0x0001793C File Offset: 0x00015B3C
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (base.Battle.DiscardZone.Count > 0)
			{
				List<Card> list = Enumerable.ToList<Card>(base.Battle.DiscardZone.SampleManyOrAll(base.Value1, base.GameRun.BattleRng));
				int count = list.Count;
				yield return new ExileManyCardAction(list);
				yield return base.HealAction(count * base.Value2);
			}
			yield break;
		}
	}
}
