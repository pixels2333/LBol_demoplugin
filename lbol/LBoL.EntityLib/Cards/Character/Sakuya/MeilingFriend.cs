using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x0200039B RID: 923
	[UsedImplicitly]
	public sealed class MeilingFriend : Card
	{
		// Token: 0x1700017A RID: 378
		// (get) Token: 0x06000D29 RID: 3369 RVA: 0x00019026 File Offset: 0x00017226
		public int Graze
		{
			get
			{
				if (!this.IsUpgraded)
				{
					return 1;
				}
				return 2;
			}
		}

		// Token: 0x06000D2A RID: 3370 RVA: 0x00019033 File Offset: 0x00017233
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<StatisticalDamageEventArgs>(base.Battle.Player.StatisticalTotalDamageReceived, new EventSequencedReactor<StatisticalDamageEventArgs>(this.OnStatisticalTotalDamageReceived));
		}

		// Token: 0x06000D2B RID: 3371 RVA: 0x00019057 File Offset: 0x00017257
		public override IEnumerable<BattleAction> OnTurnStartedInHand()
		{
			return this.GetPassiveActions();
		}

		// Token: 0x06000D2C RID: 3372 RVA: 0x0001905F File Offset: 0x0001725F
		private IEnumerable<BattleAction> OnStatisticalTotalDamageReceived(StatisticalDamageEventArgs args)
		{
			if (base.Zone != CardZone.Hand || args.DamageSource == base.Battle.Player)
			{
				return null;
			}
			return this.GetPassiveActions();
		}

		// Token: 0x06000D2D RID: 3373 RVA: 0x00019085 File Offset: 0x00017285
		public override IEnumerable<BattleAction> GetPassiveActions()
		{
			if (!base.Summoned || base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			base.Loyalty += base.PassiveCost;
			int num;
			for (int i = 0; i < base.Battle.FriendPassiveTimes; i = num + 1)
			{
				if (base.Battle.BattleShouldEnd)
				{
					yield break;
				}
				yield return PerformAction.Sfx("FairySupport", 0f);
				yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<Knife>() });
				num = i;
			}
			yield break;
		}

		// Token: 0x06000D2E RID: 3374 RVA: 0x00019095 File Offset: 0x00017295
		protected override IEnumerable<BattleAction> SummonActions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Graze>(this.Graze, 0, 0, 0, 0.2f);
			foreach (BattleAction battleAction in base.SummonActions(selector, consumingMana, precondition))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x06000D2F RID: 3375 RVA: 0x000190BA File Offset: 0x000172BA
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			base.Loyalty += base.UltimateCost;
			base.UltimateUsed = true;
			yield return base.SpellAnime;
			yield return base.HealAction(base.Value1);
			yield return base.BuffAction<Firepower>(base.Value2, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
