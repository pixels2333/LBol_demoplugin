using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Dolls
{
	[UsedImplicitly]
	public sealed class Shanghai : Doll
	{
		public Shanghai()
		{
			base.TargetType = TargetType.SingleEnemy;
			base.GunName = "Simple1";
		}
		public override int? DownCounter
		{
			get
			{
				return new int?(base.CalculateDamage(this.Damage));
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new DamageAction(base.Owner, base.Battle.FirstAliveEnemy, this.Damage, base.GunName, GunType.Single);
			yield break;
		}
	}
}
