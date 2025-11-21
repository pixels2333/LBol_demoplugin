using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Cirno.Friend
{
	// Token: 0x020004DB RID: 1243
	[UsedImplicitly]
	public sealed class LilyFriend : Card
	{
		// Token: 0x170001CF RID: 463
		// (get) Token: 0x06001076 RID: 4214 RVA: 0x0001CFEB File Offset: 0x0001B1EB
		[UsedImplicitly]
		public ManaGroup TurnMana
		{
			get
			{
				return base.Mana;
			}
		}

		// Token: 0x170001D0 RID: 464
		// (get) Token: 0x06001077 RID: 4215 RVA: 0x0001CFF3 File Offset: 0x0001B1F3
		[UsedImplicitly]
		public ManaGroup ActiveMana
		{
			get
			{
				if (!this.IsUpgraded)
				{
					return ManaGroup.Blues(1) + ManaGroup.Greens(2);
				}
				return ManaGroup.Philosophies(1) + ManaGroup.Greens(2);
			}
		}

		// Token: 0x170001D1 RID: 465
		// (get) Token: 0x06001078 RID: 4216 RVA: 0x0001D020 File Offset: 0x0001B220
		[UsedImplicitly]
		public int Heal
		{
			get
			{
				if (!this.IsUpgraded)
				{
					return 6;
				}
				return 8;
			}
		}

		// Token: 0x06001079 RID: 4217 RVA: 0x0001D02D File Offset: 0x0001B22D
		public override IEnumerable<BattleAction> OnTurnStartedInHand()
		{
			return this.GetPassiveActions();
		}

		// Token: 0x0600107A RID: 4218 RVA: 0x0001D035 File Offset: 0x0001B235
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
				yield return PerformAction.Effect(base.Battle.Player, "LilyFairy", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				yield return new GainManaAction(base.Mana);
				num = i;
			}
			yield break;
		}

		// Token: 0x0600107B RID: 4219 RVA: 0x0001D045 File Offset: 0x0001B245
		protected override IEnumerable<BattleAction> SummonActions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
			foreach (GunPair gunPair in base.CardGuns.GunPairs)
			{
				yield return base.AttackRandomAliveEnemyAction(gunPair);
			}
			List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			foreach (BattleAction battleAction in base.SummonActions(selector, consumingMana, precondition))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator2 = null;
			yield break;
			yield break;
		}

		// Token: 0x0600107C RID: 4220 RVA: 0x0001D06A File Offset: 0x0001B26A
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition == null || ((MiniSelectCardInteraction)precondition).SelectedCard.FriendToken == FriendToken.Active)
			{
				base.Loyalty += base.ActiveCost;
				yield return base.SkillAnime;
				yield return new GainManaAction(this.ActiveMana);
			}
			else
			{
				base.Loyalty += base.UltimateCost;
				base.UltimateUsed = true;
				yield return base.SkillAnime;
				yield return base.HealAction(this.Heal);
			}
			yield break;
		}
	}
}
