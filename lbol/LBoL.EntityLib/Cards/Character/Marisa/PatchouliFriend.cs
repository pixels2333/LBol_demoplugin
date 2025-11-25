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
namespace LBoL.EntityLib.Cards.Character.Marisa
{
	[UsedImplicitly]
	public sealed class PatchouliFriend : Card
	{
		[UsedImplicitly]
		public int Power
		{
			get
			{
				if (!this.IsUpgraded)
				{
					return 3;
				}
				return 5;
			}
		}
		[UsedImplicitly]
		public int PassiveColor
		{
			get
			{
				return base.PassiveCost;
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
				yield return ConvertManaAction.PhilosophyRandomMana(base.Battle.BattleMana, this.PassiveColor, base.GameRun.BattleRng);
				num = i;
			}
			yield break;
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition == null || ((MiniSelectCardInteraction)precondition).SelectedCard.FriendToken == FriendToken.Active)
			{
				base.Loyalty += base.ActiveCost;
				yield return base.SkillAnime;
				yield return base.BuffAction<Firepower>(base.Value1, 0, 0, 0, 0.2f);
				yield return base.BuffAction<Spirit>(base.Value1, 0, 0, 0, 0.2f);
				yield return new GainPowerAction(this.Power);
			}
			else
			{
				base.Loyalty += base.ActiveCost2;
				yield return base.SkillAnime;
				yield return new AddCardsToHandAction(Library.CreateCards<Potion>(base.Value2, false), AddCardsType.Normal);
			}
			yield break;
		}
	}
}
