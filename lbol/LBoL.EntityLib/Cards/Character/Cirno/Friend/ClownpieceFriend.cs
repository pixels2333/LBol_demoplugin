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
using LBoL.EntityLib.Cards.Enemy;
namespace LBoL.EntityLib.Cards.Character.Cirno.Friend
{
	[UsedImplicitly]
	public sealed class ClownpieceFriend : Card
	{
		[UsedImplicitly]
		public ManaGroup StartMana
		{
			get
			{
				return ManaGroup.Philosophies(3);
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
				yield return base.BuffAction<TempFirepower>(base.Value2, 0, 0, 0, 0.2f);
				num = i;
			}
			yield break;
		}
		protected override IEnumerable<BattleAction> SummonActions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainManaAction(this.StartMana);
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
			if (precondition == null || ((MiniSelectCardInteraction)precondition).SelectedCard.FriendToken == FriendToken.Active)
			{
				base.Loyalty += base.ActiveCost;
				yield return base.SkillAnime;
				yield return new DrawManyCardAction(base.Value1);
				if (!this.IsUpgraded)
				{
					yield return new AddCardsToDiscardAction(new Card[] { Library.CreateCard<Lunatic>() });
				}
			}
			else
			{
				base.Loyalty += base.ActiveCost2;
				yield return base.SkillAnime;
				yield return new GainManaAction(base.Mana);
				yield return new AddCardsToDiscardAction(new Card[] { Library.CreateCard<Lunatic>() });
			}
			yield break;
		}
	}
}
