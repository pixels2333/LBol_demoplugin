using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
namespace LBoL.EntityLib.UltimateSkills
{
	[UsedImplicitly]
	public sealed class CirnoUltG : UltimateSkill
	{
		[UsedImplicitly]
		public ManaGroup GainMana
		{
			get
			{
				return ManaGroup.Philosophies((base.GameRun != null) ? (base.GameRun.UltimateUseCount + 1) : 1);
			}
		}
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Philosophies(1);
			}
		}
		public CirnoUltG()
		{
			base.TargetType = TargetType.AllEnemies;
			base.GunName = "冰晶绽放";
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			yield return new DamageAction(base.Owner, selector.GetEnemies(base.Battle), this.Damage, base.GunName, GunType.Single);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new GainManaAction(this.GainMana);
			yield break;
		}
	}
}
