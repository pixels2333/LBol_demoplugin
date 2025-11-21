using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Reimu
{
	// Token: 0x0200002D RID: 45
	[UsedImplicitly]
	public sealed class JiangshenSe : StatusEffect
	{
		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600007B RID: 123 RVA: 0x00002CDB File Offset: 0x00000EDB
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Anys(1);
			}
		}

		// Token: 0x0600007C RID: 124 RVA: 0x00002CE4 File Offset: 0x00000EE4
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToDiscard, new EventSequencedReactor<CardsEventArgs>(this.OnAddCard));
			base.ReactOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToHand, new EventSequencedReactor<CardsEventArgs>(this.OnAddCard));
			base.ReactOwnerEvent<CardsEventArgs>(base.Battle.CardsAddedToExile, new EventSequencedReactor<CardsEventArgs>(this.OnAddCard));
			base.ReactOwnerEvent<CardsAddingToDrawZoneEventArgs>(base.Battle.CardsAddedToDrawZone, new EventSequencedReactor<CardsAddingToDrawZoneEventArgs>(this.OnCardsAddedToDrawZone));
		}

		// Token: 0x0600007D RID: 125 RVA: 0x00002D65 File Offset: 0x00000F65
		private IEnumerable<BattleAction> OnAddCard(CardsEventArgs args)
		{
			yield return this.Upgrade(args.Cards);
			yield break;
		}

		// Token: 0x0600007E RID: 126 RVA: 0x00002D7C File Offset: 0x00000F7C
		private IEnumerable<BattleAction> OnCardsAddedToDrawZone(CardsAddingToDrawZoneEventArgs args)
		{
			yield return this.Upgrade(args.Cards);
			yield break;
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00002D94 File Offset: 0x00000F94
		private BattleAction Upgrade(IEnumerable<Card> cards)
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(cards, (Card card) => card.CanUpgradeAndPositive));
			if (list.Count == 0)
			{
				return null;
			}
			base.NotifyActivating();
			return new UpgradeCardsAction(list);
		}
	}
}
