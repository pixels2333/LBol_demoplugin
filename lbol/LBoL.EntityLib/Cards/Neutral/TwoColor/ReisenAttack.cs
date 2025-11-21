using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x020002A3 RID: 675
	[UsedImplicitly]
	public sealed class ReisenAttack : Card
	{
		// Token: 0x06000A7E RID: 2686 RVA: 0x00015C57 File Offset: 0x00013E57
		public override ManaGroup GetXCostFromPooled(ManaGroup pooledMana)
		{
			return pooledMana;
		}

		// Token: 0x06000A7F RID: 2687 RVA: 0x00015C5A File Offset: 0x00013E5A
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			int num = base.SynergyAmount(consumingMana, ManaColor.Any, 1);
			if (num > 0)
			{
				Guns guns = new Guns(base.GunName, num, true);
				foreach (GunPair gunPair in guns.GunPairs)
				{
					yield return base.AttackAction(selector, gunPair);
				}
				List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			}
			yield break;
			yield break;
		}
	}
}
