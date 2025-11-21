using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x020003B2 RID: 946
	[UsedImplicitly]
	public sealed class SakuyaTea : Card
	{
		// Token: 0x06000D69 RID: 3433 RVA: 0x000194A6 File Offset: 0x000176A6
		protected override string GetBaseDescription()
		{
			if (!base.DebutActive)
			{
				return base.GetExtraDescription1;
			}
			return base.GetBaseDescription();
		}

		// Token: 0x06000D6A RID: 3434 RVA: 0x000194BD File Offset: 0x000176BD
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new DrawManyCardAction(base.Value1);
			if (base.TriggeredAnyhow)
			{
				if (base.Value2 > 0)
				{
					yield return base.HealAction(base.Value2);
				}
				base.DecreaseBaseCost(base.Mana);
			}
			yield break;
		}
	}
}
