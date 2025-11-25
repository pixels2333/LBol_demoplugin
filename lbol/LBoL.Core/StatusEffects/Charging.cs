using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
namespace LBoL.Core.StatusEffects
{
	[UsedImplicitly]
	public sealed class Charging : StatusEffect
	{
		public override IEnumerable<BattleAction> StackAction(Unit targetOwner, int targetLevel)
		{
			yield return new ApplyStatusEffectAction<Burst>(targetOwner, new int?(targetLevel), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
		public override string UnitEffectName
		{
			get
			{
				return "MarisaChargingLoop";
			}
		}
	}
}
