using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Adventures;
namespace LBoL.EntityLib.Adventures.Stage1
{
	[AdventureInfo(WeighterType = typeof(RumiaDriving.RumiaDrivingWeighter))]
	public sealed class RumiaDriving : Adventure
	{
		[RuntimeCommand("damageByRumia", "")]
		[UsedImplicitly]
		public void DamageByRumia(int damage)
		{
			base.GameRun.Damage(damage, DamageType.HpLose, false, true, this);
			if (base.GameRun.Player.IsDead)
			{
				base.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.RumiaAdventure);
			}
		}
		private class RumiaDrivingWeighter : IAdventureWeighter
		{
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)((gameRun.Player.Power >= 40) ? 1 : 0);
			}
		}
	}
}
