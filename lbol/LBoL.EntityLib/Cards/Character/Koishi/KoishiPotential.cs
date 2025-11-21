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
	// Token: 0x02000479 RID: 1145
	[UsedImplicitly]
	public sealed class KoishiPotential : Card
	{
		// Token: 0x06000F5B RID: 3931 RVA: 0x0001B887 File Offset: 0x00019A87
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Battle.DrawZone.Count > 0)
			{
				List<Card> list = Enumerable.ToList<Card>(Enumerable.Take<Card>(base.Battle.DrawZone, base.Value1));
				if (list.Count > 0)
				{
					MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(list, false, false, false)
					{
						Source = this
					};
					yield return new InteractionAction(interaction, false);
					Card selectedCard = interaction.SelectedCard;
					if (selectedCard != null)
					{
						selectedCard.IsTempExile = true;
						PlayCardAction playCardAction = new PlayCardAction(selectedCard)
						{
							Args = 
							{
								PlayTwice = true
							}
						};
						yield return playCardAction;
					}
					interaction = null;
				}
			}
			yield break;
		}
	}
}
