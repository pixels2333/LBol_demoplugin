using System;
using System.Linq;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using Yarn;

namespace LBoL.EntityLib.Adventures.Stage1
{
	// Token: 0x0200050D RID: 1293
	[AdventureInfo(WeighterType = typeof(EternityAscension.EternityAscensionWeighter))]
	public sealed class EternityAscension : Adventure
	{
		// Token: 0x060010FF RID: 4351 RVA: 0x0001EA88 File Offset: 0x0001CC88
		protected override void InitVariables(IVariableStorage storage)
		{
			Card card = base.GameRun.RollTransformCard(base.GameRun.AdventureRng, new CardWeightTable(RarityWeightTable.OnlyUncommon, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), false, false, null);
			storage.SetValue("$transformCard", card.Id);
		}

		// Token: 0x02000A66 RID: 2662
		private class EternityAscensionWeighter : IAdventureWeighter
		{
			// Token: 0x06003747 RID: 14151 RVA: 0x000864F8 File Offset: 0x000846F8
			public float WeightFor(Type type, GameRunController gameRun)
			{
				if (Enumerable.Count<Card>(gameRun.BaseDeck, (Card c) => c.CanUpgrade) > 0)
				{
					if (Enumerable.Count<Card>(gameRun.BaseDeckWithoutUnremovable, (Card c) => c.CardType != CardType.Misfortune && c.CardType != CardType.Status) > 0)
					{
						return 1f;
					}
				}
				return 0f;
			}
		}
	}
}
