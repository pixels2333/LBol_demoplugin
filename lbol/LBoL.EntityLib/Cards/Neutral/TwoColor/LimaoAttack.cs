using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x02000298 RID: 664
	[UsedImplicitly]
	public sealed class LimaoAttack : Card
	{
		// Token: 0x17000131 RID: 305
		// (get) Token: 0x06000A5F RID: 2655 RVA: 0x00015A17 File Offset: 0x00013C17
		[UsedImplicitly]
		public ManaGroup GainMana
		{
			get
			{
				return base.Mana * (base.GrowCount + 1);
			}
		}

		// Token: 0x06000A60 RID: 2656 RVA: 0x00015A2C File Offset: 0x00013C2C
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			yield return new GainManaAction(this.GainMana);
			yield break;
		}
	}
}
