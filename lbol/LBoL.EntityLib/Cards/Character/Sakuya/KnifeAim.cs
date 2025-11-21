using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Sakuya;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x02000392 RID: 914
	[UsedImplicitly]
	public sealed class KnifeAim : Card
	{
		// Token: 0x17000174 RID: 372
		// (get) Token: 0x06000D08 RID: 3336 RVA: 0x00018E69 File Offset: 0x00017069
		public override bool DiscardCard
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000D09 RID: 3337 RVA: 0x00018E6C File Offset: 0x0001706C
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			EnemyUnit enemy = selector.SelectedEnemy;
			yield return base.DebuffAction<LockedOn>(enemy, base.Value1, 0, 0, 0, true, 0.2f);
			List<Card> knifes = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card is Knife));
			if (knifes.NotEmpty<Card>())
			{
				if (enemy.IsAlive)
				{
					yield return base.DebuffAction<KnifeTarget>(enemy, 0, 0, 0, 0, true, 0.2f);
				}
				yield return new DiscardManyAction(knifes);
				if (enemy.IsAlive)
				{
					KnifeTarget statusEffect = enemy.GetStatusEffect<KnifeTarget>();
					if (statusEffect != null)
					{
						yield return new RemoveStatusEffectAction(statusEffect, true, 0.1f);
					}
				}
			}
			yield break;
		}
	}
}
