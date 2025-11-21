using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Sakuya;

namespace LBoL.EntityLib.StatusEffects.Sakuya
{
	// Token: 0x02000013 RID: 19
	[UsedImplicitly]
	public sealed class BladePowerSe : StatusEffect
	{
		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000024 RID: 36 RVA: 0x00002469 File Offset: 0x00000669
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Empty;
			}
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00002470 File Offset: 0x00000670
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x06000026 RID: 38 RVA: 0x0000248F File Offset: 0x0000068F
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (!base.Battle.BattleShouldEnd && args.Card is Knife)
			{
				base.NotifyActivating();
				Card[] array = base.Battle.RollCards(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.Valid, CardTypeWeightTable.OnlyAttack, false), base.Level, null);
				foreach (Card card in array)
				{
					card.SetTurnCost(this.Mana);
					card.IsEthereal = true;
					card.IsExile = true;
				}
				yield return new AddCardsToHandAction(array);
			}
			yield break;
		}
	}
}
