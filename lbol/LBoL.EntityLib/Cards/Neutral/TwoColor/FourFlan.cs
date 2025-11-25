using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using UnityEngine;
namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	[UsedImplicitly]
	public sealed class FourFlan : Card
	{
		protected override string GetBaseDescription()
		{
			if (!base.IsCopy)
			{
				return base.GetBaseDescription();
			}
			return base.GetExtraDescription1;
		}
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (!base.IsCopy)
			{
				List<Card> cards = new List<Card>();
				for (int i = 0; i < base.Value2; i++)
				{
					Card card = base.CloneBattleCard();
					card.SetTurnCost(base.Mana);
					card.IsExile = true;
					card.IsEthereal = true;
					cards.Add(card);
				}
				if (base.Zone == CardZone.Draw)
				{
					yield return PerformAction.ViewCard(this);
				}
				else
				{
					Debug.LogError("Triggering BattleStarted while not in draw zone");
				}
				yield return new AddCardsToDrawZoneAction(cards, DrawZoneTarget.Random, AddCardsType.Normal);
				cards = null;
			}
			yield break;
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return base.BuffAction<Firepower>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
