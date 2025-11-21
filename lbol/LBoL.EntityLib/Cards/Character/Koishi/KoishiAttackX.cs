using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x0200046A RID: 1130
	[UsedImplicitly]
	public sealed class KoishiAttackX : Card
	{
		// Token: 0x06000F3B RID: 3899 RVA: 0x0001B610 File Offset: 0x00019810
		public override ManaGroup GetXCostFromPooled(ManaGroup pooledMana)
		{
			if (pooledMana.Amount % 2 == 0)
			{
				return pooledMana;
			}
			if (pooledMana.Philosophy > 0)
			{
				return pooledMana - ManaGroup.Single(ManaColor.Philosophy);
			}
			foreach (ManaColor manaColor in ManaColors.WUBRGC)
			{
				if (pooledMana.GetValue(manaColor) > 0)
				{
					return pooledMana - ManaGroup.Single(manaColor);
				}
			}
			return pooledMana;
		}

		// Token: 0x06000F3C RID: 3900 RVA: 0x0001B698 File Offset: 0x00019898
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			int amount = base.SynergyAmount(consumingMana, ManaColor.Any, 2);
			if (amount > 0)
			{
				Guns guns = new Guns(base.GunName, amount, true);
				foreach (GunPair gunPair in guns.GunPairs)
				{
					yield return base.AttackAction(selector, gunPair);
				}
				List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
				if (base.Battle.BattleShouldEnd)
				{
					yield break;
				}
				yield return new GainManaAction(base.Mana * amount);
			}
			yield break;
			yield break;
		}
	}
}
