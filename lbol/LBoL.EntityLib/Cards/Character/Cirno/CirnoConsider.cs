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
namespace LBoL.EntityLib.Cards.Character.Cirno
{
	[UsedImplicitly]
	public sealed class CirnoConsider : Card
	{
		public override Interaction Precondition()
		{
			List<CirnoConsider> list = Enumerable.ToList<CirnoConsider>(Library.CreateCards<CirnoConsider>(2, this.IsUpgraded));
			CirnoConsider cirnoConsider = list[0];
			CirnoConsider cirnoConsider2 = list[1];
			cirnoConsider.ChoiceCardIndicator = 1;
			cirnoConsider2.ChoiceCardIndicator = 2;
			cirnoConsider.SetBattle(base.Battle);
			cirnoConsider2.SetBattle(base.Battle);
			return new MiniSelectCardInteraction(list, false, false, false);
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			MiniSelectCardInteraction miniSelectCardInteraction = (MiniSelectCardInteraction)precondition;
			Card card = ((miniSelectCardInteraction != null) ? miniSelectCardInteraction.SelectedCard : null);
			if (card != null)
			{
				if (card.ChoiceCardIndicator == 1)
				{
					yield return base.DefenseAction(true);
				}
				else
				{
					yield return new DrawManyCardAction(base.Value1);
				}
			}
			yield break;
		}
	}
}
