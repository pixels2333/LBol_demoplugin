using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Marisa;

namespace LBoL.EntityLib.UltimateSkills
{
	// Token: 0x0200000A RID: 10
	[UsedImplicitly]
	public sealed class MarisaUltB : UltimateSkill
	{
		// Token: 0x06000011 RID: 17 RVA: 0x000021F2 File Offset: 0x000003F2
		public MarisaUltB()
		{
			base.TargetType = TargetType.SingleEnemy;
			base.GunName = "魔药Spell";
		}

		// Token: 0x06000012 RID: 18 RVA: 0x0000220C File Offset: 0x0000040C
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			EnemyUnit enemy = selector.GetEnemy(base.Battle);
			yield return new DamageAction(base.Owner, enemy, this.Damage, base.GunName, GunType.Single);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<Potion>(base.Value1, false), DrawZoneTarget.Random, AddCardsType.Normal);
			yield break;
		}
	}
}
