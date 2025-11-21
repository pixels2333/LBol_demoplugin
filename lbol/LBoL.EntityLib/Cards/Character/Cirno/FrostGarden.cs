using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004B9 RID: 1209
	[UsedImplicitly]
	public sealed class FrostGarden : Card
	{
		// Token: 0x170001C0 RID: 448
		// (get) Token: 0x06001008 RID: 4104 RVA: 0x0001C6FA File Offset: 0x0001A8FA
		public override bool Triggered
		{
			get
			{
				return this.IsForceCost;
			}
		}

		// Token: 0x170001C1 RID: 449
		// (get) Token: 0x06001009 RID: 4105 RVA: 0x0001C702 File Offset: 0x0001A902
		public override bool IsForceCost
		{
			get
			{
				if (base.Battle != null)
				{
					return Enumerable.Any<Card>(base.Battle.HandZone, (Card card) => card.CardType == CardType.Friend && card.Summoned);
				}
				return false;
			}
		}

		// Token: 0x0600100A RID: 4106 RVA: 0x0001C73D File Offset: 0x0001A93D
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			foreach (BattleAction battleAction in base.DebuffAction<Cold>(base.Battle.AllAliveEnemies, 0, 0, 0, 0, true, 0.1f))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			if (this.IsUpgraded)
			{
				yield return base.BuffAction<NextTurnColdAll>(1, 0, 0, 0, 0.2f);
			}
			yield break;
			yield break;
		}
	}
}
