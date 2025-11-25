using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Marisa
{
	[UsedImplicitly]
	public sealed class RainbowStar : Card
	{
		public override ManaGroup GetXCostFromPooled(ManaGroup pooledMana)
		{
			return pooledMana;
		}
		private DamageInfo CalculateDamage(ManaGroup? manaGroup)
		{
			if (manaGroup != null)
			{
				ManaGroup valueOrDefault = manaGroup.GetValueOrDefault();
				return DamageInfo.Attack((float)(base.RawDamage + base.SynergyAmount(valueOrDefault, ManaColor.Any, 1) * base.Value1 + base.SynergyAmount(valueOrDefault, ManaColor.Philosophy, 1) * base.Value2), base.IsAccuracy);
			}
			return DamageInfo.Attack((float)base.RawDamage, base.IsAccuracy);
		}
		[UsedImplicitly]
		public override DamageInfo Damage
		{
			get
			{
				return this.CalculateDamage(base.PendingManaUsage);
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, this.CalculateDamage(new ManaGroup?(consumingMana)), null);
			yield break;
		}
	}
}
