using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004A5 RID: 1189
	[UsedImplicitly]
	public sealed class CirnoFire : Card
	{
		// Token: 0x170001BB RID: 443
		// (get) Token: 0x06000FD4 RID: 4052 RVA: 0x0001C28B File Offset: 0x0001A48B
		protected override int AdditionalValue1
		{
			get
			{
				return this.ColdEnemies * base.Value2;
			}
		}

		// Token: 0x170001BC RID: 444
		// (get) Token: 0x06000FD5 RID: 4053 RVA: 0x0001C29A File Offset: 0x0001A49A
		private int ColdEnemies
		{
			get
			{
				if (base.Battle == null)
				{
					return 0;
				}
				return Enumerable.Count<EnemyUnit>(base.Battle.AllAliveEnemies, (EnemyUnit enemy) => enemy.HasStatusEffect<Cold>());
			}
		}

		// Token: 0x06000FD6 RID: 4054 RVA: 0x0001C2D5 File Offset: 0x0001A4D5
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Firepower>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
