using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Adventures;

namespace LBoL.EntityLib.Adventures.Stage1
{
	// Token: 0x0200050F RID: 1295
	[AdventureInfo(WeighterType = typeof(MystiaBbq.MystiaBbqWeighter))]
	public sealed class MystiaBbq : Adventure
	{
		// Token: 0x06001103 RID: 4355 RVA: 0x0001EB28 File Offset: 0x0001CD28
		[RuntimeCommand("commitPay", "")]
		[UsedImplicitly]
		public void CommitPay()
		{
			base.GameRun.AchievementHandler.IncreaseStats(ProfileStatsKey.PayMoney);
		}

		// Token: 0x06001104 RID: 4356 RVA: 0x0001EB3B File Offset: 0x0001CD3B
		[RuntimeCommand("commitCrime", "")]
		[UsedImplicitly]
		public void CommitCrime()
		{
			base.GameRun.ExtraFlags.Add("MystiaSin");
		}

		// Token: 0x02000A67 RID: 2663
		private class MystiaBbqWeighter : IAdventureWeighter
		{
			// Token: 0x06003749 RID: 14153 RVA: 0x00086572 File Offset: 0x00084772
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)((gameRun.Player.MaxHp - gameRun.Player.Hp >= 20 || gameRun.Player.Power < gameRun.Player.MaxPower) ? 1 : 0);
			}
		}
	}
}
