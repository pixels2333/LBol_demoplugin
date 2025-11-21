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
	// Token: 0x0200051C RID: 1308
	[AdventureInfo(WeighterType = typeof(YoumuDelivery.YoumuDeliveryWeighter))]
	public sealed class YoumuDelivery : Adventure
	{
		// Token: 0x06001129 RID: 4393 RVA: 0x0001FB28 File Offset: 0x0001DD28
		protected override void InitVariables(IVariableStorage storage)
		{
			Card card = base.GameRun.RollTransformCard(base.GameRun.AdventureRng, new CardWeightTable(RarityWeightTable.EliteCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), false, false, (CardConfig config) => config.Colors.Count > 1);
			storage.SetValue("$transformCard", card.Id);
		}

		// Token: 0x0600112A RID: 4394 RVA: 0x0001FB93 File Offset: 0x0001DD93
		[RuntimeCommand("gainTuanzi", "")]
		[UsedImplicitly]
		public void GainTuanzi()
		{
			base.GameRun.ExtraFlags.Add("YoumuTuanzi");
		}

		// Token: 0x0600112B RID: 4395 RVA: 0x0001FBAB File Offset: 0x0001DDAB
		[RuntimeCommand("tagYoumuMooncake", "")]
		[UsedImplicitly]
		public void TagYoumuMooncake()
		{
			base.GameRun.ExtraFlags.Add("YoumuMooncake");
		}

		// Token: 0x02000A75 RID: 2677
		private class YoumuDeliveryWeighter : IAdventureWeighter
		{
			// Token: 0x06003771 RID: 14193 RVA: 0x00086A05 File Offset: 0x00084C05
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
