using System;
using JetBrains.Annotations;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x0200009D RID: 157
	[UsedImplicitly]
	public sealed class EnemyEnergyNegative : StatusEffect, IOpposing<EnemyEnergy>
	{
		// Token: 0x06000234 RID: 564 RVA: 0x00006887 File Offset: 0x00004A87
		public OpposeResult Oppose(EnemyEnergy other)
		{
			other.Level = Math.Max(0, other.Level - base.Level);
			return OpposeResult.KeepOther;
		}
	}
}
