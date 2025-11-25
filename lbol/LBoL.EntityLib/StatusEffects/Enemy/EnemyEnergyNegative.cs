using System;
using JetBrains.Annotations;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.StatusEffects.Enemy
{
	[UsedImplicitly]
	public sealed class EnemyEnergyNegative : StatusEffect, IOpposing<EnemyEnergy>
	{
		public OpposeResult Oppose(EnemyEnergy other)
		{
			other.Level = Math.Max(0, other.Level - base.Level);
			return OpposeResult.KeepOther;
		}
	}
}
