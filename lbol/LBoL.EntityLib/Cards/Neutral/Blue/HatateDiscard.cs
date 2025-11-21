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

namespace LBoL.EntityLib.Cards.Neutral.Blue
{
	// Token: 0x02000318 RID: 792
	[UsedImplicitly]
	public sealed class HatateDiscard : Card
	{
		// Token: 0x17000153 RID: 339
		// (get) Token: 0x06000BB4 RID: 2996 RVA: 0x00017565 File Offset: 0x00015765
		public override bool DiscardCard
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000BB5 RID: 2997 RVA: 0x00017568 File Offset: 0x00015768
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card hand) => hand != this));
			if (list.Count <= base.Value1)
			{
				this._allHand = list;
			}
			if (list.Count <= base.Value1)
			{
				return null;
			}
			return new SelectHandInteraction(base.Value1, base.Value2, list);
		}

		// Token: 0x06000BB6 RID: 2998 RVA: 0x000175CE File Offset: 0x000157CE
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition != null)
			{
				IReadOnlyList<Card> selectedCards = ((SelectHandInteraction)precondition).SelectedCards;
				if (selectedCards != null)
				{
					yield return new DiscardManyAction(selectedCards);
				}
			}
			else if (this._allHand.Count > 0)
			{
				yield return new DiscardManyAction(this._allHand);
				this._allHand = null;
			}
			yield return new GainManaAction(base.Mana);
			yield break;
		}

		// Token: 0x040000FB RID: 251
		private List<Card> _allHand;
	}
}
