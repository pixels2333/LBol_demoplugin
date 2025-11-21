using System;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000177 RID: 375
	[UsedImplicitly]
	[ExhibitInfo(WeighterType = typeof(JingboliJing.JingboliJingWeighter))]
	public sealed class JingboliJing : Exhibit
	{
		// Token: 0x0600053A RID: 1338 RVA: 0x0000CF05 File Offset: 0x0000B105
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<ScryEventArgs>(base.Battle.Scrying, delegate(ScryEventArgs args)
			{
				args.ScryInfo = args.ScryInfo.IncreasedBy(base.Value1);
				args.AddModifier(this);
			});
		}

		// Token: 0x02000648 RID: 1608
		private class JingboliJingWeighter : IExhibitWeighter
		{
			// Token: 0x06001996 RID: 6550 RVA: 0x0003457F File Offset: 0x0003277F
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)(Enumerable.Any<Card>(gameRun.BaseDeck, (Card card) => card.ConfigRelativeKeywords.HasFlag(Keyword.Scry)) ? 1 : 0);
			}
		}
	}
}
