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
	[UsedImplicitly]
	public sealed class SummerFlower : Card
	{
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
