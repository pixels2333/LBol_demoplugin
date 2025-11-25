using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Cirno
{
	[UsedImplicitly]
	public sealed class ColdChain : Card
	{
		public override Interaction Precondition()
		{
			IReadOnlyList<Card> drawZoneToShow = base.Battle.DrawZoneToShow;
			if (drawZoneToShow.Count <= 0)
			{
				return null;
			}
			return new SelectCardInteraction(0, base.Value1, drawZoneToShow, SelectedCardHandling.DoNothing);
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			SelectCardInteraction selectCardInteraction = (SelectCardInteraction)precondition;
			IReadOnlyList<Card> readOnlyList = ((selectCardInteraction != null) ? selectCardInteraction.SelectedCards : null);
			if (readOnlyList != null && readOnlyList.Count > 0)
			{
				foreach (Card card in readOnlyList)
				{
					yield return new MoveCardAction(card, CardZone.Hand);
				}
				IEnumerator<Card> enumerator = null;
			}
			yield break;
			yield break;
		}
	}
}
