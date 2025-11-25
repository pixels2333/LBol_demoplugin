using System;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using Yarn;
namespace LBoL.EntityLib.Adventures.Shared12
{
	[AdventureInfo(WeighterType = typeof(YoumuDelivery.YoumuDeliveryWeighter))]
	public sealed class YoumuDelivery : Adventure
	{
		protected override void InitVariables(IVariableStorage storage)
		{
			Card card = base.GameRun.RollTransformCard(base.GameRun.AdventureRng, new CardWeightTable(RarityWeightTable.EliteCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), false, false, (CardConfig config) => config.Colors.Count > 1);
			storage.SetValue("$transformCard", card.Id);
		}
		[RuntimeCommand("gainTuanzi", "")]
		[UsedImplicitly]
		public void GainTuanzi()
		{
			base.GameRun.ExtraFlags.Add("YoumuTuanzi");
		}
		[RuntimeCommand("tagYoumuMooncake", "")]
		[UsedImplicitly]
		public void TagYoumuMooncake()
		{
			base.GameRun.ExtraFlags.Add("YoumuMooncake");
		}
		private class YoumuDeliveryWeighter : IAdventureWeighter
		{
			public float WeightFor(Type type, GameRunController gameRun)
			{
				if (Enumerable.Count<Card>(gameRun.BaseDeckWithoutUnremovable, (Card c) => c.CardType != CardType.Misfortune && c.CardType != CardType.Status) > 0)
				{
					return 1f;
				}
				return 0f;
			}
		}
	}
}
