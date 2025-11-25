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
	[UsedImplicitly]
	public sealed class SuikaBigball : Card
	{
		public DamageInfo HalfDamage
		{
			get
			{
				return this.Damage.MultiplyBy(0.5f);
			}
		}
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
