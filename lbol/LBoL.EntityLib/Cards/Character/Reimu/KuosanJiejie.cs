using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003DD RID: 989
	[UsedImplicitly]
	public sealed class KuosanJiejie : Card
	{
		// Token: 0x06000DDF RID: 3551 RVA: 0x00019D24 File Offset: 0x00017F24
		public override ManaGroup GetXCostFromPooled(ManaGroup pooledMana)
		{
			return new ManaGroup
			{
				White = pooledMana.White,
				Red = pooledMana.Red,
				Philosophy = pooledMana.Philosophy
			};
		}

		// Token: 0x06000DE0 RID: 3552 RVA: 0x00019D64 File Offset: 0x00017F64
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			int white = base.SynergyAmount(consumingMana, ManaColor.White, 1);
			int red = base.SynergyAmount(consumingMana, ManaColor.Red, 1);
			if (white > 0)
			{
				bool flag = true;
				int num;
				for (int i = 0; i < white; i = num + 1)
				{
					yield return new CastBlockShieldAction(base.Battle.Player, base.Battle.Player, this.Block, flag);
					flag = false;
					num = i;
				}
			}
			if (red > 0)
			{
				Guns guns = new Guns(base.GunName, red, true);
				foreach (GunPair gunPair in guns.GunPairs)
				{
					yield return base.AttackAction(selector, gunPair);
					foreach (EnemyUnit enemyUnit in base.Battle.EnemyGroup.Alives)
					{
						yield return base.CardPerformAction(0, new Card.PerformTargetType?(Card.PerformTargetType.TargetSelf), enemyUnit);
						yield return base.CardPerformAction(1, new Card.PerformTargetType?(Card.PerformTargetType.Player), null);
					}
					IEnumerator<EnemyUnit> enumerator2 = null;
				}
				List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			}
			yield break;
			yield break;
		}
	}
}
