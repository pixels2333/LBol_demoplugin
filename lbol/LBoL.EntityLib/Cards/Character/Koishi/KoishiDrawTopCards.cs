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

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000471 RID: 1137
	[UsedImplicitly]
	public sealed class KoishiDrawTopCards : Card
	{
		// Token: 0x170001A7 RID: 423
		// (get) Token: 0x06000F47 RID: 3911 RVA: 0x0001B71E File Offset: 0x0001991E
		[UsedImplicitly]
		public int DrawCount
		{
			get
			{
				return Math.Max(0, base.Value1 - 1);
			}
		}

		// Token: 0x06000F48 RID: 3912 RVA: 0x0001B72E File Offset: 0x0001992E
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Battle.DrawZone.Count > 0)
			{
				List<Card> cards = Enumerable.ToList<Card>(Enumerable.Take<Card>(base.Battle.DrawZone, base.Value1));
				if (cards.Count > 0)
				{
					MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(cards, false, false, false)
					{
						Source = this
					};
					yield return new InteractionAction(interaction, false);
					Card selectedCard = interaction.SelectedCard;
					cards.Remove(selectedCard);
					if (cards.Count > 0)
					{
						foreach (Card card in cards)
						{
							if (card.Zone == CardZone.Draw)
							{
								yield return new MoveCardAction(card, CardZone.Hand);
							}
						}
						List<Card>.Enumerator enumerator = default(List<Card>.Enumerator);
					}
					interaction = null;
				}
				cards = null;
			}
			yield break;
			yield break;
		}
	}
}
