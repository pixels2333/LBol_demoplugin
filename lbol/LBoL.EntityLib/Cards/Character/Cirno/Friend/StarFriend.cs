using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace LBoL.EntityLib.Cards.Character.Cirno.Friend
{
	// Token: 0x020004DE RID: 1246
	[UsedImplicitly]
	public sealed class StarFriend : Card
	{
		// Token: 0x170001D4 RID: 468
		// (get) Token: 0x06001089 RID: 4233 RVA: 0x0001D112 File Offset: 0x0001B312
		[UsedImplicitly]
		public int Light
		{
			get
			{
				return 1;
			}
		}

		// Token: 0x0600108A RID: 4234 RVA: 0x0001D115 File Offset: 0x0001B315
		public override IEnumerable<BattleAction> OnTurnEndingInHand()
		{
			return this.GetPassiveActions();
		}

		// Token: 0x0600108B RID: 4235 RVA: 0x0001D11D File Offset: 0x0001B31D
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
				yield return PerformAction.Effect(base.Battle.Player, "StarFairy", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				yield return base.AttackAction(base.Battle.AllAliveEnemies, StarFriend.PassiveGunName);
				num = i;
			}
			yield break;
		}

		// Token: 0x0600108C RID: 4236 RVA: 0x0001D12D File Offset: 0x0001B32D
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition == null || ((MiniSelectCardInteraction)precondition).SelectedCard.FriendToken == FriendToken.Active)
			{
				base.Loyalty += base.ActiveCost;
				base.CardGuns = new Guns(base.GunName, base.Value1, true);
				foreach (GunPair gunPair in base.CardGuns.GunPairs)
				{
					yield return base.AttackAllAliveEnemyAction(gunPair);
				}
				List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			}
			else
			{
				base.Loyalty += base.UltimateCost;
				base.UltimateUsed = true;
				yield return base.BuffAction<StarFriendSe>(this.Light, 0, 0, 0, 0.2f);
				base.CardGuns = new Guns("StarPas", base.Value2, true);
				foreach (GunPair gunPair2 in base.CardGuns.GunPairs)
				{
					yield return base.AttackAllAliveEnemyAction(gunPair2);
				}
				List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			}
			yield break;
			yield break;
		}

		// Token: 0x04000113 RID: 275
		public static string PassiveGunName = "StarPasNoAni";
	}
}
