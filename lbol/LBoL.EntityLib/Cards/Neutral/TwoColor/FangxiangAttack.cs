using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Others;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x0200028B RID: 651
	[UsedImplicitly]
	public sealed class FangxiangAttack : Card
	{
		// Token: 0x06000A3A RID: 2618 RVA: 0x00015725 File Offset: 0x00013925
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (selector.SelectedEnemy.HasStatusEffect<Poison>())
			{
				base.CardGuns = new Guns(base.GunName, base.Value2, true);
				foreach (GunPair gunPair in base.CardGuns.GunPairs)
				{
					yield return base.AttackAction(selector, gunPair);
				}
				List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			}
			else
			{
				yield return base.DebuffAction<Poison>(selector.SelectedEnemy, base.Value1, 0, 0, 0, true, 0.2f);
			}
			yield break;
			yield break;
		}
	}
}
