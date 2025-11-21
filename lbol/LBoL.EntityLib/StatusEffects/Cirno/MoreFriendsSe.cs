using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Cirno
{
	// Token: 0x020000E6 RID: 230
	[UsedImplicitly]
	public sealed class MoreFriendsSe : StatusEffect
	{
		// Token: 0x06000339 RID: 825 RVA: 0x00008975 File Offset: 0x00006B75
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}

		// Token: 0x0600033A RID: 826 RVA: 0x00008994 File Offset: 0x00006B94
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			List<Card> list = Enumerable.ToList<Card>(base.Battle.RollCards(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.Valid, CardTypeWeightTable.OnlyFriend, false), base.Level, (CardConfig config) => config.Cost.Amount < 5));
			if (list.Count > 0)
			{
				foreach (Card card in list)
				{
					card.IsEthereal = true;
				}
				yield return new AddCardsToHandAction(list, AddCardsType.Normal);
			}
			yield break;
		}
	}
}
