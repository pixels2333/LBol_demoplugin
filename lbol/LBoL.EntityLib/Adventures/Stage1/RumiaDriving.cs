using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Adventures;

namespace LBoL.EntityLib.Adventures.Stage1
{
	// Token: 0x02000511 RID: 1297
	[AdventureInfo(WeighterType = typeof(RumiaDriving.RumiaDrivingWeighter))]
	public sealed class RumiaDriving : Adventure
	{
		// Token: 0x06001109 RID: 4361 RVA: 0x0001EC93 File Offset: 0x0001CE93
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

		// Token: 0x02000A6A RID: 2666
		private class RumiaDrivingWeighter : IAdventureWeighter
		{
			// Token: 0x06003751 RID: 14161 RVA: 0x0008661F File Offset: 0x0008481F
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)((gameRun.Player.Power >= 40) ? 1 : 0);
			}
		}
	}
}
