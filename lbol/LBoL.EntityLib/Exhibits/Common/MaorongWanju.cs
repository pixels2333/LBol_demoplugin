using System;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	[ExhibitInfo(WeighterType = typeof(MaorongWanju.MaorongWanjuWeighter))]
	public sealed class MaorongWanju : Exhibit
	{
		protected override void OnAdded(PlayerUnit player)
		{
			base.GameRun.ExtraLoyalty += base.Value1;
		}
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.ExtraLoyalty -= base.Value1;
		}
		private class MaorongWanjuWeighter : IExhibitWeighter
		{
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)(Enumerable.Any<Card>(gameRun.BaseDeck, (Card card) => card.CardType == CardType.Friend) ? 1 : 0);
			}
		}
	}
}
