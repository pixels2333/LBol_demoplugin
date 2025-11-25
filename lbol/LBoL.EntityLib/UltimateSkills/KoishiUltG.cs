using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Koishi;
namespace LBoL.EntityLib.UltimateSkills
{
	[UsedImplicitly]
	public sealed class KoishiUltG : UltimateSkill
	{
		public KoishiUltG()
		{
			base.TargetType = TargetType.SingleEnemy;
			base.GunName = "生命的本源";
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			EnemyUnit enemy = selector.GetEnemy(base.Battle);
			yield return new DamageAction(base.Owner, enemy, this.Damage, base.GunName, GunType.Single);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new AddCardsToHandAction(Library.CreateCards<KoishiUltimateToken>(base.Value1, false), AddCardsType.Normal);
			yield break;
		}
	}
}
