using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Cirno.Friend
{
	// Token: 0x020004DF RID: 1247
	[UsedImplicitly]
	public sealed class SunnyFriend : Card
	{
		// Token: 0x170001D5 RID: 469
		// (get) Token: 0x0600108F RID: 4239 RVA: 0x0001D158 File Offset: 0x0001B358
		[UsedImplicitly]
		public int Light
		{
			get
			{
				return 1;
			}
		}

		// Token: 0x06001090 RID: 4240 RVA: 0x0001D15B File Offset: 0x0001B35B
		public override IEnumerable<BattleAction> OnTurnEndingInHand()
		{
			return this.GetPassiveActions();
		}

		// Token: 0x06001091 RID: 4241 RVA: 0x0001D163 File Offset: 0x0001B363
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
				yield return PerformAction.Effect(base.Battle.Player, "SunnyFairy", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				base.CardGuns = new Guns("SunnyPasNoAni", base.Value1, true);
				foreach (GunPair gunPair in base.CardGuns.GunPairs)
				{
					yield return base.AttackRandomAliveEnemyAction(gunPair);
				}
				List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
				num = i;
			}
			yield break;
			yield break;
		}

		// Token: 0x06001092 RID: 4242 RVA: 0x0001D173 File Offset: 0x0001B373
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition == null || ((MiniSelectCardInteraction)precondition).SelectedCard.FriendToken == FriendToken.Active)
			{
				base.Loyalty += base.ActiveCost;
				yield return base.SkillAnime;
				yield return base.BuffAction<GuangxueMicai>(0, this.Light, 0, 0, 0.2f);
			}
			else
			{
				base.Loyalty += base.UltimateCost;
				base.UltimateUsed = true;
				base.CardGuns = new Guns("SunnyPas", base.Value1, true);
				foreach (GunPair gunPair in base.CardGuns.GunPairs)
				{
					yield return base.AttackRandomAliveEnemyAction(gunPair);
				}
				List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
				if (!base.Battle.BattleShouldEnd)
				{
					yield return base.BuffAction<Firepower>(base.Value2, 0, 0, 0, 0.2f);
				}
			}
			yield break;
			yield break;
		}
	}
}
