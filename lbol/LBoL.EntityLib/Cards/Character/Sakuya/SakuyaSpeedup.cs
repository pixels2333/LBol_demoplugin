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
	// Token: 0x020003B1 RID: 945
	[UsedImplicitly]
	public sealed class SakuyaSpeedup : Card
	{
		// Token: 0x1700017E RID: 382
		// (get) Token: 0x06000D65 RID: 3429 RVA: 0x0001946A File Offset: 0x0001766A
		public override ManaGroup? PlentifulMana
		{
			get
			{
				return new ManaGroup?(base.Mana);
			}
		}

		// Token: 0x06000D66 RID: 3430 RVA: 0x00019477 File Offset: 0x00017677
		protected override string GetBaseDescription()
		{
			if (!base.PlentifulHappenThisTurn)
			{
				return base.GetBaseDescription();
			}
			return base.GetExtraDescription1;
		}

		// Token: 0x06000D67 RID: 3431 RVA: 0x0001948E File Offset: 0x0001768E
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			TimeAuraSe statusEffect = base.Battle.Player.GetStatusEffect<TimeAuraSe>();
			if (statusEffect != null)
			{
				yield return base.BuffAction<TimeAuraSe>(statusEffect.Level, 0, 0, 0, 0.2f);
			}
			yield break;
		}
	}
}
