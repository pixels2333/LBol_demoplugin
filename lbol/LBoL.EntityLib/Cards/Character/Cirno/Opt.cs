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
	public sealed class Opt : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			Card target = Enumerable.FirstOrDefault<Card>(base.Battle.DrawZone);
			if (target != null)
			{
				OptBottom optBottom = Library.CreateCard<OptBottom>();
				optBottom.TargetName = StringDecorator.GetEntityName(target.Name);
				MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(new Card[] { target, optBottom }, false, false, false)
				{
					Source = this
				};
				yield return new InteractionAction(interaction, false);
				if (interaction.SelectedCard is OptBottom)
				{
					yield return new MoveCardToDrawZoneAction(target, DrawZoneTarget.Bottom);
				}
				interaction = null;
			}
			yield return new DrawCardAction();
			yield return new GainManaAction(base.Mana);
			yield break;
		}
	}
}
