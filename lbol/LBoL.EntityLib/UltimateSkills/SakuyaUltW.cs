using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Sakuya;

namespace LBoL.EntityLib.UltimateSkills
{
	// Token: 0x02000011 RID: 17
	[UsedImplicitly]
	public sealed class SakuyaUltW : UltimateSkill
	{
		// Token: 0x0600001E RID: 30 RVA: 0x000022F3 File Offset: 0x000004F3
		public SakuyaUltW()
		{
			base.TargetType = TargetType.SingleEnemy;
			base.GunName = "SakuyaSpell1";
		}

		// Token: 0x0600001F RID: 31 RVA: 0x0000230D File Offset: 0x0000050D
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			EnemyUnit enemy = selector.GetEnemy(base.Battle);
			yield return new DamageAction(base.Owner, enemy, this.Damage, base.GunName, GunType.Single);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new AddCardsToHandAction(Library.CreateCards<Knife>(base.Value1, false), AddCardsType.Normal);
			yield break;
		}
	}
}
