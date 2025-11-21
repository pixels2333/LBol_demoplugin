using System;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200017F RID: 383
	[UsedImplicitly]
	[ExhibitInfo(WeighterType = typeof(MaorongWanju.MaorongWanjuWeighter))]
	public sealed class MaorongWanju : Exhibit
	{
		// Token: 0x06000560 RID: 1376 RVA: 0x0000D27B File Offset: 0x0000B47B
		protected override void OnAdded(PlayerUnit player)
		{
			base.GameRun.ExtraLoyalty += base.Value1;
		}

		// Token: 0x06000561 RID: 1377 RVA: 0x0000D295 File Offset: 0x0000B495
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.ExtraLoyalty -= base.Value1;
		}

		// Token: 0x0200064E RID: 1614
		private class MaorongWanjuWeighter : IExhibitWeighter
		{
			// Token: 0x060019BB RID: 6587 RVA: 0x00034A8F File Offset: 0x00032C8F
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)(Enumerable.Any<Card>(gameRun.BaseDeck, (Card card) => card.CardType == CardType.Friend) ? 1 : 0);
			}
		}
	}
}
