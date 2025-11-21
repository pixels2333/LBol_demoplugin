using System;
using System.Collections.Generic;
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
	// Token: 0x020003C3 RID: 963
	[UsedImplicitly]
	public sealed class UnlimitedTime : Card
	{
		// Token: 0x06000D94 RID: 3476 RVA: 0x0001976D File Offset: 0x0001796D
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			Type[] array = LimitedStopTimeCard.All.SampleManyOrAll(base.Value1, base.GameRun.BattleCardRng);
			List<LimitedStopTimeCard> list = new List<LimitedStopTimeCard>();
			Type[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				LimitedStopTimeCard limitedStopTimeCard = (LimitedStopTimeCard)Library.CreateCard(array2[i]);
				limitedStopTimeCard.Limited = false;
				list.Add(limitedStopTimeCard);
			}
			if (list.Count > 0)
			{
				MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(list, false, false, false)
				{
					Source = this
				};
				yield return new InteractionAction(interaction, false);
				Card selectedCard = interaction.SelectedCard;
				selectedCard.SetTurnCost(base.Mana);
				selectedCard.IsEthereal = true;
				selectedCard.IsExile = true;
				yield return new AddCardsToHandAction(new Card[] { selectedCard });
				interaction = null;
			}
			yield break;
		}
	}
}
