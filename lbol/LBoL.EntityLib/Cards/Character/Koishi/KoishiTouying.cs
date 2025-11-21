using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x0200047C RID: 1148
	[UsedImplicitly]
	public sealed class KoishiTouying : Card
	{
		// Token: 0x06000F61 RID: 3937 RVA: 0x0001B8DB File Offset: 0x00019ADB
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainManaAction(base.Mana);
			Card[] array = base.Battle.RollCards(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), base.Value1, (CardConfig config) => config.Cost.Amount >= base.Value2);
			if (array.Length != 0)
			{
				MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(array, false, false, false)
				{
					Source = this
				};
				yield return new InteractionAction(interaction, false);
				Card selectedCard = interaction.SelectedCard;
				selectedCard.IsReplenish = true;
				selectedCard.IsEthereal = true;
				selectedCard.IsExile = true;
				yield return new AddCardsToDrawZoneAction(new Card[] { selectedCard }, DrawZoneTarget.Top, AddCardsType.Normal);
				interaction = null;
			}
			yield break;
		}
	}
}
