using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Blue
{
	// Token: 0x02000329 RID: 809
	[UsedImplicitly]
	public sealed class YonglinDraw : Card
	{
		// Token: 0x06000BE2 RID: 3042 RVA: 0x00017841 File Offset: 0x00015A41
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			DrawManyCardAction drawAction = new DrawManyCardAction(base.Value1);
			yield return drawAction;
			using (IEnumerator<Card> enumerator = drawAction.DrawnCards.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Card card = enumerator.Current;
					card.DecreaseTurnCost(base.Mana);
				}
				yield break;
			}
			yield break;
		}
	}
}
