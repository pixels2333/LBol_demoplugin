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
	// Token: 0x0200031C RID: 796
	[UsedImplicitly]
	public sealed class PlayWithWater : Card
	{
		// Token: 0x17000154 RID: 340
		// (get) Token: 0x06000BBF RID: 3007 RVA: 0x00017645 File Offset: 0x00015845
		public override bool DiscardCard
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000BC0 RID: 3008 RVA: 0x00017648 File Offset: 0x00015848
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new DrawManyCardAction(base.Value1);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			List<Card> list = Enumerable.ToList<Card>(base.Battle.HandZone);
			if (this.IsUpgraded)
			{
				if (list.Count > 0)
				{
					SelectHandInteraction interaction = new SelectHandInteraction(0, 1, base.Battle.HandZone)
					{
						Source = this
					};
					yield return new InteractionAction(interaction, false);
					Card card = Enumerable.FirstOrDefault<Card>(interaction.SelectedCards);
					if (card != null)
					{
						yield return new DiscardAction(card);
					}
					interaction = null;
				}
			}
			else if (list.Count > 1)
			{
				SelectHandInteraction interaction = new SelectHandInteraction(1, 1, list)
				{
					Source = this
				};
				yield return new InteractionAction(interaction, false);
				yield return new DiscardAction(interaction.SelectedCards[0]);
				interaction = null;
			}
			else if (list.Count == 1)
			{
				yield return new DiscardAction(list[0]);
			}
			yield break;
		}
	}
}
