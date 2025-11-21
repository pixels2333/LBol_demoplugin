using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004BF RID: 1215
	[UsedImplicitly]
	public sealed class HualiIce : Card
	{
		// Token: 0x170001C5 RID: 453
		// (get) Token: 0x0600101E RID: 4126 RVA: 0x0001C920 File Offset: 0x0001AB20
		[UsedImplicitly]
		public int Countdown
		{
			get
			{
				if (base.Battle != null)
				{
					return Math.Max(base.Value1 - base.Battle.TurnCardUsageHistory.Count, 0);
				}
				return 0;
			}
		}

		// Token: 0x170001C6 RID: 454
		// (get) Token: 0x0600101F RID: 4127 RVA: 0x0001C949 File Offset: 0x0001AB49
		public override bool Triggered
		{
			get
			{
				return this.IsForceCost;
			}
		}

		// Token: 0x170001C7 RID: 455
		// (get) Token: 0x06001020 RID: 4128 RVA: 0x0001C951 File Offset: 0x0001AB51
		public override bool IsForceCost
		{
			get
			{
				return base.Battle != null && this.Countdown == 0;
			}
		}

		// Token: 0x06001021 RID: 4129 RVA: 0x0001C966 File Offset: 0x0001AB66
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector.GetUnits(base.Battle));
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			foreach (BattleAction battleAction in base.DebuffAction<Cold>(base.Battle.AllAliveEnemies, 0, 0, 0, 0, true, 0.03f))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x06001022 RID: 4130 RVA: 0x0001C97D File Offset: 0x0001AB7D
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, delegate(CardUsingEventArgs _)
			{
				if (base.Zone == CardZone.Hand)
				{
					this.NotifyChanged();
				}
			}, (GameEventPriority)0);
		}
	}
}
