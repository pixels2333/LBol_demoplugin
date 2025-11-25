using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.Colorless
{
	[UsedImplicitly]
	public sealed class FengjiAttack : Card
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
				return DamageInfo.Attack((float)(base.RawDamage + base.SynergyAmount(valueOrDefault, ManaColor.Colorless, 1) * base.Value1), base.IsAccuracy);
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
			base.CardGuns = new Guns(base.GunName, base.Value2, true);
			foreach (GunPair gunPair in base.CardGuns.GunPairs)
			{
				yield return base.AttackAction(selector, this.CalculateDamage(new ManaGroup?(consumingMana)), gunPair);
			}
			List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			yield break;
			yield break;
		}
	}
}
