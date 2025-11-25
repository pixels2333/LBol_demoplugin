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
	public sealed class MarisaUltR : UltimateSkill
	{
		public MarisaUltR()
		{
			base.TargetType = TargetType.AllEnemies;
			base.GunName = "MasterSpark";
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			yield return new DamageAction(base.Owner, selector.GetUnits(base.Battle), this.Damage, base.GunName, GunType.Single);
			yield break;
		}
	}
}
