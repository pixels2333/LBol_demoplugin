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
namespace LBoL.EntityLib.Cards.Character.Cirno
{
	[UsedImplicitly]
	public sealed class IceWing : Card
	{
		[UsedImplicitly]
		public int BlueCount
		{
			get
			{
				if (base.Battle == null)
				{
					return 0;
				}
				return Enumerable.Count<Card>(base.Battle.DrawZone, (Card card) => Enumerable.Contains<ManaColor>(card.Config.Colors, ManaColor.Blue));
			}
		}
		protected override string GetBaseDescription()
		{
			if (base.Battle == null || this.BlueCount >= base.Value1)
			{
				return base.GetBaseDescription();
			}
			return base.GetExtraDescription1;
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainManaAction(base.Mana);
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.DrawZone, (Card card) => Enumerable.Contains<ManaColor>(card.Config.Colors, ManaColor.Blue)));
			if (list.Count > 0)
			{
				List<Card> cards = Enumerable.ToList<Card>(list.SampleManyOrAll(base.Value1, base.GameRun.BattleRng));
				if (cards.Count > 0)
				{
					MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(cards, false, false, false)
					{
						Source = this
					};
					yield return new InteractionAction(interaction, false);
					Card card3 = interaction.SelectedCard;
					yield return new MoveCardAction(card3, CardZone.Hand);
					cards.Remove(card3);
					if (cards.Count > 0)
					{
						foreach (Card card2 in cards)
						{
							yield return new MoveCardToDrawZoneAction(card2, DrawZoneTarget.Bottom);
						}
						List<Card>.Enumerator enumerator = default(List<Card>.Enumerator);
					}
					interaction = null;
					card3 = null;
				}
				cards = null;
			}
			yield break;
			yield break;
		}
	}
}
