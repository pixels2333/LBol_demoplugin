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
	[UsedImplicitly]
	public sealed class LilyFriend : Card
	{
		[UsedImplicitly]
		public ManaGroup TurnMana
		{
			get
			{
				return base.Mana;
			}
		}
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
		public override IEnumerable<BattleAction> OnTurnStartedInHand()
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
				yield return PerformAction.Effect(base.Battle.Player, "LilyFairy", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				yield return new GainManaAction(base.Mana);
				num = i;
			}
			yield break;
		}
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
