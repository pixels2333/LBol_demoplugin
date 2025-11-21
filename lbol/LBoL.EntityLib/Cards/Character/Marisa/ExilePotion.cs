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

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x0200041C RID: 1052
	[UsedImplicitly]
	public sealed class ExilePotion : Card
	{
		// Token: 0x06000E76 RID: 3702 RVA: 0x0001A860 File Offset: 0x00018A60
		public override Interaction Precondition()
		{
			List<Card> list = (this.IsUpgraded ? Enumerable.ToList<Card>(Enumerable.Where<Card>(Enumerable.Concat<Card>(Enumerable.Concat<Card>(base.Battle.HandZone, base.Battle.DrawZoneToShow), base.Battle.DiscardZone), (Card card) => card != this)) : Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card != this)));
			if (!list.Empty<Card>())
			{
				return new SelectCardInteraction(0, base.Value1, list, SelectedCardHandling.DoNothing);
			}
			return null;
		}

		// Token: 0x06000E77 RID: 3703 RVA: 0x0001A8F2 File Offset: 0x00018AF2
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition != null)
			{
				IReadOnlyList<Card> cards = ((SelectCardInteraction)precondition).SelectedCards;
				if (cards.Count > 0)
				{
					yield return new ExileManyCardAction(cards);
					yield return new AddCardsToDiscardAction(Library.CreateCards<Potion>(cards.Count, false), AddCardsType.Normal);
				}
				cards = null;
			}
			yield break;
		}
	}
}
