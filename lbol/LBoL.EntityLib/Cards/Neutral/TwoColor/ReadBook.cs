using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x020002A2 RID: 674
	[UsedImplicitly]
	public sealed class ReadBook : Card
	{
		// Token: 0x06000A7B RID: 2683 RVA: 0x00015C28 File Offset: 0x00013E28
		protected override string GetBaseDescription()
		{
			if (!base.DebutActive)
			{
				return base.GetExtraDescription1;
			}
			return base.GetBaseDescription();
		}

		// Token: 0x06000A7C RID: 2684 RVA: 0x00015C3F File Offset: 0x00013E3F
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.TriggeredAnyhow)
			{
				Card[] array = base.Battle.RollCards(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), base.Value1, delegate(CardConfig config)
				{
					if (config.Colors.Count > 0)
					{
						return Enumerable.All<ManaColor>(config.Colors, (ManaColor color) => color == ManaColor.White || color == ManaColor.Blue);
					}
					return false;
				});
				if (array.Length != 0)
				{
					MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(array, false, false, false)
					{
						Source = this
					};
					yield return new InteractionAction(interaction, false);
					Card selectedCard = interaction.SelectedCard;
					yield return new AddCardsToHandAction(new Card[] { selectedCard });
					interaction = null;
				}
			}
			yield break;
		}
	}
}
