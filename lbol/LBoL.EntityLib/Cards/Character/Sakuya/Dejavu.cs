using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x02000388 RID: 904
	[UsedImplicitly]
	public sealed class Dejavu : Card
	{
		// Token: 0x06000CE4 RID: 3300 RVA: 0x00018B6C File Offset: 0x00016D6C
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card hand) => hand != this));
			if (!list.Empty<Card>())
			{
				return new SelectHandInteraction(0, base.Value1, list);
			}
			return null;
		}

		// Token: 0x06000CE5 RID: 3301 RVA: 0x00018BB2 File Offset: 0x00016DB2
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition != null)
			{
				IReadOnlyList<Card> selectedCards = ((SelectHandInteraction)precondition).SelectedCards;
				if (selectedCards.Count > 0)
				{
					foreach (Card card in selectedCards)
					{
						yield return new MoveCardToDrawZoneAction(card, DrawZoneTarget.Top);
					}
					IEnumerator<Card> enumerator = null;
				}
			}
			yield return new DrawManyCardAction(base.Value1);
			yield return new GainManaAction(base.Mana);
			yield break;
			yield break;
		}
	}
}
