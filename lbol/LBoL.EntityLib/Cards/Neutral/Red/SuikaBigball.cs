using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Cards.Neutral.Red
{
	// Token: 0x020002D3 RID: 723
	[UsedImplicitly]
	public sealed class SuikaBigball : Card
	{
		// Token: 0x17000142 RID: 322
		// (get) Token: 0x06000B01 RID: 2817 RVA: 0x00016694 File Offset: 0x00014894
		public DamageInfo HalfDamage
		{
			get
			{
				return this.Damage.MultiplyBy(0.5f);
			}
		}

		// Token: 0x06000B02 RID: 2818 RVA: 0x000166B4 File Offset: 0x000148B4
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			EnemyUnit target = selector.GetEnemy(base.Battle);
			yield return base.AttackAction(target);
			List<Unit> list = Enumerable.ToList<Unit>(Enumerable.Cast<Unit>(Enumerable.Where<EnemyUnit>(base.Battle.EnemyGroup.Alives, (EnemyUnit enemy) => enemy != target)));
			yield return base.AttackAction(list, "Instant", this.HalfDamage);
			yield return base.DebuffAction<FirepowerNegative>(base.Battle.Player, base.Value1, 0, 0, 0, true, 0.2f);
			yield break;
		}
	}
}
