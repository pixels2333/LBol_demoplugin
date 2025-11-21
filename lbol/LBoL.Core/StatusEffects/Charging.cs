using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x02000090 RID: 144
	[UsedImplicitly]
	public sealed class Charging : StatusEffect
	{
		// Token: 0x06000741 RID: 1857 RVA: 0x00015903 File Offset: 0x00013B03
		public override IEnumerable<BattleAction> StackAction(Unit targetOwner, int targetLevel)
		{
			yield return new ApplyStatusEffectAction<Burst>(targetOwner, new int?(targetLevel), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x1700026E RID: 622
		// (get) Token: 0x06000742 RID: 1858 RVA: 0x0001591A File Offset: 0x00013B1A
		public override string UnitEffectName
		{
			get
			{
				return "MarisaChargingLoop";
			}
		}
	}
}
