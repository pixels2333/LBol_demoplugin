using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004AB RID: 1195
	[UsedImplicitly]
	public sealed class CoolBreeze : Card
	{
		// Token: 0x170001BE RID: 446
		// (get) Token: 0x06000FE5 RID: 4069 RVA: 0x0001C3FE File Offset: 0x0001A5FE
		public override bool Triggered
		{
			get
			{
				if (base.Battle != null)
				{
					return Enumerable.Any<EnemyUnit>(base.Battle.AllAliveEnemies, (EnemyUnit enemy) => enemy.HasStatusEffect<Cold>());
				}
				return false;
			}
		}

		// Token: 0x06000FE6 RID: 4070 RVA: 0x0001C439 File Offset: 0x0001A639
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			if (base.TriggeredAnyhow)
			{
				yield return base.UpgradeRandomHandAction(base.Value1, CardType.Unknown);
			}
			yield break;
		}
	}
}
