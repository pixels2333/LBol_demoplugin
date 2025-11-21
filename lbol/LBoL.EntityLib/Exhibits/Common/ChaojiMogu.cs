using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200015D RID: 349
	[UsedImplicitly]
	[ExhibitInfo(WeighterType = typeof(ChaojiMogu.ChaojiMoguWeighter))]
	public sealed class ChaojiMogu : Exhibit
	{
		// Token: 0x060004CC RID: 1228 RVA: 0x0000C4C5 File Offset: 0x0000A6C5
		protected override void OnGain(PlayerUnit player)
		{
			base.GameRun.GainPower(base.Value1, false);
		}

		// Token: 0x060004CD RID: 1229 RVA: 0x0000C4D9 File Offset: 0x0000A6D9
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}

		// Token: 0x02000635 RID: 1589
		private class ChaojiMoguWeighter : IExhibitWeighter
		{
			// Token: 0x06001917 RID: 6423 RVA: 0x000332B3 File Offset: 0x000314B3
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)((gameRun.Player.Power <= gameRun.Player.MaxPower / 2) ? 1 : 0);
			}
		}
	}
}
