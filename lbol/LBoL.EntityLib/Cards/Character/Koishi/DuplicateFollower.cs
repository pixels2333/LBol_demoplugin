using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using UnityEngine;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x0200045F RID: 1119
	[UsedImplicitly]
	public sealed class DuplicateFollower : Card
	{
		// Token: 0x06000F1E RID: 3870 RVA: 0x0001B45A File Offset: 0x0001965A
		protected override string GetBaseDescription()
		{
			if (!base.IsCopy)
			{
				return base.GetBaseDescription();
			}
			return base.GetExtraDescription1;
		}

		// Token: 0x06000F1F RID: 3871 RVA: 0x0001B471 File Offset: 0x00019671
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}

		// Token: 0x06000F20 RID: 3872 RVA: 0x0001B490 File Offset: 0x00019690
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (!base.IsCopy)
			{
				List<Card> cards = new List<Card>();
				for (int i = 0; i < base.Value1; i++)
				{
					Card card = base.CloneBattleCard();
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

		// Token: 0x06000F21 RID: 3873 RVA: 0x0001B4A0 File Offset: 0x000196A0
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new GainManaAction(base.Mana);
			yield break;
		}
	}
}
