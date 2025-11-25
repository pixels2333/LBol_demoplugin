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
	public sealed class Penglai : Doll
	{
		public Penglai()
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
		protected override IEnumerable<BattleAction> ActiveActions()
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new DamageAction(base.Owner, base.Battle.LowestHpEnemy, this.Damage, "Simple1", GunType.Single);
			yield break;
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new DamageAction(base.Owner, base.Battle.LowestHpEnemy, this.Damage, "Simple1", GunType.Single);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new RemoveDollAction(this);
			yield break;
		}
		public override IEnumerable<BattleAction> OnRemove()
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Magic > 0)
			{
				yield return new GainManaAction(base.Mana * base.Magic);
			}
			yield break;
		}
	}
}
