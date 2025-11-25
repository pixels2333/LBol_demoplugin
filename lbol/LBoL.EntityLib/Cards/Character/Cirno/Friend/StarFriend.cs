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
	[UsedImplicitly]
	public sealed class StarFriend : Card
	{
		[UsedImplicitly]
		public int Light
		{
			get
			{
				return 1;
			}
		}
		public override IEnumerable<BattleAction> OnTurnEndingInHand()
		{
			return this.GetPassiveActions();
		}
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
		public static string PassiveGunName = "StarPasNoAni";
	}
}
