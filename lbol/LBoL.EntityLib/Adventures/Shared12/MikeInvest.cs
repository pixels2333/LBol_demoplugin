using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.EntityLib.Cards.Adventure;
using Yarn;

namespace LBoL.EntityLib.Adventures.Shared12
{
	// Token: 0x0200051B RID: 1307
	[AdventureInfo(WeighterType = typeof(MikeInvest.MikeInvestWeighter))]
	[UsedImplicitly]
	public sealed class MikeInvest : Adventure
	{
		// Token: 0x06001127 RID: 4391 RVA: 0x0001FA8C File Offset: 0x0001DC8C
		protected override void InitVariables(IVariableStorage storage)
		{
			GainTreasure gainTreasure = LBoL.Core.Library.CreateCard<GainTreasure>();
			storage.SetValue("$gainTreasure", gainTreasure.Id);
			Card card = base.GameRun.RollCard(base.GameRun.AdventureRng, new CardWeightTable(RarityWeightTable.OnlyRare, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), false, false, null);
			storage.SetValue("$longMoney", (float)base.GameRun.AdventureRng.NextInt(30, 40));
			storage.SetValue("$shortMoney", 80f);
			storage.SetValue("$cardReward", card.Id);
		}

		// Token: 0x04000141 RID: 321
		private const int LongMoneyMin = 30;

		// Token: 0x04000142 RID: 322
		private const int LongMoneyMax = 40;

		// Token: 0x04000143 RID: 323
		private const int ShortMoney = 80;

		// Token: 0x02000A74 RID: 2676
		private class MikeInvestWeighter : IAdventureWeighter
		{
			// Token: 0x0600376F RID: 14191 RVA: 0x000869E6 File Offset: 0x00084BE6
			public float WeightFor(Type type, GameRunController gameRun)
			{
				if (gameRun.Money < 80)
				{
					return 0f;
				}
				return 1f;
			}
		}
	}
}
