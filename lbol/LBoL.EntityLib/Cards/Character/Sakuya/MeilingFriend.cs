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
	[UsedImplicitly]
	public sealed class MeilingFriend : Card
	{
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
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<StatisticalDamageEventArgs>(base.Battle.Player.StatisticalTotalDamageReceived, new EventSequencedReactor<StatisticalDamageEventArgs>(this.OnStatisticalTotalDamageReceived));
		}
		public override IEnumerable<BattleAction> OnTurnStartedInHand()
		{
			return this.GetPassiveActions();
		}
		private IEnumerable<BattleAction> OnStatisticalTotalDamageReceived(StatisticalDamageEventArgs args)
		{
			if (base.Zone != CardZone.Hand || args.DamageSource == base.Battle.Player)
			{
				return null;
			}
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
				yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<Knife>() });
				num = i;
			}
			yield break;
		}
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
