using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Sakuya
{
	// Token: 0x02000021 RID: 33
	[UsedImplicitly]
	public sealed class SpecialClockSe : StatusEffect
	{
		// Token: 0x0600004E RID: 78 RVA: 0x000027E1 File Offset: 0x000009E1
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00002800 File Offset: 0x00000A00
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd || base.Battle.HandZone.Count == 0)
			{
				yield break;
			}
			base.NotifyActivating();
			SelectHandInteraction interaction = new SelectHandInteraction(0, base.Level, base.Battle.HandZone)
			{
				Source = this
			};
			yield return new InteractionAction(interaction, false);
			IReadOnlyList<Card> cards = interaction.SelectedCards;
			if (cards.Count > 0)
			{
				yield return new DiscardManyAction(cards);
				if (base.Battle.DrawAfterDiscard > 0)
				{
					int drawAfterDiscard = base.Battle.DrawAfterDiscard;
					base.Battle.DrawAfterDiscard = 0;
					yield return new DrawManyCardAction(drawAfterDiscard);
				}
				yield return new DrawManyCardAction(cards.Count);
			}
			yield break;
		}
	}
}
