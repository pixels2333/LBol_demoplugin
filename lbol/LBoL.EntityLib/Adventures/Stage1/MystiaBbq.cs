using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Adventures;
namespace LBoL.EntityLib.Adventures.Stage1
{
	[AdventureInfo(WeighterType = typeof(MystiaBbq.MystiaBbqWeighter))]
	public sealed class MystiaBbq : Adventure
	{
		[RuntimeCommand("commitPay", "")]
		[UsedImplicitly]
		public void CommitPay()
		{
			base.GameRun.AchievementHandler.IncreaseStats(ProfileStatsKey.PayMoney);
		}
		[RuntimeCommand("commitCrime", "")]
		[UsedImplicitly]
		public void CommitCrime()
		{
			base.GameRun.ExtraFlags.Add("MystiaSin");
		}
		private class MystiaBbqWeighter : IAdventureWeighter
		{
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)((gameRun.Player.MaxHp - gameRun.Player.Hp >= 20 || gameRun.Player.Power < gameRun.Player.MaxPower) ? 1 : 0);
			}
		}
	}
}
