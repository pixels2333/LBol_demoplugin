using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	[UsedImplicitly]
	public sealed class SakuyaGoOut : Card
	{
		public override bool DiscardCard
		{
			get
			{
				return true;
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new DrawManyCardAction(base.Value1);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Battle.HandZone.Count > base.Value1)
			{
				SelectHandInteraction interaction = new SelectHandInteraction(base.Value1, base.Value1, base.Battle.HandZone)
				{
					Source = this
				};
				yield return new InteractionAction(interaction, false);
				yield return new DiscardManyAction(interaction.SelectedCards);
				interaction = null;
			}
			else
			{
				yield return new DiscardManyAction(base.Battle.HandZone);
			}
			yield return new GainManaAction(base.Mana);
			yield break;
		}
	}
}
