using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x020003A6 RID: 934
	[UsedImplicitly]
	public sealed class SakuyaDetective : Card
	{
		// Token: 0x1700017C RID: 380
		// (get) Token: 0x06000D49 RID: 3401 RVA: 0x000192BA File Offset: 0x000174BA
		[UsedImplicitly]
		public int BossDamage
		{
			get
			{
				GameRunController gameRun = base.GameRun;
				if (gameRun == null)
				{
					return 0;
				}
				return gameRun.FinalBossInitialDamage;
			}
		}

		// Token: 0x06000D4A RID: 3402 RVA: 0x000192CD File Offset: 0x000174CD
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			base.GameRun.FinalBossInitialDamage += base.Value1;
			yield return new DrawManyCardAction(base.Value2);
			yield break;
		}
	}
}
