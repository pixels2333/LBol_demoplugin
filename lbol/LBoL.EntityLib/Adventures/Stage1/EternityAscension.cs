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
	[AdventureInfo(WeighterType = typeof(EternityAscension.EternityAscensionWeighter))]
	public sealed class EternityAscension : Adventure
	{
		protected override void InitVariables(IVariableStorage storage)
		{
			Card card = base.GameRun.RollTransformCard(base.GameRun.AdventureRng, new CardWeightTable(RarityWeightTable.OnlyUncommon, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), false, false, null);
			storage.SetValue("$transformCard", card.Id);
		}
		private class EternityAscensionWeighter : IAdventureWeighter
		{
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
