using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Koishi;

namespace LBoL.EntityLib.UltimateSkills
{
	// Token: 0x02000009 RID: 9
	[UsedImplicitly]
	public sealed class KoishiUltG : UltimateSkill
	{
		// Token: 0x0600000F RID: 15 RVA: 0x000021C1 File Offset: 0x000003C1
		public KoishiUltG()
		{
			base.TargetType = TargetType.SingleEnemy;
			base.GunName = "生命的本源";
		}

		// Token: 0x06000010 RID: 16 RVA: 0x000021DB File Offset: 0x000003DB
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			EnemyUnit enemy = selector.GetEnemy(base.Battle);
			yield return new DamageAction(base.Owner, enemy, this.Damage, base.GunName, GunType.Single);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new AddCardsToHandAction(Library.CreateCards<KoishiUltimateToken>(base.Value1, false), AddCardsType.Normal);
			yield break;
		}
	}
}
