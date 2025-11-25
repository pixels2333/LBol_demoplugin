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
	[AdventureInfo(WeighterType = typeof(MikeInvest.MikeInvestWeighter))]
	[UsedImplicitly]
	public sealed class MikeInvest : Adventure
	{
		protected override void InitVariables(IVariableStorage storage)
		{
			GainTreasure gainTreasure = LBoL.Core.Library.CreateCard<GainTreasure>();
			storage.SetValue("$gainTreasure", gainTreasure.Id);
			Card card = base.GameRun.RollCard(base.GameRun.AdventureRng, new CardWeightTable(RarityWeightTable.OnlyRare, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), false, false, null);
			storage.SetValue("$longMoney", (float)base.GameRun.AdventureRng.NextInt(30, 40));
			storage.SetValue("$shortMoney", 80f);
			storage.SetValue("$cardReward", card.Id);
		}
		private const int LongMoneyMin = 30;
		private const int LongMoneyMax = 40;
		private const int ShortMoney = 80;
		private class MikeInvestWeighter : IAdventureWeighter
		{
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
