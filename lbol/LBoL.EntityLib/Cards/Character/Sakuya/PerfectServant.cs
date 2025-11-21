using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Sakuya;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x0200039D RID: 925
	[UsedImplicitly]
	public sealed class PerfectServant : Card
	{
		// Token: 0x06000D34 RID: 3380 RVA: 0x000190F8 File Offset: 0x000172F8
		public override ManaGroup GetXCostFromPooled(ManaGroup pooledMana)
		{
			return new ManaGroup
			{
				White = pooledMana.White,
				Blue = pooledMana.Blue,
				Philosophy = pooledMana.Philosophy
			};
		}

		// Token: 0x06000D35 RID: 3381 RVA: 0x00019138 File Offset: 0x00017338
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<PerfectServantUSe>(base.SynergyAmount(consumingMana, ManaColor.Blue, 1) * base.Value2, 0, 0, 0, 0.2f);
			yield return base.BuffAction<PerfectServantWSe>(base.SynergyAmount(consumingMana, ManaColor.White, 2) * base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
