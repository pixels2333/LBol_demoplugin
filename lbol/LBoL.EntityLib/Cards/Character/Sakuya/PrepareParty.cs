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
	[UsedImplicitly]
	public sealed class PrepareParty : Card
	{
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card hand) => hand != this));
			if (!list.Empty<Card>())
			{
				return new SelectHandInteraction(0, 1, list);
			}
			return null;
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			SelectHandInteraction selectHandInteraction = (SelectHandInteraction)precondition;
			Card card = ((selectHandInteraction != null) ? Enumerable.FirstOrDefault<Card>(selectHandInteraction.SelectedCards) : null);
			if (card != null)
			{
				yield return new MoveCardToDrawZoneAction(card, DrawZoneTarget.Top);
			}
			yield return base.DefenseAction(true);
			yield break;
		}
	}
}
