using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004D0 RID: 1232
	[UsedImplicitly]
	public sealed class SummerFlower : Card
	{
		// Token: 0x170001CD RID: 461
		// (get) Token: 0x06001054 RID: 4180 RVA: 0x0001CE16 File Offset: 0x0001B016
		protected override int AdditionalDamage
		{
			get
			{
				if (base.Battle == null)
				{
					return 0;
				}
				return base.GetSeLevel<SummerFlowerSe>();
			}
		}

		// Token: 0x06001055 RID: 4181 RVA: 0x0001CE28 File Offset: 0x0001B028
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return base.BuffAction<SummerFlowerSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
