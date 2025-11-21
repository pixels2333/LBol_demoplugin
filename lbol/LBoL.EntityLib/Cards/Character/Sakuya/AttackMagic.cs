using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x0200037D RID: 893
	[UsedImplicitly]
	public sealed class AttackMagic : Card
	{
		// Token: 0x17000168 RID: 360
		// (get) Token: 0x06000CC3 RID: 3267 RVA: 0x0001895B File Offset: 0x00016B5B
		public override bool DiscardCard
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000CC4 RID: 3268 RVA: 0x0001895E File Offset: 0x00016B5E
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			List<Card> list = Enumerable.ToList<Card>(base.Battle.HandZone);
			if (list.Count > 0)
			{
				SelectHandInteraction interaction = new SelectHandInteraction(0, base.Value1, list)
				{
					Source = this
				};
				yield return new InteractionAction(interaction, false);
				IReadOnlyList<Card> cards = interaction.SelectedCards;
				if (cards.Count > 0)
				{
					yield return new DiscardManyAction(cards);
					yield return new DrawManyCardAction(cards.Count);
				}
				interaction = null;
				cards = null;
			}
			yield break;
		}
	}
}
