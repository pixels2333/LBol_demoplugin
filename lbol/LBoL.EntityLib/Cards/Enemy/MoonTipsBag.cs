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
using LBoL.EntityLib.Cards.Neutral.NoColor;
namespace LBoL.EntityLib.Cards.Enemy
{
	[UsedImplicitly]
	public sealed class MoonTipsBag : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<Card> list = new List<Card>();
			list.Add(Library.CreateCard<MoonTipsAttack>());
			list.Add(Library.CreateCard<MoonTipsDefense>());
			list.Add(Library.CreateCard<MoonTipsHeal>());
			List<Card> list2 = list;
			MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(list2, false, false, false)
			{
				Source = this
			};
			yield return new InteractionAction(interaction, false);
			Card selectedCard = interaction.SelectedCard;
			if (!(selectedCard is MoonTipsAttack))
			{
				if (!(selectedCard is MoonTipsDefense))
				{
					if (selectedCard is MoonTipsHeal)
					{
						yield return base.HealAction(selectedCard.Value1);
					}
				}
				else
				{
					yield return base.DefenseAction(0, selectedCard.Value1, BlockShieldType.Direct, true);
				}
			}
			else
			{
				List<Shoot> list3 = Enumerable.ToList<Shoot>(Library.CreateCards<Shoot>(selectedCard.Value1, false));
				foreach (Shoot shoot in list3)
				{
					shoot.IsReplenish = true;
				}
				yield return new AddCardsToDrawZoneAction(list3, DrawZoneTarget.Random, AddCardsType.Normal);
			}
			yield break;
		}
	}
}
